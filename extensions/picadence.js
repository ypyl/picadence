// Reports one pi session's state to the tray.
//
// The tray reads a small JSON file per session from %LOCALAPPDATA%\picadence. This
// file is the writer half of that contract, and it is plain ESM so the same file
// can be tested with `node --test extensions`.
//
// The state names are pi's own, from the terminal status protocol pi already
// emits: idle, working, blocked, done, error. `blocked` means pi is waiting on
// you, and `done` means a run finished and pi will not continue on its own.
//
// Reporting must never break a session: every handler catches its own errors.

import fs from "node:fs";
import os from "node:os";
import path from "node:path";

export const STATES = Object.freeze(["idle", "working", "blocked", "done", "error"]);

/** A session id is only useful as a file name when it is already plain. */
export function isPlainSessionId(value) {
  return typeof value === "string" && value.length > 0 && /^[A-Za-z0-9-]+$/.test(value);
}

export function fileNameFor(pid, sessionId) {
  if (!Number.isInteger(pid) || pid <= 0) {
    throw new Error(`picadence: process id must be a positive integer, got ${pid}`);
  }
  if (!isPlainSessionId(sessionId)) {
    throw new Error(`picadence: session id is not a plain stem: ${JSON.stringify(sessionId)}`);
  }
  return `${pid}-${sessionId}.json`;
}

/** The directory the tray reads. Undefined when the platform does not name one. */
export function stateDirectory(env = process.env) {
  const base = env.LOCALAPPDATA || path.join(os.homedir(), "AppData", "Local");
  if (!base) {
    return undefined;
  }
  return path.join(base, "picadence", "agents");
}

/**
 * The directory the tray writes a ping into, beside the state it reads. One file
 * per session, named by session id, which is a plain stem on both sides.
 */
export function pingDirectory(env = process.env) {
  const agents = stateDirectory(env);
  return agents ? path.join(path.dirname(agents), "pings") : undefined;
}

// shortcut: a ping lands up to ten seconds after the tray writes it, poll faster
// or switch to fs.watch when a cadence ever needs second precision
/** How often a session looks for a ping. */
export const PingPollMilliseconds = 10_000;

/** A ping older than this is not worth acting on: the tray may have died. */
export const PingMaxAgeMilliseconds = 5 * 60 * 1000;

/**
 * The one decision about a ping: send it, or drop it and say why. Pure, so every
 * branch is a test rather than an observation.
 */
export function pingAction(state, ping, now = Date.now()) {
  if (!ping || typeof ping.prompt !== "string" || ping.prompt.length === 0) {
    return { action: "skip", reason: "unreadable" };
  }

  const at = typeof ping.at === "string" ? Date.parse(ping.at) : Number.NaN;
  if (!Number.isFinite(at) || now - at > PingMaxAgeMilliseconds) {
    return { action: "skip", reason: "stale" };
  }

  // A session that is mid-run or waiting on the user is skipped rather than
  // queued: a ping that lands inside a run you started is a surprise.
  if (state === "working" || state === "blocked") {
    return { action: "skip", reason: "busy" };
  }

  return { action: "send", text: ping.prompt };
}

/**
 * Reads this session's ping file, deletes it, and sends it when it is still
 * worth sending. Deleting before sending means a failing send cannot loop.
 */
export function pollPings({ directory, sessionId, state, send, now = Date.now() }) {
  if (!directory || !isPlainSessionId(sessionId)) {
    return { action: "skip", reason: "nothing" };
  }

  const file = path.join(directory, `${sessionId}.json`);
  let ping;
  try {
    ping = JSON.parse(fs.readFileSync(file, "utf8"));
  } catch {
    ping = undefined;
  }

  if (ping !== undefined) {
    try {
      fs.unlinkSync(file);
    } catch {
      // The tray may have rewritten it; the next poll reads that one.
    }
  }

  const decision = ping ? pingAction(state, ping, now) : { action: "skip", reason: "nothing" };
  if (decision.action === "send") {
    send(decision.text);
  }

  return decision;
}

export function initialState() {
  return { state: "idle", detail: undefined, runActive: false, outcome: undefined };
}

/**
 * The one place a pi event becomes a state. Pure, so the mapping is tested
 * rather than observed.
 */
