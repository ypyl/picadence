import assert from "node:assert/strict";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import test from "node:test";

import picadence, {
  fileNameFor,
  isPlainSessionId,
  initialState,
  PingMaxAgeMilliseconds,
  pingDirectory,
  pollPings,
  notifyBody,
  notifySequence,
  processStartedAt,
  tabProgressSequence,
  tabTitle,
  wantsNotification,
  wantsTabProgress,
  reduce,
  stateDirectory,
} from "./picadence.js";

function run(states, events) {
  let current = states;
  for (const event of events) {
    current = reduce(current, event);
  }
  return current;
}

test("each pi lifecycle step maps to one state", () => {
  const steps = [
    { event: { type: "session_start" }, state: "idle" },
    { event: { type: "agent_start" }, state: "working" },
    { event: { type: "turn_start" }, state: "working" },
    { event: { type: "tool_execution_start", toolName: "edit" }, state: "working" },
    { event: { type: "ui_prompt_start", title: "Confirm" }, state: "blocked" },
    { event: { type: "ui_prompt_end" }, state: "working" },
    { event: { type: "agent_before_settle", outcome: "completed" }, state: "working" },
    { event: { type: "agent_settled", aborted: false }, state: "done" },
  ];

  let current = initialState();
  for (const step of steps) {
    current = reduce(current, step.event);
    assert.equal(current.state, step.state, step.event.type);
  }
});

test("a failed run reports error and a cancelled one reports idle", () => {
  const failed = run(initialState(), [
    { type: "agent_start" },
    { type: "agent_before_settle", outcome: "error" },
    { type: "agent_settled", aborted: false },
  ]);
  assert.equal(failed.state, "error");

  const cancelled = run(initialState(), [
    { type: "agent_start" },
    { type: "agent_settled", aborted: true },
  ]);
  assert.equal(cancelled.state, "idle");
});

test("a blocked session keeps its prompt title and drops it when the prompt closes", () => {
  const blocked = reduce(reduce(initialState(), { type: "agent_start" }), {
    type: "ui_prompt_start",
    title: "Choose a branch",
  });
  assert.equal(blocked.detail, "Choose a branch");

  const resumed = reduce(blocked, { type: "ui_prompt_end" });
  assert.equal(resumed.state, "working");
  assert.equal(resumed.detail, undefined);
});

test("the state directory sits under the local application data", () => {
  assert.equal(
    stateDirectory({ LOCALAPPDATA: "C:\\Data" }),
    path.join("C:\\Data", "picadence", "agents"),
  );
});

test("a session id that is not a plain stem is rejected", () => {
  for (const bad of ["..", "a/b", "a\\b", "a:b", "", "a b", "a.b", null, 7]) {
    assert.equal(isPlainSessionId(bad), false, String(bad));
    assert.throws(() => fileNameFor(1, bad), String(bad));
  }
  assert.equal(fileNameFor(1234, "01a1fa93-ff0c"), "1234-01a1fa93-ff0c.json");
});

test("a clean pi exit leaves nothing behind", async () => {
  const localAppData = fs.mkdtempSync(path.join(os.tmpdir(), "picadence-state-"));
  const previous = process.env.LOCALAPPDATA;
  process.env.LOCALAPPDATA = localAppData;

  try {
    const handlers = new Map();
    const pi = {
      on(type, handler) {
        const list = handlers.get(type) ?? [];
        list.push(handler);
        handlers.set(type, list);
      },
    };
    picadence(pi);

    const ctx = {
      mode: "tui",
      model: { id: "test-model" },
      sessionManager: { getSessionId: () => "abc-123", getSessionName: () => "a session" },
    };

    for (const handler of handlers.get("session_start")) {
      await handler({ type: "session_start", reason: "startup" }, ctx);
    }

    const file = path.join(localAppData, "picadence", "agents", `${process.pid}-abc-123.json`);
    assert.equal(fs.existsSync(file), true);
    const written = JSON.parse(fs.readFileSync(file, "utf8"));
    assert.equal(written.state, "idle");
    assert.equal(written.sessionId, "abc-123");
    assert.equal(written.pid, process.pid);

    for (const handler of handlers.get("agent_start")) {
      await handler({ type: "agent_start" }, ctx);
    }
    assert.equal(JSON.parse(fs.readFileSync(file, "utf8")).state, "working");

    for (const handler of handlers.get("session_shutdown")) {
      await handler({ type: "session_shutdown", reason: "quit" }, ctx);
    }
    assert.equal(fs.existsSync(file), false);
  } finally {
    if (previous === undefined) {
      delete process.env.LOCALAPPDATA;
    } else {
      process.env.LOCALAPPDATA = previous;
    }
    fs.rmSync(localAppData, { recursive: true, force: true });
  }
});

