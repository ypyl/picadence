# Design

## One value, checked on the tick

```csharp
public sealed record Dependencies(bool Pi, bool Terminal)
{
    public static Dependencies Detect() => new(Paths.PiLauncher() is not null, Paths.WindowsTerminal() is not null);
}
```

Two `File.Exists` calls once a second is not a cost worth caching, and a person
who installs pi while the tray is running gets the truth on the next tick instead
of on the next restart. The check is deliberately dependent-free: no registry, no
process spawn, no network.

## Where the words go

Two places, because the two dependencies cost different things.

**pi missing, nothing live.** The empty state already has two lines, so it says
what is true:

```
pi is not installed
Install it from pi.dev, then start a session
```

**Windows Terminal missing.** Sessions still appear, so this cannot take over the
panel. It goes on its own line above the controls, beside where the scroll legend
sits:

```
windows terminal is not installed · a session cannot be started
```

One line, in the secondary colour, naming the cost rather than only the absence.

## What is not built

No startup dialog, no balloon, no tray icon change. The panel is the surface; a
modal on launch would be the loudest possible way to say the quietest thing.

## Tests

| behaviour | test |
|---|---|
| pi missing and nothing live names pi | `PanelTests` |
| pi present and nothing live says nothing is running | existing test |
| Windows Terminal missing lists the sessions and adds one line | `PanelTests` |
| both present shows no notice | `PanelTests` |
