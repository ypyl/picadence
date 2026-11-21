# Repo facts

Operational facts confirmed in this repository. Not instructions. A fact names
its evidence, and it is removed when the repository contradicts it.

Rules:

- A fact is recorded with the evidence that produced it: what was tried, what
  happened, and the date. One run is enough here. Two confirmations are needed
  before a fact becomes a rule, or moves to Project facts.
- A fact never overrides `AGENTS.md`, a spec, or a gate. If it would, it is a
  decision, not a fact.
- A stable command moves to `AGENTS.md` Project facts and leaves this file.
- Keep this file under about 80 lines. Prune before adding.

## Commands

- `dotnet build piclaw.sln -c Release` finished in 4 s with 0 warnings and 0
  errors, on this machine, 2026-11-20.
- `dotnet test piclaw.sln -c Release` reported 62 passed, 0 failed, 76 ms,
  2026-11-20 (26 before the agent-status change).
- `dotnet test piclaw.sln -c Release` reported 111 passed, 0 failed, 74 ms after
  the scheduled-ping change, 2026-11-21. `node --test
  extensions/piclaw.test.mjs` reported 13 passed, up from 7.
- `dotnet format piclaw.sln --verify-no-changes` exited 0 after `.editorconfig`
  was added, 2026-11-20.
- `dotnet run --project src/Piclaw -c Release` started the tray and the child
  process stayed alive past 6 s, 2026-11-20. It runs the JIT build, so it costs
  about 19 MB; the AOT publish is the build a user runs.
- `dotnet publish src/Piclaw/Piclaw.csproj -c Release` produced one
  `Piclaw.exe` of 1,556,480 bytes, with no runtime dependency, 2026-11-20. It
  needs the MSVC linker; without Visual Studio Build Tools the AOT step fails.
  Reading state added `System.Text.Json`, which took the exe from 876,032 bytes.
- The same publish after the scheduled-ping change produced 1,751,552 bytes, and
  the published exe read 12.5 MB working set and 3.97 MB private after 8 s with
  five live sessions on disk, 2026-11-21. The schedule and ping directories are
  read or written on the same tick, so this is the number that includes them.
- With the panel open on a 125% display: 16.3 MB working set and 4.6 MB private,
  2026-11-20. The panel is 320x320 DIPs at scale 1.25, so the larger surface is
  the cost, not a leak. Idle with the panel closed stays near the row below.
- The capture for the visual check has to come from a DPI-aware process. A
  DPI-unaware one sees 400 physical pixels as 320 and saves a cropped image that
  looks like a layout bug; `SetProcessDPIAware` in the capture script fixed it,
  2026-11-20.
- Memory: the ten-second figures read on 2026-11-20 (3.1 to 3.4 MB private,
  about 11 MB working set, one session) are warm-up samples and `README.md`
  quoted them as if they were the product's cost. The same exe with one dead and
  one live session read 11.18 MB and 3.42 MB at 6 s and removed the dead file,
  which is the poll and liveness path working, not its steady state.
- Steady state, five live sessions, sampled every 20 s for nine minutes on
  2026-11-21: 16.4 MB private and 28.6 MB working set before the state parse
  cache, about 7.2 MB and 19 MB after it, both still creeping a few hundred
  kilobytes a minute when sampling stopped. Nothing is retained: a build that
  forces a full collection every 15 s reports a live managed heap of 56 KB,
  flat, 800 KB committed, private bytes flat at 4.16 MB for five minutes. The
  creep is the GC's budget meeting the allocation rate.
- The JIT build measured 19.98 MB working set against the AOT build's 11 MB,
  which is why the release build is AOT.
- `node --test extensions/piclaw.test.mjs` reported 7 passed, 0 failed,
  2026-11-20. `node --test extensions` does not work on Node 22.23: it treats the
  directory as a module. Name the file.
- `dotnet list piclaw.sln package --vulnerable --include-transitive` reported
  no vulnerable package in either project, 2026-11-20.

## Tray messages

- The icon callback and the panel both act on button *down*: `Native.WM_LBUTTONUP`
  holds `0x0201`, which is `WM_LBUTTONDOWN`. Posting `0x0202` to either window
  does nothing, and a live click works because the shell sends both. Renaming the
  constant is not free, so it stays, and the next reader needs the fact, not a
  surprise. Confirmed by posting both values to the panel, 2026-11-21.

## Schedule lane

- With a state file for a live process and a due cadence, the tray wrote
  `pings/<session id>.json` on the next 1000 ms tick and advanced
  `schedules/<session id>.json` to now plus the cadence, 2026-11-21. Driven by
  hand-written files, no session involved.
- A `WM_RBUTTONUP` on a session row, then a `WM_BUTTONUP` at the `every hour` item
  of the list, wrote `schedules/<session id>.json` with `every` `1h` and
  `nextAt` one hour later, 2026-11-21. That is the whole click path, exercised
  without moving the pointer.
- A state file whose `cwd` held single backslashes was rejected and its session
  never appeared, which is the state reader being strict rather than a bug,
  2026-11-21.

## Environment

- Windows 11 Pro, build 10.0.26300.9974, 32 GB RAM, 2026-11-20.
- .NET SDK 10.0.404 on win-x64; Visual Studio 2022 Build Tools with the C++
  workload at `C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools`,
  found with `vswhere`, 2026-11-20.
- Terminal is Windows Terminal (`WT_SESSION` is set), 2026-11-20.

## Generated files

- ``packages.lock.json` per project, written by `dotnet restore` because
  `Directory.Build.props` sets `RestorePackagesWithLockFile`. Committed on
  purpose; `dotnet restore --locked-mode` fails when a package moves without the
  file.
- `obj/` and `bin/` are build output, ignored by `**/bin/` and `**/obj/`.
- `.tmp/` holds gate pages, the template cache, and scratch work. Nothing there
  is committed.
