# Threat model

Bootstrap adds two local processes and one directory of files. It adds no
network path, no credentials, and no multi-user surface. The attacker is another
program running as the same user, which is the only party that can reach the
state directory.

## Assets

- The set of agent sessions: which projects are open, their names, and their
  state. Project folder paths are the sensitive part.
- The tray process itself, which the user leaves running for a whole session.

## Entry points and trust boundaries

| Entry point | Who reaches it | Boundary crossed |
|---|---|---|
| `%LOCALAPPDATA%\trayclaw\agents\*.json` | any process running as the user | untrusted input, read by the tray |
| the pi extension, in the pi process | pi, loading a local file | trusted code, same user |
| the tray icon and its menu | the mouse | none |

There is no boundary between users: `%LOCALAPPDATA%` is per profile. There is no
boundary to the network: neither half opens a socket.

## Threats

| # | Threat | Path | Likelihood | Impact | Mitigation |
|---|---|---|---|---|---|
| 1 | Tampering: a program writes a file that claims to be a session | state directory, read by the tray | Low | Low | The tray validates the shape, ignores a file it cannot parse, and never derives an action from it. A fake row is UI noise, not a command. |
| 2 | Path escape: a session id carrying `..`, a separator, or a drive letter | extension to file name | Low | Medium | `SessionRef` rejects an id that is not already a plain stem, rather than repairing it, so it cannot both escape and collide. Tested. |
| 3 | Denial of service: thousands of stale files make the menu crawl | state directory | Low | Low | Files with a dead `pid` are deleted on read, and the menu lists a bounded number of rows. |
| 4 | Information disclosure: a project path appears in a screen share or a screenshot | tray tooltip and menu | Medium | Low | Accepted. The paths are already visible in the terminal tab bar next to it. |
| 5 | Spoofing: a file reports a `pid` of a different running process, so a dead session looks alive | state directory | Low | Low | Liveness is a hint, not an authorization. The worst case is a stale row. |
| 6 | Elevation: the tray is asked to start something on behalf of a session | none today | Low | High | The tray never launches a process and never executes content from a file. Kept true by construction: no such call exists in the code. |
| 7 | Prompt injection reaching the tray through model output | extension to file to tray | Low | Low | No model text enters the contract. Fields are enumerated, `detail` is a short label such as a tool name, and the tray renders no markup. |

## Accepted risks

| Risk | Why accepted | Who accepted | Revisit when |
|---|---|---|---|
| State files are unencrypted, readable by any process running as the user | The data is project paths and state words, already visible on screen; encrypting would need a key the tray has to keep anyway | the user, at the bootstrap gate | a second user or account shares the machine |
| A crash leaves a stale file until a process-liveness check removes it | Liveness is checked on every read, and the damage is a stale row for one poll cycle | the user, at the bootstrap gate | the tray shows a session that has been gone for hours |

## Controls that get a test

- Threat 2: a session id holding `..`, `/`, `\`, a colon, a space, a dot, or a
  NUL produces no file name at all, and an accepted name resolves inside the
  state directory.
