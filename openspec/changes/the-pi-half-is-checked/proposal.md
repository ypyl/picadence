# Find out whether the pi half is installed (L1)

## Why

winget cannot suggest a dependency: a manifest has no way to carry one, and
`PackageDependencies` is not accepted in the community repository. So a person who
installs the tray from winget gets an empty panel, and the README's instruction is
a path into a repository they never cloned. The product is two halves and only one
of them can arrive by winget.

The tray is the only thing standing where the problem appears, and pi keeps the list
of what it has installed in a file the tray can read without writing anything.

## What Changes

- The panel gains a third dependency: the pi half. When pi is installed and nothing
  has reported, it says either that the pi half is not installed, naming the
  command, or that it is installed and reports when a session starts.
- The command it names, `pi install git:github.com/ypyl/picadence`, only works if
  the repository is a pi package. It becomes one: a root `package.json` with
  `"pi": { "extensions": ["./extensions/picadence.js"] }`.
- That manifest is not cosmetic. pi's discovery reads a `pi.extensions` list first
  and returns it, so the list is also what keeps `extensions/picadence.test.mjs`
  from being loaded as an extension in every user's session.

## Artifacts

- `design.md`: where the answer comes from, what each of the three states proves,
  and the one thing a declaration cannot promise.

## Capabilities

### Modified Capabilities

- `agent-status`: the third dependency, and what the panel says about it.

## Impact

- New: `package.json` at the repository root.
- `PiExtension` (the check), `Dependencies`, `PanelModel` (one more notice), tests,
  the README's install section, `REPO-FACTS.md`.

## Routing

0. **Does observable behavior change?** Yes: the panel says something new about a
   missing half, and the repository becomes installable by pi.
1. **Can it be undone easily?** The panel, yes. The `package.json` cannot be
   un-known by a person who has already run the command, which is why the command
   is only printed after the manifest exists and is verified.
2. **Can a machine prove it correct?** Yes: three states from a settings file that a
   test writes, and the manifest's effect on discovery is pi's own documented order
   (manifest first, conventional directory second).
3. **Does anything outside the change depend on it?** pi's package discovery does,
   and its layout is documented, not invented here.

Level: **L1**, because the product's behaviour grows a third answer about itself.
EOF

## Approval

- Gate: L1 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-23
- Artifact: openspec/changes/the-pi-half-is-checked/specs/agent-status/spec.md sha256:f20f136b6e2a2761e6210a513ca73a5cc387b46057286c2a318d0a1181611ab1
- Artifact: openspec/changes/the-pi-half-is-checked/proposal.md sha256:a12473b83ba445c94a2a2cc0940dd04d09f55df73b182ef10b1b7cee8859c156
- Words: "(none)"
