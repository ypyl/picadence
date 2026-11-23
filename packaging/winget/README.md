# winget manifests

The files under `0.3.0/` are exactly what went to
[microsoft/winget-pkgs](https://github.com/microsoft/winget-pkgs) in
[pull request 487538](https://github.com/microsoft/winget-pkgs/pull/487538), for
`Ypyl.Picadence`. They live here so a release edits one file instead of
reconstructing three.

## What the three files are

| file | holds |
|---|---|
| `Ypyl.Picadence.yaml` | the identifier, the version, the default locale |
| `Ypyl.Picadence.installer.yaml` | installer type, architecture, URL, SHA256, command |
| `Ypyl.Picadence.locale.en-US.yaml` | the words: publisher, license, description, tags |

`InstallerType: portable` is what a single self-contained exe is for: winget copies
the file, verifies the hash, and puts a `picadence` alias on the PATH. No MSI, no
installer, no runtime.

## Cutting a new version

1. Cut the release, note the asset URL and its SHA256 from the release notes.
2. Copy `0.3.0/` to `<new version>/` and change, in all three files,
   `PackageVersion`, and in the installer file `InstallerUrl`, `InstallerSha256`
   and `ReleaseDate`.
3. Check it before sending it:

   ```
   winget validate --manifest packaging/winget/<version>
   winget install --manifest packaging/winget/<version>
   winget uninstall --manifest packaging/winget/<version>
   ```

   The install step is not optional: validation checks the schema, the install
   checks that the hash, the alias and the uninstall all work.
4. Fork, add the folder under `manifests/y/Ypyl/Picadence/`, and open one pull
   request. A bot validates it and a moderator merges it; a week is normal.
5. `winget upgrade` reaches users only after that merge, so a release is not
   "shipped by winget" until the pull request lands.

## Two things that cannot change

- `PackageIdentifier` is permanent once merged. `Ypyl.Picadence` is the name for
  good.
- No `Moniker` is claimed yet. `picadence` would let people type
  `winget install picadence`, and asking for it is a separate request against the
  community repository's moniker list.
