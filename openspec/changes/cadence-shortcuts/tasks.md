# Tasks

- [x] 1.1 Add the `shortcut:` comment above `PingPollMilliseconds` in
  `extensions/piclaw.js`, naming the ten-second ceiling and the trigger.
  Verified by `scripts/check-change.sh` and by `node --test
  extensions/piclaw.test.mjs` still passing.
- [x] 1.2 Add the `shortcut:` comment above `MaxMissed` in `src/Piclaw/Cadence.cs`,
  naming the 1000 ceiling and the trigger. Verified by `scripts/check-change.sh`
  and `dotnet test piclaw.sln -c Release` still passing.
- [x] 1.3 Run `bash scripts/verify.sh` and record the outcome in the archive
  commit.
