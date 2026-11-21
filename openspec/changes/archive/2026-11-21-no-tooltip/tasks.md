# Tasks

## 1. The removal

- [x] 1.1 Stop setting the icon tip: the flags keep the message and the icon,
      and lose the tip. Verified: the smoke shows no tooltip on hover, and the
      icon still changes colour.
- [x] 1.2 Remove `AgentStatus.Tooltip` and the tests that read it, and drop the
      tooltip assertion from the quiet test. Verified: `dotnet test`.
- [x] 1.3 The panel summary still counts the sessions. Verified: the existing
      summary test.
- [x] 1.4 `README.md` and `CHANGELOG.md`. Verified: no document promises a
      tooltip.

## 2. Gates and finish

- [x] 2.1 `bash scripts/verify.sh`, then the smoke. Verified: the output is in
      the archive commit.
- [x] 2.2 `review-change`. Verified: every finding fixed or recorded.
- [x] 2.3 Archive, commit, merge. Verified: the delta removes the requirement.