test("a session that is not the root TUI session reports nothing", async () => {
  const localAppData = fs.mkdtempSync(path.join(os.tmpdir(), "picadence-state-"));
  const previous = process.env.LOCALAPPDATA;
  process.env.LOCALAPPDATA = localAppData;

  try {
    const handlers = new Map();
    picadence({
      on(type, handler) {
        const list = handlers.get(type) ?? [];
        list.push(handler);
        handlers.set(type, list);
      },
    });

    for (const handler of handlers.get("session_start")) {
      await handler(
        { type: "session_start", reason: "startup" },
        { mode: "rpc", sessionManager: { getSessionId: () => "abc-123" } },
      );
    }

    const directory = path.join(localAppData, "picadence", "agents");
    assert.equal(fs.existsSync(directory), false);
  } finally {
    if (previous === undefined) {
      delete process.env.LOCALAPPDATA;
    } else {
      process.env.LOCALAPPDATA = previous;
    }
    fs.rmSync(localAppData, { recursive: true, force: true });
  }
});

function withPings() {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "picadence-pings-"));
  return {
    directory: path.join(root, "pings"),
    clean: () => fs.rmSync(root, { recursive: true, force: true }),
  };
}

function writePing(directory, sessionId, ping) {
  fs.mkdirSync(directory, { recursive: true });
  fs.writeFileSync(path.join(directory, `${sessionId}.json`), JSON.stringify(ping), "utf8");
}

test("the ping directory sits beside the state directory", () => {
  assert.equal(
    pingDirectory({ LOCALAPPDATA: "C:\Data" }),
    path.join("C:\Data", "picadence", "pings"),
  );
});

test("a cadence follows its session across restarts and resumes", () => {
  const { directory, clean } = withPings();
  try {
    // Written while the session was closed: the tray kept the cadence, and the
    // session that resumes is the one it was armed on.
    const at = new Date(Date.now() - 1000).toISOString();
    writePing(directory, "abc-123", { at, every: "1h", missed: 0, prompt: "picadence ping - 09:00, every hour" });

    const sent = [];
    const decision = pollPings({
      directory,
      sessionId: "abc-123",
      state: "done",
      send: (text) => sent.push(text),
    });

    assert.equal(decision.action, "send");
    assert.deepEqual(sent, ["picadence ping - 09:00, every hour"]);
    assert.equal(fs.existsSync(path.join(directory, "abc-123.json")), false);
  } finally {
    clean();
  }
});

test("a busy session is skipped and the window counts as missed", () => {
  const { directory, clean } = withPings();
  try {
    const at = new Date().toISOString();
    for (const state of ["working", "blocked"]) {
      writePing(directory, "abc-123", { at, prompt: "picadence ping" });
      const sent = [];
      const decision = pollPings({ directory, sessionId: "abc-123", state, send: (text) => sent.push(text) });
      assert.equal(decision.action, "skip");
      assert.equal(decision.reason, "busy");
      assert.deepEqual(sent, []);
      assert.equal(fs.existsSync(path.join(directory, "abc-123.json")), false, "the file is not left behind");
    }
  } finally {
    clean();
  }
});

test("a ping older than five minutes is dropped", () => {
  const { directory, clean } = withPings();
  try {
    const at = new Date(Date.now() - PingMaxAgeMilliseconds - 1000).toISOString();
    writePing(directory, "abc-123", { at, prompt: "picadence ping" });
    const sent = [];
    const decision = pollPings({ directory, sessionId: "abc-123", state: "idle", send: (text) => sent.push(text) });
    assert.equal(decision.reason, "stale");
    assert.deepEqual(sent, []);
  } finally {
    clean();
  }
});

