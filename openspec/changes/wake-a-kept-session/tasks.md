# Tasks

- [ ] 1.1 Teach the tick a fourth case: a due window for a kept session that is
  not running. Verified by a `SchedulePlanTests` case per branch.
- [ ] 1.2 Carry the wake on the ping: send, start then send, or skip, with the
  launch when there is one.
- [ ] 1.3 Order it so it cannot half-happen: ping file first, tab second, record
  third. A failed ping write still retries the window.
- [ ] 1.4 Remember a start for thirty seconds so one window opens one tab.
  Verified by a test that ticks twice.
- [ ] 1.5 A refusal is not an attempt: the window counts as missed and the next is
  a full cadence away.
- [ ] 1.6 `Panel` shows nothing new; a starting session simply becomes live.
- [ ] 1.7 `AGENTS.md` What this is, `README.md`, `CHANGELOG.md`.
- [ ] 1.8 Run `bash scripts/verify.sh`, then watch one real cadence open a tab.
