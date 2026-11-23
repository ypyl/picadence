# winget carries the tray (L0)

## Why

The only way to get picadence today is to find a release page and download an exe.
winget is where a Windows user looks, and a portable manifest fits what the product
already is: one self-contained file, no installer, no runtime.

The manifests were written, validated with `winget validate`, and installed and
uninstalled on this machine with `winget install --manifest` before the pull request
went out. The pull request is microsoft/winget-pkgs#487538.

## What Changes

- `packaging/winget/0.2.0/` holds the three manifests exactly as submitted, with a
  `README.md` that says what each file is and what a new version has to change.
- `README.md` gains an install section: the release asset, and the winget command
  once the community repository takes it.
- `REPO-FACTS.md` records the identifier, the pull request, and the one step that is
  easy to forget: a release is not on winget until that pull request merges.

## Capabilities

None. Packaging and prose, so `skip_specs: true`.

## Impact

- `packaging/winget/`, `README.md`, `REPO-FACTS.md`.
- No code, no dependency. The product binary is unchanged; winget installs the
  release asset it already produces.

## Routing

0. **Does observable behavior change?** The program does not. How it arrives does.
1. **Can it be undone easily?** A submitted manifest cannot be unpublished, but it
   can be versioned forward and the package is just an identifier. The repository
   copies are one commit.
2. **Can a machine prove it correct?** Yes, and it was: `winget validate`, then an
   install and an uninstall of these exact files on Windows 11.
3. **Does anything outside the change depend on it?** A package identifier that is
   permanent once merged, and a pull request a moderator owns.

Level: **L0**.
