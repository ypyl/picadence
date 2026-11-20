# Design

One requirement forces the whole shape: the registration must name the running
exe, not a path written at install time. That is why the control lives in the app
and the registry write happens in the app.

## The registry, through Win32

`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, value name `piclaw`, type
`REG_SZ`, data `"C:\path\to\Piclaw.exe"`. The quotes are the difference between
starting and failing silently when the path has a space, and
`C:\Users\ypyl\projects\...` has none today but the folder is a name a person
chose.

`advapi32.dll` is reached through `Native.cs` like everything else:
`RegOpenKeyExW`, `RegQueryValueExW`, `RegSetValueExW`, `RegDeleteValueW`,
`RegCloseKey`. No package, no elevation: `HKEY_CURRENT_USER` is the user's own
hive, and `KEY_READ | KEY_SET_VALUE` is enough.

The alternative was `Microsoft.Win32.Registry`, which is a package reference and
therefore an L2 dependency for four calls that are already written in the same
style as the rest of the file. Rejected.

## The rules, as pure functions

| Function | What it decides |
|---|---|
| `Autostart.CommandFor(path)` | the quoted command line |
| `Autostart.IsSetTo(existing, path)` | whether the registration already names this exe, which is the follow rule |
| `Autostart.Toggle(bool)` | the next state |
| `Autostart.NeedsRepair(existing, path)` | true when autostart is on but the value is missing or stale |

`NeedsRepair` is what runs at startup: read the value once, compare with
`Environment.ProcessPath`, rewrite when they differ. A missing exe is caught by
Windows itself, not by us; a run key pointing at a path that no longer exists
fails silently at sign-in, which is why the repair happens on every start.

## The panel

The footer becomes a list of action rows, `28` DIPs tall, full width, each with a
label on the left and a value on the right:

```
Start with Windows                        on
Quit
```

Both rows hover exactly like Quit does now, and both ask for the hand. The model
carries `PanelActionRow(Action, Bounds, Label, Value)`, and `HitTest` returns
`Autostart`, `Quit`, `Row`, or `None`. Adding a third control later is a row, not
a layout rewrite.

## What is not tested

The four registry calls, and the Windows behaviour at sign-in. The smoke flips
the control and reads the value back with `reg query`, which is the closest a
machine gets to signing in.
