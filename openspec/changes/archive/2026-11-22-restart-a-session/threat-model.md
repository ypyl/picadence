# Threat Model

Runs before the design is fixed, because the tray stops being a reader and becomes
a launcher. Short by intent: the surface is one program and four arguments.

## What is worth protecting

| asset | why it matters |
|---|---|
| the user's machine | the tray would be able to start programs |
| the user's conversations | transcripts are the private content of every session |
| the pin files | the first input whose contents decide what runs |
| the tray's own integrity | a tray that can be made to run anything is a foothold |

## Who could act

| actor | capability |
|---|---|
| the user | everything; the feature is theirs |
| another local user | cannot read or write `%LOCALAPPDATA%` of this user |
| malware already running as this user | can do everything the user can; the tray is not the weakest link |
| a session's own content | names and folders come from a file the extension writes |

There is no network surface and no elevation, so there is no remote actor and no
privilege boundary to cross.

## The one surface that decides execution

A pin file names a program, a folder and a transcript. Three rules keep it from
being a general launch mechanism:

1. **The program is fixed.** `wt.exe`, resolved by path. No shell, no `PATH`
   search for a bare name beyond the alias, no `cmd /c`, and no argument a caller
   can extend.
2. **The arguments are shaped, not pasted.** An id is a plain stem, a folder is an
   existing directory, a transcript is an existing `.jsonl` inside pi's sessions
   directory after `Path.GetFullPath`. Validation rejects; it never quotes its way
   out of a problem.
3. **The row is the only caller.** Nothing starts from a timer, a cadence, or a
   file arriving. A person clicks a row for a session they kept.

A pin that fails any rule is refused with a sentence, not repaired.

## Specific things that could go wrong

| attempt | what happens |
|---|---|
| put `"` or `&` in the folder | refused: the folder must be a directory whose path exists, and those characters fail the control and quote rule |
| put a newline or `ESC` in a session name | stripped before the title, so no terminal sequence is written through the tab title |
| point the transcript at `C:\Windows\System32\cmd.exe` | refused: it must end in `.jsonl` and live under the sessions directory |
| use a junction to point inside the sessions directory at something else | refused: a reparse point is not a conversation |
| arm a pin for a session that is running | refused, so two processes never share one conversation |
| leave a stale pin behind | the row says the conversation is gone; nothing runs |

## What a successful attack would look like

Somebody who can write a pin file can already write the extension's state files
and pi's config, as this user, on this machine. The most a hostile pin achieves is
launching `pi` on one of the user's own conversations, inside a folder they can
already read. That is the ceiling, and it is why this stays L2 rather than L3: no
data is deleted, no permission model moves, and no one else's data is in reach.

## Residual risks accepted

- **Tab opens in the wrong folder** if the folder was renamed after the session
  ended. The restart still works; the session's relative paths may not. Named in
  the row? No: the panel would have to explain a stale path, and one sentence that
  says "folder is gone" is enough. It gets one sentence.
- **A turn is lost** if the tab was closed mid-run. pi writes per completed turn.
- **A remote-desktop or multi-user setup** would change the ACL story. Out of
  scope: one person, one machine, stated in the product definition.
