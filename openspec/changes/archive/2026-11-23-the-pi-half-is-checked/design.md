# Design

## The three answers, and what each proves

pi writes its installed packages to `~/.pi/agent/settings.json`, and a
project-local install goes to `.pi/settings.json` beside the session. Both are
readable, neither is written.

| evidence | answer | what the panel says |
|---|---|---|
| a session is live, or a pin exists | working | nothing |
| no such evidence, settings list picadence | declared | installed, reports when a session starts |
| no such evidence, settings never mention it | missing | not installed, and the command |

A declaration is intent, not execution: pi warns and skips a package that fails
to install, and a session can be project-local with `-l`, which the user file
never mentions. So a live session outranks both, and the check reads the user
file plus the `.pi/settings.json` of every folder the tray knows, live or kept.

## Reading it

```csharp
public enum PiHalf { Missing, Silent, Working }
public static class PiExtension
{
    public static PiHalf Check(bool anythingReported, IReadOnlyList<string> folders);
}
```

The match is a case-insensitive search for `picadence` in the package entries,
with slashes normalised, so the same package is recognised whether it was
installed from a path, a git source, or npm.

## Why the repository becomes a package

`pi install git:github.com/ypyl/picadence` clones the repository and asks it what
to load. Without a manifest, pi scans `extensions/*.js`, which in this repository
means `picadence.js` **and** `picadence.test.mjs`. With
`"pi": { "extensions": ["./extensions/picadence.js"] }` pi takes that list and
returns it, so the test file is not an extension in anybody session.

## The notice

One line, in the slot the missing-terminal notice already uses, and the pi half
wins when both apply: without the extension there is nothing to report, and a
terminal that cannot start a session matters less than a product that cannot see
one.

## Tests

| behaviour | test |
|---|---|
| a settings file that lists picadence reads as declared | `PiExtensionTests` |
| a settings file that does not, reads as missing | same |
| a project-local file counts as well as the user one | same |
| anything reported outranks both | same |
| the panel names the command when missing | `PanelTests` |
| the panel does not ask again when declared | `PanelTests` |
