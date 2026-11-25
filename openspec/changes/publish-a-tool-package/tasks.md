# Tasks

- [ ] 1.1 `Picadence.csproj`: pack as a tool with the id, command name, readme, license, and no symbols.
- [ ] 1.2 `.github/workflows/release.yml`: tag trigger, verify, OIDC login, pack with the tag's version, push.
- [ ] 1.3 `ci.yml`: `push: branches: ['**']`, so a tag starts no ci run.
- [ ] 1.4 `README.md`: the tool line, the SDK prerequisite, and which channel is for whom.
- [ ] 1.5 `AGENTS.md`: the package id and the trusted publish fact; the Deploy block's four lines.
- [ ] 1.6 The human's side: nuget.org 2FA, the trusted publishing policy, and `gh variable set NUGET_USER --body y_pyl`.
- [ ] 1.7 `rehearsal.md`, `rollback.md`, `reconciliation.md`, each run rather than written from memory.
- [ ] 1.8 Run `bash scripts/verify.sh` and record the counts.