test("a ping that cannot be read is dropped without a turn", () => {
  const { directory, clean } = withPings();
  try {
    fs.mkdirSync(directory, { recursive: true });
    fs.writeFileSync(path.join(directory, "abc-123.json"), "not json", "utf8");
    const sent = [];
    const decision = pollPings({ directory, sessionId: "abc-123", state: "idle", send: (text) => sent.push(text) });
    assert.equal(decision.reason, "nothing");
    assert.deepEqual(sent, []);
  } finally {
    clean();
  }
});

test("a session with no ping sends nothing", () => {
  const { directory, clean } = withPings();
  try {
    const sent = [];
    const decision = pollPings({ directory, sessionId: "abc-123", state: "idle", send: (text) => sent.push(text) });
    assert.equal(decision.reason, "nothing");
    assert.deepEqual(sent, []);
  } finally {
    clean();
  }
});

test("the tab carries the state as a mark", () => {
  assert.equal(tabTitle("working", "docs"), "● docs");
  assert.equal(tabTitle("blocked", "docs"), "▲ docs");
  assert.equal(tabTitle("error", "docs"), "✖ docs");
  assert.equal(tabTitle("done", "docs"), "✓ docs");

  const marks = new Set(["working", "blocked", "error", "done"].map((state) => tabTitle(state, "x")[0]));
  assert.equal(marks.size, 4, "every state has its own mark");
  assert.notEqual(tabTitle("blocked", "x")[0], tabTitle("error", "x")[0]);
});

test("an idle session shows no mark", () => {
  assert.equal(tabTitle("idle", "docs"), "docs");
  assert.equal(tabTitle("idle", "docs"), tabTitle("nonsense", "docs"));
});

test("the tab keeps the session's label", () => {
  assert.equal(tabTitle("working", "named-session"), "● named-session");
  assert.equal(tabTitle("working", ""), "●", "a mark with no label is still a mark");
  assert.equal(tabTitle("idle", ""), "");
});

test("the progress sequences are the ones a terminal understands", () => {
  assert.equal(tabProgressSequence("working"), "]9;4;3;0");
  assert.equal(tabProgressSequence("blocked"), "]9;4;4;100");
  assert.equal(tabProgressSequence("error"), "]9;4;2;100");
  assert.equal(tabProgressSequence("done"), "]9;4;0;0");
  assert.equal(tabProgressSequence("idle"), "]9;4;0;0");
});

test("a transition sets the tab title and a console run does not", async () => {
  const localAppData = fs.mkdtempSync(path.join(os.tmpdir(), "picadence-tab-"));
  const previous = process.env.LOCALAPPDATA;
  process.env.LOCALAPPDATA = localAppData;

  try {
    const handlers = new Map();
    const titles = [];
    const pi = {
      on(type, handler) {
        const list = handlers.get(type) ?? [];
        list.push(handler);
        handlers.set(type, list);
      },
    };
    picadence(pi);

    const ctx = {
      mode: "tui",
      model: { id: "test-model" },
      sessionManager: { getSessionId: () => "tab-1", getSessionName: () => "named" },
      ui: { setTitle: (value) => titles.push(value) },
    };

    for (const handler of handlers.get("session_start")) {
      await handler({ type: "session_start" }, ctx);
    }
    assert.deepEqual(titles, ["named"], "an idle session carries no mark");

    for (const handler of handlers.get("agent_start")) {
      await handler({ type: "agent_start" }, ctx);
    }
    assert.equal(titles.at(-1), "● named");

    for (const handler of handlers.get("ui_prompt_start")) {
      await handler({ type: "ui_prompt_start", title: "Confirm?" }, ctx);
    }
    assert.equal(titles.at(-1), "▲ named");

    for (const handler of handlers.get("agent_settled")) {
      await handler({ type: "agent_settled" }, ctx);
    }
    assert.equal(titles.at(-1), "✓ named");
  } finally {
    if (previous === undefined) {
      delete process.env.LOCALAPPDATA;
    } else {
      process.env.LOCALAPPDATA = previous;
    }
    fs.rmSync(localAppData, { recursive: true, force: true });
  }
});

