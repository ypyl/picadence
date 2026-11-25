# Tasks

- [x] 1.1 `Schedule`: carry the armed day, and use it when advancing and counting missed windows.
- [x] 1.2 `CadenceExtensions.Advance`: clamp the day to the month it lands in.
- [x] 1.3 `ScheduleFile`: read and write the day, and treat a record without it as today's behavior.
- [x] 1.4 `Panel.SetCadence`: arm with today's day; `SchedulePlan` and `PingFile` pass the day through.
- [x] 1.5 Tests: the hop past February, the leap day in 2032, and a due record that lands on 31 March.
- [x] 1.6 Changelog entry under `## Unreleased`.
- [x] 1.7 Run `bash scripts/verify.sh` and record the counts.
