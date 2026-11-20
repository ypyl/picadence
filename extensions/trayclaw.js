// Reports one pi session's state to the tray.
//
// The tray reads a small JSON file per session from %LOCALAPPDATA%\trayclaw. This
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
    throw new Error(`trayclaw: process id must be a positive integer, got ${pid}`);
  }
  if (!isPlainSessionId(sessionId)) {
    throw new Error(`trayclaw: session id is not a plain stem: ${JSON.stringify(sessionId)}`);
  }
  return `${pid}-${sessionId}.json`;
}

/** The directory the tray reads. Undefined when the platform does not name one. */
export function stateDirectory(env = process.env) {
  const base = env.LOCALAPPDATA || path.join(os.homedir(), "AppData", "Local");
  if (!base) {
    return undefined;
  }
  return path.join(base, "trayclaw", "agents");
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

export default function trayclaw(pi) {
  const directory = stateDirectory();
  if (!directory) {
    return;
  }

  let sessionId;
  let fileName;
  let current = initialState();
  let reporting = false;

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
      reporting = false;
      fileName = undefined;
    }
  });
}
