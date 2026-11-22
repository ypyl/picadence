# Design

## The pin lane

One more directory beside the ones already there, written by the tray and named by
session id, so a pin is the same identity a cadence already uses:

```
%LOCALAPPDATA%\picadence\
  agents\<pid>-<sid>.json      pi writes, the tray reads and prunes
  schedules\<sid>.json         the tray writes
  pings\<sid>.json             the tray writes
  pins\<sid>.json              the tray writes (new)
```

```json
{
  "sessionId": "01a1fe3b-e1c8-71eb-afd8-f796496f0f78",
  "name": "trayclaw",
  "folder": "C:\\Users\\ypyl\\projects\\trayclaw",
  "transcript": "C:\\Users\\ypyl\\.pi\\agent\\sessions\\--C--Users-ypyl-projects-trayclaw--\\2026-11-22T09-14-02-118Z_01a1fe3b-….jsonl",
  "keptAt": "2026-11-22T19:31:04+01:00"
}
```

`transcript` is written once, while the session is live and the file is findable,
because that is the only moment the search is cheap. Nothing about the pin is
derived from the process, so it survives the process, the tray and the machine.

## Finding the conversation

pi stores one file per session per project folder:

```
%USERPROFILE%\.pi\agent\sessions\<folder-slug>\<timestamp>_<session-id>.jsonl
```

The slug is derived from the folder, and reimplementing that derivation would be a
copy of a rule pi owns. Instead the lookup enumerates the immediate subdirectories
of `sessions` and accepts `*<session-id>.jsonl` in any of them. One session id is
globally unique, so the search cannot be ambiguous in practice; if two files ever
matched, the newest wins and the choice is written into the pin.

Before a restart the stored path is checked again. If the file has moved, one more
search by id runs and the pin is updated. If it is gone, the row says the
conversation is gone.

## The one command line, built from parts

The launch is a value, produced by a pure function, not assembled from strings at
the call site:

```csharp
public sealed record Launch(string File, IReadOnlyList<string> Arguments, string WorkingDirectory);

public static RestartPlan TryBuild(Pin pin, bool isRunning, Func<string, bool> exists, string? terminal)
```

Refusal is a first-class result, because the panel needs to say why:

```csharp
public sealed record RestartPlan(RestartRefusal Refusal, Launch? Launch, string Note);
public enum RestartRefusal { None, Running, NoTranscript, NoFolder, NoTerminal }
```

Validation, each rule its own line so a test can name it:

| part | rule |
|---|---|
| session id | already the plain-stem rule the parser uses: letters, digits, hyphen |
| folder | rooted, exists, is a directory, no control character, no `"` |
| transcript | rooted, `File.Exists`, ends in `.jsonl`, inside pi's `sessions` directory after `Path.GetFullPath`, not a reparse point |
| running | the session must have no live process |
| terminal | `wt.exe` resolved below, and not a directory |

The command line itself:

```
wt.exe -w 0 new-tab --title "<name>" --startingDirectory "<folder>" -- pi --session "<transcript>"
```

Quoting is one function with one job: wrap an argument in quotes, double a
trailing backslash before the closing quote. Because validation has already
rejected quotes and control characters, quoting cannot be talked out of its depth.
The title is the session name with everything below 0x20 and 0x7f removed and the
length capped, so a session called with a terminal escape sequence in its name
cannot write to the terminal through a tab title.

## Starting it

`wt.exe` is a Store execution alias, so it is resolved by path rather than by
searching for an executable: `%LOCALAPPDATA%\Microsoft\WindowsApps\wt.exe`, and
`PATH` after that. `CreateProcessW` with `CREATE_NO_WINDOW`, no wait, both handles
closed, because the tray must not hold anything of a program it did not write.

The call is injected:

```csharp
Func<Launch, bool> launch = Program.StartProcess;
```

That is what lets the interesting scenarios be tests. A test passes a recorder and
asserts the arguments a restart would use, and a second test asserts that a running
session produces a refusal and never reaches the recorder at all.

## The panel

Rows gain a kind and, when a restart is unavailable, a sentence saying why:

```csharp
public enum PanelRowKind { Live, Kept }

public sealed record PanelRow(
    …, PanelRowKind Kind, bool CanRestart, string? Note);
```

Order: live rows first, most recently changed first, then kept rows, oldest kept
first, then the controls. A kept row that is running is a live row with a pin on
it, so it sorts with the live ones and says nothing about being kept.

The flyout grows from a cadence list into a short menu, still painted inside the
panel's own window:

| row | items |
|---|---|
| live, not kept | cadences, then `Keep` |
| live, kept | cadences, then `Stop keeping` |
| kept, not running | `Restart` (disabled with its reason), then `Stop keeping` |

A left click on any row still does nothing, which keeps the existing requirement
honest and makes a restart a deliberate act rather than a slip of the mouse.

## What the panel says when it cannot

The note comes from the refusal, in the same voice as the rest of the panel:

- `the conversation is gone`
- `the folder is gone`
- `windows terminal is not installed`
- `already running`

## Tests

| behaviour | test |
|---|---|
| a pin round-trips, and a corrupt one is ignored | `PinStoreTests` |
| the conversation is found in any session subdirectory | `TranscriptTests` |
| each refusal branch | `RestartPlanTests`, one test per row of the table |
| a folder with a space survives quoting | `RestartPlanTests` |
| a name with a newline or an escape character cannot reach the title | `RestartPlanTests` |
| a kept session outlives the tray | `PinStoreTests` reading from a fresh store |
| a restarted session keeps what was armed for it | `RestartPlanTests` with a schedule on disk |
| a running session offers no restart | `RestartPlanTests` |
| a missing conversation is named | `PanelTests` |
| a missing terminal is named | `PanelTests` |
| kept rows come after live ones | `PanelTests` |
| the tray's write paths all sit under its state directory | `PathsTests` |

The one thing no test can prove is that Windows Terminal opens the tab and pi
resumes. That check is the human's, on the machine, and it is recorded in the
archive commit.
