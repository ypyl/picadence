# Tasks

- [ ] 1.1 `.openspec.yaml` carries `x-level: L1`. Verified by reading it.
- [ ] 1.2 `Startup.Decide` holds the four-row table from `design.md` as a pure
      function. Verified by `StartupTests`, which covers every row.
- [ ] 1.3 `Native` gains the one Toolhelp32 snapshot and the parent lookup it needs.
      Verified by the process test in 3.2, which only detaches if the lookup is
      right, plus the by-hand check in 4.2.
- [ ] 1.4 `Program` takes the start sequence: `--tray` first, then the mutex probe,
      then the decision, then either the detached copy or the tray. Verified by
      `StartupTests` and by 3.2.
- [ ] 1.5 The detached copy is started with `CREATE_NO_WINDOW` and a breakaway
      attempt, falling back to no breakaway and then to running in place. Verified
      by 4.2, where the tray outlives the terminal it was started from.
- [ ] 1.6 A second start says `picadence is already running` and exits 0. Verified
      by hand: two starts in one terminal, one icon, one message.

- [ ] 2.1 `StartupTests` covers the four rows, with the scenario titles
      `A_start_from_a_command_shell_leaves_the_shell_free` and
      `A_start_from_a_shortcut_runs_the_tray_in_place` among them. Verified by
      `dotnet test`, and both tests fail before 1.2 lands.
- [ ] 2.2 `StartupProcessTests` writes the batch shape the CLI writes around the
      built exe, runs it through cmd, and fails when cmd is still there after the
      timeout. Verified by running it: it fails before 1.4, passes after.

- [ ] 3.1 `CHANGELOG.md` gets one entry under `## Unreleased`: the command gives the
      terminal back, and a second start says it is already running.
- [ ] 3.2 `README.md`: the .NET tool route is no longer the route with a console
      window, so the two paragraphs that say it is are rewritten. The tool section
      says what a start does now, and the login section keeps steering to the exe
      because a shortcut to a batch still flashes a console window.
- [ ] 3.3 `REPO-FACTS.md`: a command shell waits for what a batch file starts, with
      the two measurements and the date.

- [ ] 4.1 `dotnet format picadence.sln --verify-no-changes`, `dotnet build
      picadence.sln -c Release`, `dotnet test picadence.sln -c Release` and
      `node --test extensions/picadence.test.mjs` all pass. Verified by
      `bash scripts/verify.sh`, with the counts in the archive commit.
- [ ] 4.2 By hand, with the tray stopped and the tool installed from the packed
      package: `picadence` returns the prompt, the icon is there, closing the
      terminal leaves the icon, and a second `picadence` says it is already
      running. Verified by running it and recording the result in `REPO-FACTS.md`.
