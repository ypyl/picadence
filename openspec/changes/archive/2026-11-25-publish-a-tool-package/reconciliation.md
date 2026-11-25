# Reconciliation

Counted on 2026-11-25, at the end of the rehearsal, with the folder feed restored.

| What | Count | Detail |
|---|---|---|
| Package files in the feed | 1 | `Ypyl.Picadence.0.4.0.nupkg` |
| Versions in that package | 1 | `0.4.0` in the nuspec |
| `.pdb` files in the package | 0 | 0 bytes against a 1,951,232 byte exe |
| Tools the package declares | 1 | `picadence` runs `Picadence.exe` |
| Packages installed from it | 1 | installed, started, uninstalled |
| Left behind after uninstall | 0 | the tool path is empty |

The package's hash, which is what a release note quotes:

```
sha256 42ac5167390ec0770a12601fd8050438a3bac406960d01c3452c7e3407aa686a
```

The exe inside the package is 1,951,232 bytes. The exe the release asset carries is the
same publish output, so the two channels ship one binary. That is checked by reading the
package, not by trusting the build:

```
unzip -p Ypyl.Picadence.0.4.0.nupkg tools/any/win-x64/DotnetToolSettings.xml
  <Command Name="picadence" EntryPoint="Picadence.exe" Runner="executable" />
```

## After the first real publish

The same three counts against nuget.org: one version listed, its hash equal to the
release note, and `dotnet tool install` resolving it. If the feed lists a second version
or a hash that does not match, the release is not done.