test("a session in Windows Terminal carries its state there", () => {
  assert.equal(wantsTabProgress({ WT_SESSION: "abc" }), true);
  assert.equal(wantsTabProgress({ TERM_PROGRAM: "Windows_Terminal" }), true);
  assert.equal(wantsTabProgress({}), false);
  assert.equal(wantsTabProgress({ TERM_PROGRAM: "iTerm.app" }), false);
});

test("the switch overrides what the product detected", () => {
  assert.equal(wantsTabProgress({ WT_SESSION: "abc", PICADENCE_TAB_PROGRESS: "0" }), false);
  assert.equal(wantsTabProgress({ PICADENCE_TAB_PROGRESS: "1" }), true);
  assert.equal(wantsTabProgress({ TERM_PROGRAM: "xterm", PICADENCE_TAB_PROGRESS: "1" }), true);
});

test("another terminal is written nothing", async () => {
  const localAppData = fs.mkdtempSync(path.join(os.tmpdir(), "picadence-notwt-"));
  const saved = {
    LOCALAPPDATA: process.env.LOCALAPPDATA,
    WT_SESSION: process.env.WT_SESSION,
    TERM_PROGRAM: process.env.TERM_PROGRAM,
    PICADENCE_TAB_PROGRESS: process.env.PICADENCE_TAB_PROGRESS,
  };
  process.env.LOCALAPPDATA = localAppData;
  delete process.env.WT_SESSION;
  delete process.env.TERM_PROGRAM;
  delete process.env.PICADENCE_TAB_PROGRESS;

  const written = [];
  const originalWrite = process.stdout.write;
  process.stdout.write = (chunk) => {
    written.push(String(chunk));
    return true;
  };

  try {
    const handlers = new Map();
    const titles = [];
    picadence({
      on(type, handler) {
        const list = handlers.get(type) ?? [];
        list.push(handler);
        handlers.set(type, list);
      },
    });

    const ctx = {
      mode: "tui",
      model: { id: "test-model" },
      sessionManager: { getSessionId: () => "not-wt-1", getSessionName: () => "plain" },
      ui: { setTitle: (value) => titles.push(value) },
    };

    for (const handler of handlers.get("session_start")) {
      await handler({ type: "session_start" }, ctx);
    }
    for (const handler of handlers.get("agent_start")) {
      await handler({ type: "agent_start" }, ctx);
    }

    process.stdout.write = originalWrite;
    assert.deepEqual(written.filter((chunk) => chunk.includes("]9;4;")), [], "no progress bytes");
    assert.deepEqual(titles, ["plain", "● plain"], "the mark and the label are all it shows");
  } finally {
    process.stdout.write = originalWrite;
    for (const [key, value] of Object.entries(saved)) {
      if (value === undefined) {
        delete process.env[key];
      } else {
        process.env[key] = value;
      }
    }
    fs.rmSync(localAppData, { recursive: true, force: true });
  }
});

test("a session that needs you asks the terminal to notify", () => {
  // OSC 777 from the terminal's own vocabulary, terminated the way the tab's other
  // sequences are. The bytes are the promise: a terminal reads this or ignores it.
  assert.equal(
    notifySequence("blocked", "trayclaw"),
    "\u001b]777;notify;picadence;trayclaw needs you\u0007",
  );
  assert.equal(
    notifySequence("done", "trayclaw"),
    "\u001b]777;notify;picadence;trayclaw finished\u0007",
  );
  assert.equal(
    notifySequence("error", "trayclaw"),
    "\u001b]777;notify;picadence;trayclaw failed\u0007",
  );
});

test("a quiet session asks for nothing", () => {
  for (const state of ["idle", "working", undefined]) {
    assert.equal(notifySequence(state, "trayclaw"), "");
    assert.equal(notifyBody(state, "trayclaw"), null);
  }
});

