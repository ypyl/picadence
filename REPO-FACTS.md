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

- `dotnet build trayclaw.sln -c Release` finished in 4 s with 0 warnings and 0
  errors, on this machine, 2026-11-20.
- `dotnet test trayclaw.sln -c Release` reported 26 passed, 0 failed, 94 ms,
  2026-11-20.
- `dotnet format trayclaw.sln --verify-no-changes` exited 0 after `.editorconfig`
  was added, 2026-11-20.
- `dotnet run --project src/Trayclaw -c Release` started the tray and the child
  process stayed alive past 6 s, 2026-11-20. It runs the JIT build, so it costs
  about 19 MB; the AOT publish is the build a user runs.
- `dotnet publish src/Trayclaw/Trayclaw.csproj -c Release` produced one
  `Trayclaw.exe` of 876,032 bytes, with no runtime dependency, 2026-11-20. It
  needs the MSVC linker; without Visual Studio Build Tools the AOT step fails.
- The published exe, idle with one tray icon: 10.35 MB working set and 3.11 MB
  private bytes after 10 s, measured with `Get-Process`, 2026-11-20. An earlier
  run of the same shape read 11.00 MB and 3.16 MB, so treat ~11 MB working set
  and ~3 MB private as the figure. The same source through the JIT measured
  19.98 MB working set, which is why the release build is AOT.
- `dotnet list trayclaw.sln package --vulnerable --include-transitive` reported
  no vulnerable package in either project, 2026-11-20.

## Environment

- Windows 11 Pro, build 10.0.26300.9974, 32 GB RAM, 2026-11-20.
- .NET SDK 10.0.404 on win-x64; Visual Studio 2022 Build Tools with the C++
  workload at `C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools`,
  found with `vswhere`, 2026-11-20.
- Terminal is Windows Terminal (`WT_SESSION` is set), 2026-11-20.

## Generated files

- `packages.lock.json` per project, written by `dotnet restore` because
  `Directory.Build.props` sets `RestorePackagesWithLockFile`. Committed on
  purpose; `dotnet restore --locked-mode` fails when a package moves without the
  file.
- `obj/` and `bin/` are build output, ignored by `**/bin/` and `**/obj/`.
- `.tmp/` holds gate pages, the template cache, and scratch work. Nothing there
  is committed.
