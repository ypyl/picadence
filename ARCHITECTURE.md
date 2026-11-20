# Architecture

Last verified: 2026-11-20, after the `agent-status` change.

trayclaw is a tray icon on Windows that reports what the pi coding agents in your
terminal tabs are doing. It reads agent state; it never drives pi.

## Context

```mermaid
flowchart LR
  user(["Person: You<br/>Windows 11, Windows Terminal"])

  system["Software System: trayclaw<br/>agent status at the edge of the screen"]

  pi["External: pi sessions<br/>coding agents in terminal tabs"]

  user -->|"reads the icon and its menu"| system
  pi -->|"reports idle, working, blocked, done, error"| system

  classDef person fill:#08427B,color:#fff,stroke:#052E56
  classDef system fill:#1168BD,color:#fff,stroke:#0B4884
  classDef external fill:#999999,color:#fff,stroke:#666666,stroke-dasharray:5 5

  class user person
  class system system
  class pi external
```

## Container

```mermaid
flowchart TB
  user(["Person: You"])

  subgraph trayclaw["trayclaw"]
    app["Trayclaw.exe<br/>C# .NET 10, NativeAOT<br/>tray icon, tooltip, menu, poll"]
    ext["pi extension<br/>trayclaw.js, plain ESM<br/>reports one state per session"]
    store[("state directory<br/>%LOCALAPPDATA%/trayclaw/agents<br/>one JSON file per session")]
  end

  pi["External: pi<br/>TUI session in a terminal tab"]
  os["External: Windows shell<br/>notification area"]

  user -->|"click, Quit"| app
  app -->|"Shell_NotifyIcon, Win32"| os
  os -->|"icon, tooltip, panel"| user
  pi -->|"loads"| ext
  ext -->|"writes one file per transition"| store
  app -->|"reads every second, deletes what ended"| store

  classDef person fill:#08427B,color:#fff,stroke:#052E56
  classDef container fill:#438DD5,color:#fff,stroke:#2E6295
  classDef store fill:#438DD5,color:#fff,stroke:#2E6295
  classDef external fill:#999999,color:#fff,stroke:#666666,stroke-dasharray:5 5

  class user person
  class app container
  class ext container
  class store store
  class pi external
  class os external
```

## Legend

| Shape | C4 element |
|---|---|
| `(["..."])` | person |
| `["..."]` | system or container |
| `[("...")]` | store |
| dashed `["..."]` | external system |

## Containers

| Container | Responsibility | Path | Technology |
|---|---|---|---|
| Trayclaw.exe | tray icon, tooltip, the status panel, one-second poll, liveness, aggregation | `src/Trayclaw` | C# .NET 10, NativeAOT, Win32 and GDI through P/Invoke |
| pi extension | maps pi's lifecycle events to one state per session and writes it | `extensions/trayclaw.js` | Node ESM, loaded by pi |

## Store

| Store | Holds | Path | Lifetime |
|---|---|---|---|
| state directory | one JSON file per session: pid, session id, folder, name, model, state, detail, updated time | `%LOCALAPPDATA%/trayclaw/agents` | written on every transition, removed on clean exit or by the tray once the process is gone |

## Open questions

- Which terminal tab a session belongs to. Windows Terminal exposes no handle, so
  the menu shows the working folder and session name instead.
- Whether a second agent host ever needs the same status. If so, the state
  directory stops being private to trayclaw and the contract moves.

## Decisions and behavior

- `adr/0001-native-tray-fed-by-pi-extension.md`: the stack, and why the state
  comes from pi rather than from the terminal.
- `openspec/specs/agent-status/spec.md`: the behavior a user can observe.
- `openspec/changes/archive/2026-11-20-bootstrap/design.md`: the state contract,
  field by field, as it was designed before it existed.