test("the switch turns it off", () => {
  const previous = process.env.PICADENCE_NOTIFY;
  try {
    process.env.PICADENCE_NOTIFY = "0";
    assert.equal(wantsNotification(), false);
    process.env.PICADENCE_NOTIFY = "1";
    assert.equal(wantsNotification(), true);
    delete process.env.PICADENCE_NOTIFY;
    assert.equal(wantsNotification(), true, "absent means on");
  } finally {
    if (previous === undefined) {
      delete process.env.PICADENCE_NOTIFY;
    } else {
      process.env.PICADENCE_NOTIFY = previous;
    }
  }
});

test("the words cannot break the sequence", () => {
  // A folder named with the separator and an escape would add a fourth field or
  // end the sequence early; both are stripped, and the field stays one field.
  const sequence = notifySequence("done", "a;b\u001bc\u0007d");
  assert.equal(sequence, "\u001b]777;notify;picadence;abcd finished\u0007");
  assert.equal(sequence.split(";").length, 4);

  // A name that is nothing but separators still says something.
  assert.equal(notifySequence("done", ";;\u0007"), "\u001b]777;notify;picadence;a session finished\u0007");
});

test("the state file says when its process started", () => {
  // Derived from uptime, so it is the same value on every write from this process,
  // and close enough to the kernel's own answer for the tray to match them.
  const now = Date.parse("2026-11-23T09:15:20.000Z");
  assert.equal(processStartedAt(now, 0), "2026-11-23T09:15:20.000Z");
  assert.equal(processStartedAt(now, 90.5), "2026-11-23T09:13:49.500Z");
  assert.equal(processStartedAt(now, NaN), "2026-11-23T09:15:20.000Z");
  assert.equal(processStartedAt(now, undefined), "2026-11-23T09:15:20.000Z");
});

test("a clock correction does not move the start it reports", async () => {
  const localAppData = fs.mkdtempSync(path.join(os.tmpdir(), "picadence-state-"));
  const previous = process.env.LOCALAPPDATA;
  const saved = {
    PICADENCE_TAB_PROGRESS: process.env.PICADENCE_TAB_PROGRESS,
    PICADENCE_NOTIFY: process.env.PICADENCE_NOTIFY,
  };
  process.env.LOCALAPPDATA = localAppData;
  process.env.PICADENCE_TAB_PROGRESS = "0";
  process.env.PICADENCE_NOTIFY = "0";
  const realNow = Date.now;

  try {
    const handlers = new Map();
    picadence({
      on(type, handler) {
        const list = handlers.get(type) ?? [];
        list.push(handler);
        handlers.set(type, list);
      },
    });

    const ctx = {
      mode: "tui",
      model: { id: "test-model" },
      sessionManager: { getSessionId: () => "abc-123", getSessionName: () => "a session" },
    };

    for (const handler of handlers.get("session_start")) {
      await handler({ type: "session_start", reason: "startup" }, ctx);
    }

    const file = path.join(localAppData, "picadence", "agents", `${process.pid}-abc-123.json`);
    const started = JSON.parse(fs.readFileSync(file, "utf8")).startedAt;
    assert.equal(typeof started, "string");

    // The machine corrects its clock a minute forward while pi keeps running. The
    // process began when it began, so the value the tray compares with the kernel
    // must not move; if it does, the tray reads a live session as a reused pid.
    Date.now = () => realNow() + 60_000;
    for (const handler of handlers.get("agent_start")) {
      await handler({ type: "agent_start" }, ctx);
    }

    assert.equal(JSON.parse(fs.readFileSync(file, "utf8")).startedAt, started);
  } finally {
    Date.now = realNow;
    if (previous === undefined) {
      delete process.env.LOCALAPPDATA;
    } else {
      process.env.LOCALAPPDATA = previous;
    }
    for (const [key, value] of Object.entries(saved)) {
      if (value === undefined) {
        delete process.env[key];
      } else {
        process.env[key] = value;
      }
    }
    fs.rmSync(localAppData, { recursive: true, force: true });
  }
});
