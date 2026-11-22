# Tasks

- [x] 1.1 Keep pi's `cwd` on the session the tray reads, beside the folder name it
  shows. Verified by a `StateFile` test that asserts both.
- [x] 1.2 Record that path in the pin. Verified end to end: state file, pin, read
  back, rooted path out.
- [x] 1.3 Accept a pin whose folder is only a name, and refuse the restart with
  `the folder is gone` instead of hiding it at read time.
- [x] 1.4 Run `bash scripts/verify.sh`, restart the tray, click the pin.
