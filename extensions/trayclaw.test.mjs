import assert from "node:assert/strict";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import test from "node:test";

import trayclaw, {
  fileNameFor,
  isPlainSessionId,
  initialState,
  reduce,
  stateDirectory,
} from "./trayclaw.js";

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
    path.join("C:\\Data", "trayclaw", "agents"),
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
  const localAppData = fs.mkdtempSync(path.join(os.tmpdir(), "trayclaw-state-"));
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
    trayclaw(pi);

    const ctx = {
      mode: "tui",
      model: { id: "test-model" },
      sessionManager: { getSessionId: () => "abc-123", getSessionName: () => "a session" },
    };

    for (const handler of handlers.get("session_start")) {
      await handler({ type: "session_start", reason: "startup" }, ctx);
    }

    const file = path.join(localAppData, "trayclaw", "agents", `${process.pid}-abc-123.json`);
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
  const localAppData = fs.mkdtempSync(path.join(os.tmpdir(), "trayclaw-state-"));
  const previous = process.env.LOCALAPPDATA;
  process.env.LOCALAPPDATA = localAppData;

  try {
    const handlers = new Map();
    trayclaw({
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

    const directory = path.join(localAppData, "trayclaw", "agents");
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
