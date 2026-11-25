# Tasks

- [x] 1.1 The `agents` sentence in **Install the pi side** points at the new
      section. Verified by reading the two together.
- [x] 1.2 A **What it writes and reads** section: every folder the tray or the pi
      half creates, its writer, its contents, and when an entry goes. Verified by
      reading `Paths.cs`, `SessionStore`, `PinStore`, `ScheduleStore`, `PingFile`,
      and `extensions/picadence.js` against each line.
- [x] 1.3 The same section lists what is read and never written, with a reason per
      row, and names the pid claim file a ping is read through. Verified likewise.
- [x] 1.4 An **Uninstalling** section: stop the tray first, what the tool
      uninstall removes, what it leaves, what to delete, and the winget and
      release-asset routes. Verified against an uninstall run on a throwaway tool
      path: the command and the store copy go, `%LOCALAPPDATA%\picadence` and the
      empty `.store` folders stay, and an uninstall with the tray running fails
      with `Access to the path 'Picadence.exe' is denied`.
- [x] 2.1 `bash scripts/verify.sh`: all checks pass, and the lint, build, and test
      counts are unchanged, because no code moved.
