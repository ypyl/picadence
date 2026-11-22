# Tasks

- [x] 1.1 Resolve pi: `pi.exe` then `pi.cmd`, in `%USERPROFILE%\.pi\agent\bin`
  first, then on the path. Verified by the resolver returning what the machine has.
- [x] 1.2 Start a shim through `%ComSpec% /d /c` with the payload wrapped for
  `cmd`'s quote stripping, and a real executable directly. Verified by three tests,
  one per shape.
- [x] 1.3 Refuse with `pi is not installed` when neither exists.
- [x] 1.4 Prove it by hand: the same command line, run once in a real tab, resumed
  the session and pi wrote its state file.
- [x] 1.5 Run `bash scripts/verify.sh`, restart the tray.
