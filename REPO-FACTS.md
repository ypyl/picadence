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

- `dotnet build picadence.sln -c Release` finished in 4 s with 0 warnings and 0
  errors, on this machine, 2026-11-20.
- `dotnet test picadence.sln -c Release` reported 114 passed, 0 failed, on
  2026-11-21 (111 after the scheduled-ping change, 62 before it).
  `node --test extensions/picadence.test.mjs` reported 13 passed, up from 7.
- `dotnet run --project src/Picadence -c Release` started the tray and the child
  process stayed alive past 6 s, 2026-11-20. It runs the JIT build, so it costs
  about 19 MB; the AOT publish is the build a user runs.
- `dotnet publish src/Picadence/Picadence.csproj -c Release` produced one
  `Picadence.exe`, 1,760,768 bytes at v0.1.0, with no runtime dependency. It needs
  the MSVC linker; without Visual Studio Build Tools the AOT step fails, and the
  hosted runner has one. `-p:Version=0.1.0` put `0.1.0+<commit>` in the product
  version without a version file, 2026-11-21.
- With the panel open on a 125% display: 16.3 MB working set and 4.6 MB private,
  2026-11-20. The panel is 320x320 DIPs at scale 1.25, so the larger surface is
  the cost, not a leak. Idle with the panel closed stays near the row below.
- Windows Terminal on this machine is 1.25.3131.0, 2026-11-23. That version has
  `wt focus-tab -t <index>` (verified: exit 0) and ignores `OSC 777`. Support for
  OSC 777 and for bell-as-notification arrived in 1.26.2734.0, both **off by
  default**, behind the profile settings `compatibility.allowOSC777` and
  `bellStyle: notification`.
- There is no supported way for a process outside a tab to focus that tab, and
  none is planned: microsoft/terminal#19783 was closed as not planned, and PR
  #19784, which implemented `wt focus-tab --session <WT_SESSION>`, was rejected on
  2026-01-26 because focus-stealing sets a precedent the platform will not set.
  What a session gets from Terminal is `WT_SESSION` and `WT_PROFILE_ID`, and
  nothing that maps them to a window or a tab index. UIA can select a tab by its
  title and is what every third-party tool does; it is unsupported and fragile
  because titles change, repeat, and get renamed.
- The repository is a pi package: a root `package.json` with
  `"pi": { "extensions": ["./extensions/picadence.js"] }`, so
  `pi install git:github.com/ypyl/picadence` works. The manifest is load-bearing,
  not cosmetic: pi's loader reads `pi.extensions` first and returns it
  (`dist/core/extensions/loader.js`, `resolveExtensionEntries`), and only scans
  `extensions/*.js` when no manifest is there. Without it, `picadence.test.mjs`
  would load as an extension in every session.
- The executable is unsigned, on purpose: a code-signing certificate costs money
  every year, and only an EV one is granted immediate SmartScreen reputation (an OV
  certificate and Azure Trusted Signing both start at zero). Since 2023 the CA
  requirement puts signing keys in hardware, so a hosted runner cannot sign with a
  USB token anyway; it would need Trusted Signing or a cloud HSM. Until then the
  README tells the reader what the prompt means and how to check the hash, and the
  winget path never takes the browser route.
- winget carries the tray as `Ypyl.Picadence`, `InstallerType: portable`, from the
  release asset. The identifier is permanent. The submission is
  microsoft/winget-pkgs#487538, opened 2026-11-23 for 0.2.0 and moved to 0.3.0 on
  2026-11-24 with the same three files and a new hash, because 0.2.0 was never
  merged. Both versions passed `winget validate` and a local
  `winget install --manifest`. A release is not on winget until that pull request
  merges, so a version bump after it lands is a new pull request, not a tag. The
  copies live in `packaging/winget/`.
- A pid is not an identity, and the write time is not either. On 2026-11-23 a
  `folio` row stayed in the panel for eleven hours after its tab closed: the file
  named pid 29240, `OpenProcess(0x1000, false, 29240)` returned a handle, and
  `GetProcessTimes` reported that process started *before* the file was written, so
  "it started before the file" accepted it. `QueryFullProcessImageNameW` on the same
  handle failed with 31, so nothing outside could say what it was. The file now
  carries `startedAt`, derived by the extension from `process.uptime()`, and the tray
  matches it against the kernel's creation time within two seconds.
- A running tray locks the executable it was started from, so a build fails with
  `MSB3027 could not copy ... Picadence.exe` while it runs. Stop the process before
  `dotnet build` or `scripts/verify.sh`; restart it afterwards. Evident every time
  a change is built with the tray up, 2026-11-23.
- The capture for the visual check has to come from a DPI-aware process, and so
  does anything that turns a window rect into a screen grab. A DPI-unaware one
  sees 400 physical pixels as 320, so a `CopyFromScreen` of a `GetWindowRect`
  lands 25% short of the window and saves a cropped image that looks like a layout
  bug. `SetProcessDpiAwarenessContext(-4)` first, then measure, 2026-11-21.
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
- `node --test extensions/picadence.test.mjs` reported 7 passed, 0 failed,
  2026-11-20. `node --test extensions` does not work on Node 22.23: it treats the
  directory as a module. Name the file.
- `dotnet list picadence.sln package --vulnerable --include-transitive` reported
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

## Hosted runner

- The first CI run, 2026-11-21, on `windows-latest` (`windows-2025`): 1m56s,
  green on the first try, with `All checks passed, 0 warning(s).`, 114 .NET tests
  and 13 node tests. The earlier pushes ran in 2m06s to 2m20s. It found no
  interop problem: the icon, cursor and registry calls all work without a desktop.
- The image's default SDK is **11.0.100**, not the 10.0.404 in Project facts. The
  net10.0 build, `dotnet format --verify-no-changes`, the AOT link and both suites
  all pass under it. `global.json` was tried, pinning 10.0.404 with
  `rollForward: latestFeature`, and the run then reported 10.0.404, but the pin
  was dropped on the human's decision: the ADR says the SDK comes from the image.
  So this machine compiles with 10.0.404 and CI compiles with 11.0.100. The
  `Record the toolchain` step prints both versions on every run, so the day they
  disagree about formatting or an analyzer, the log says which compiler did it.
  The upgrade trigger, that pin coming back, is a `shortcut:` comment in
  `.github/workflows/ci.yml`, where the debt query looks.
- Node on the image is 22.23.5, and the OpenSpec CLI the workflow installs is
  2.1.0, read from Project facts at run time. `gh run watch --exit-status` exits 0
  on a green run and non-zero on a red one.

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
