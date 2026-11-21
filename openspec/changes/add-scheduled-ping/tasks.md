# Tasks

Every task names how completion is checked. A scenario title in the delta specs
appears in the test that proves it, spelled the same.

## 1. The schedule record

- [ ] 1.1 Add the cadence type: parse and format `15m`, `1h`, `1w`, `1mo`,
  `1y`, its panel label (`every hour`), `Advance(fromUtc)`, and `CountMissed`.
  Months and years advance by the calendar. Verified by xunit tests, one named
  `a monthly cadence keeps its calendar day`, asserting 31 January to 28
  February in a non-leap year and to 29 February in a leap year, plus a
  duration case for each of the other three.
- [ ] 1.2 Add the schedule record and its store under `schedules/`, named by
  session id: read every record, ignore one that does not parse or is over the
  size cap, write one atomically, and delete only on request. Verified by xunit
  tests for the round trip and for a corrupt and an oversized file being
  ignored, and by one named `an unused cadence is still remembered a year later`
  that reads a record untouched for 400 days.
- [ ] 1.3 Add the decision that turns a tick into work: given the live sessions,
  the schedules and the current time, produce the pings to write and the records
  to advance. Verified by xunit tests named after the scenarios they prove: `a
  due cadence wakes an idle session`, `a busy session is skipped and counted as
  missed`, `a cadence never wakes another session`, `a gap produces one wake-up
  and says how many windows were missed`, `an unarmed session is never woken`,
  `the tray never starts a session`.
- [ ] 1.4 Compose and write a ping: the sentence naming piclaw, the local time
  and the cadence, asking for the session's standing instruction and nothing
  else; a catch-up line when windows were missed; a five-minute staleness bound
  recorded in the record that writes it. Verified by a xunit test named `the
  wake-up names piclaw and the cadence and gives no instruction`, which asserts
  the text and asserts it contains no verb beyond following the standing
  instruction.

## 2. The pi side consumes a ping

- [ ] 2.1 Add the pure decision `what to do with this ping` to
  `extensions/piclaw.js`: skip when the session is working or blocked, skip and
  delete when the ping is unreadable or older than five minutes, otherwise
  deliver. Verified by `node --test extensions/piclaw.test.mjs`, one test per
  branch, no file system involved.
- [ ] 2.2 Wire the decision into the extension: a chained timer every 10 seconds
  started on `session_start` when the mode is `tui`, reading and deleting
  `pings/<sid>.json`, sending a custom message with `triggerTurn: true`, and
  clearing the timer on `session_shutdown`. Verified by `node --test`, with a
  test named `a cadence follows its session across restarts and resumes` that
  consumes a ping written before the session started, and one that proves a
  console run (mode other than `tui`) never polls.

## 3. The panel arms a row

- [ ] 3.1 Add the flyout to the panel layout: its bounds anchored at the row,
  clamped inside the panel, the five cadences plus off, hit-test order flyout
  then rows then controls, and an armed row's detail line reading `every hour -
  next in 24m`. Verified by xunit tests named `arming a row sets the cadence and
  shows what comes next`, `turning a cadence off removes it`, and by the
  existing `a session row takes no action` still passing for a left click.
- [ ] 3.2 Paint the flyout in the panel's own tokens and record it in
  `DESIGN.md`: the surface, hover and text colours it reuses, and its contrast
  figures. Verified by the existing contrast test covering the new pair and by
  the capture in the visual check.
- [ ] 3.3 Wire the interaction: right click on a row opens the flyout, a click
  inside chooses, a click anywhere else only closes it, Escape closes the flyout
  before the panel, and a choice writes or deletes the schedule record. Verified
  by a manual pass on the running tray recorded in `REPO-FACTS.md`, because the
  click path itself is Win32 message handling.

## 4. One more step in the tick

- [ ] 4.1 Run the schedule step in `Refresh()` after the state directory has
  been read and the dead sessions removed, so no ping is ever written for a
  session that was just found gone. Verified by xunit on the ordered steps, and
  by a live run: arm a real session with 15 minutes, watch the state go
  `working` with no keystroke.
- [ ] 4.2 Record the live run in `REPO-FACTS.md` with the measurements this
  feature could plausibly break: the private bytes and working set of the
  published exe with a schedule armed, and the `node --test` count.

## 5. Documentation and the product's own words

- [ ] 5.1 Run `architecture-doc` and update `ARCHITECTURE.md`: the two new
  stores, the reversed arrow into pi, and the trust note that the tray may start
  a turn only in an armed session.
- [ ] 5.2 Update `README.md`: what a cadence does, the four ways to resume the
  session that a cadence follows (`pi -c`, `pi --resume`, `pi --session-id`,
  and the fact that a fresh session is not the one that was armed), and replace
  the "not an orchestrator" line with what it is now.
- [ ] 5.3 Add the `CHANGELOG.md` entry under `## Unreleased` in the words of
  what the user can now do.
- [ ] 5.4 Run `bash scripts/verify.sh` and record the commands and results.

## Workflow follow-up

- Approve the ADR gate, then the spec gate, and write both approval blocks into
  `proposal.md` before the freeze commit.
- Archive, run `scripts/check-change.sh`, commit `feat` and `chore: archive`,
  push the branch, merge with `--no-ff`, delete the branch, and remove the gate
  page and submission from `.tmp/gate/`.