export function reduce(previous, event) {
  const state = previous ?? initialState();
  switch (event?.type) {
    case "session_start":
      return { ...initialState() };
    case "agent_start":
      return { ...state, state: "working", runActive: true, detail: undefined, outcome: undefined };
    case "turn_start":
      return { ...state, state: "working", runActive: true };
    case "tool_execution_start":
      return { ...state, state: "working", detail: event.toolName };
    case "ui_prompt_start":
      return { ...state, state: "blocked", detail: event.title };
    case "ui_prompt_end":
      return { ...state, state: state.runActive ? "working" : "idle", detail: undefined };
    case "agent_before_settle":
      return { ...state, outcome: event.outcome };
    case "agent_settled": {
      const next = event.aborted ? "idle" : state.outcome === "error" ? "error" : "done";
      return { state: next, detail: undefined, runActive: false, outcome: undefined };
    }
    default:
      return state;
  }
}

/** Writes the file in one step, so a reader never sees half of it. */
export function writeState(directory, fileName, payload) {
  fs.mkdirSync(directory, { recursive: true });
  const target = path.join(directory, fileName);
  const temporary = `${target}.${process.pid}.tmp`;
  fs.writeFileSync(temporary, `${JSON.stringify(payload)}\n`, "utf8");
  fs.renameSync(temporary, target);
}

export function removeState(directory, fileName) {
  try {
    fs.unlinkSync(path.join(directory, fileName));
  } catch (error) {
    if (error?.code !== "ENOENT") {
      throw error;
    }
  }
}

function describe(ctx) {
  const cwd = process.cwd();
  const name = (() => {
    try {
      return ctx?.sessionManager?.getSessionName?.() ?? undefined;
    } catch {
      return undefined;
    }
  })();
  const model = (() => {
    try {
      return ctx?.model?.id ?? process.env.PI_MODEL ?? undefined;
    } catch {
      return undefined;
    }
  })();
  return { cwd, folder: path.basename(cwd), name, model };
}

export default function picadence(pi) {
  const directory = stateDirectory();
  if (!directory) {
    return;
  }
  let sessionId;
  let fileName;
  let pollTimer;
  const pings = pingDirectory();
  let current = initialState();
  let reporting = false;

  const stopPolling = () => {
    if (pollTimer) {
      clearTimeout(pollTimer);
      pollTimer = undefined;
    }
  };

  const poll = () => {
    if (!reporting) {
      return;
    }

    try {
      pollPings({
        directory: pings,
        sessionId,
        state: current.state,
        send: (text) =>
          pi.sendMessage(
            { customType: "picadence-ping", content: text, display: true },
            { triggerTurn: true },
          ),
      });
    } catch {
      // A ping that cannot be delivered must not break the session that was
      // going to receive it.
    }

    if (reporting) {
      pollTimer = setTimeout(poll, PingPollMilliseconds);
      pollTimer.unref?.();
    }
  };

  const flush = (ctx) => {
    if (!reporting || !fileName) {
      return;
    }
    try {
      writeState(directory, fileName, {
        pid: process.pid,
        sessionId,
        ...describe(ctx),
        state: current.state,
        detail: current.detail,
        updatedAt: new Date().toISOString(),
      });
    } catch {
      // A tray that cannot read the file is better than a session that dies
      // because the tray was missing.
    }
  };

  pi.on("session_start", async (event, ctx) => {
    // Only the root TUI session has a place on the tray; RPC, print, and child
    // sessions report nothing.
    if (ctx?.mode !== "tui") {
      return;
    }
    try {
      const manager = ctx.sessionManager;
      const id = manager?.getSessionId?.();
      if (!isPlainSessionId(id)) {
        return;
      }
      sessionId = id;
      fileName = fileNameFor(process.pid, id);
      current = initialState();
      reporting = true;
      flush(ctx);

      // A ping may have been written while this session was closed, so the first
      // look happens now, not one interval from now.
      stopPolling();
      poll();
    } catch {
      reporting = false;
    }
  });

  for (const type of [
    "agent_start",
    "turn_start",
    "tool_execution_start",
    "ui_prompt_start",
    "ui_prompt_end",
    "agent_before_settle",
    "agent_settled",
  ]) {
    pi.on(type, async (event, ctx) => {
      try {
        current = reduce(current, event);
        flush(ctx);
      } catch {
        // Same reason as above.
      }
    });
  }

  pi.on("session_shutdown", async () => {
    try {
      if (fileName) {
        removeState(directory, fileName);
      }
    } catch {
      // Nothing to report and nobody to tell.
    } finally {
      stopPolling();
      reporting = false;
      fileName = undefined;
    }
  });
}
