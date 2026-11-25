// The one program the tray may start. Everything is decided by a pure function so
// each refusal is a test, and the command line is built from validated parts
// rather than from text as given.
//
// pi on Windows is a .cmd shim, not an executable, and CreateProcess cannot run a
// batch file. So the tab asks cmd.exe to run it, which is exactly what a shell
// does when the user types `pi`. A real pi.exe is started directly when one exists.
namespace Picadence;

/// <summary>Why a restart is not offered. Each one has a sentence the row shows.</summary>
public enum RestartRefusal
{
    None,
    Running,
    NoConversation,
    NoFolder,
    NoTerminal,
    NoPi,
}

/// <summary>The one command line the tray may run, and where it runs.</summary>
public sealed record Launch(string File, string CommandLine, string WorkingDirectory);

/// <summary>Whether a kept session can be restarted, and the words when it cannot.</summary>
public sealed record RestartPlan(RestartRefusal Refusal, Launch? Launch, string Note)
{
    public bool CanRestart => Refusal == RestartRefusal.None && Launch is not null;
}

public static class Restart
{
    /// <summary>Long enough for a project name, short enough for a tab.</summary>
    private const int MaxTitleLength = 40;

    public static RestartPlan Plan(
        Pin pin,
        bool running,
        string? terminal,
        string? pi,
        Func<string, bool> fileExists,
        Func<string, bool> directoryExists,
        string sessionsRoot)
    {
        if (running)
        {
            return new RestartPlan(RestartRefusal.Running, null, "already running");
        }

        if (!PinFile.IsUsableFolder(pin.Folder)
            || !Path.IsPathRooted(pin.Folder)
            || !directoryExists(pin.Folder))
        {
            return new RestartPlan(RestartRefusal.NoFolder, null, "the folder is gone");
        }

        if (pin.Transcript is not { Length: > 0 } transcript
            || !PinFile.IsUsableTranscript(transcript)
            || !fileExists(transcript)
            || !Transcripts.IsInside(sessionsRoot, transcript))
        {
            return new RestartPlan(RestartRefusal.NoConversation, null, "the conversation is gone");
        }

        if (terminal is not { Length: > 0 } program)
        {
            return new RestartPlan(RestartRefusal.NoTerminal, null, "windows terminal is not installed");
        }

        if (pi is not { Length: > 0 } piProgram)
        {
            return new RestartPlan(RestartRefusal.NoPi, null, "pi is not installed");
        }

        return new RestartPlan(
            RestartRefusal.None,
            new Launch(
                program,
                CommandLine(program, Title(pin), pin.Folder, piProgram, transcript),
                pin.Folder),
            string.Empty);
    }

    /// <summary>The same decision with the machine answering the questions.</summary>
    public static RestartPlan View(Pin pin, bool running) =>
        Plan(
            pin,
            running,
            Paths.WindowsTerminal(),
            Paths.PiLauncher(),
            File.Exists,
            Directory.Exists,
            Paths.PiSessions);

    /// <summary>
    /// One new tab in the session's folder, running pi on the conversation the pin
    /// points at. A shim goes through cmd.exe, because a batch file is not a
    /// program CreateProcess can start.
    /// </summary>
    // shortcut: cmd expands %NAME% in this payload, and a command line has no escape
    // that stops it (%% is expanded again, a caret stays in the path), so a folder or
    // conversation whose path holds a name in percent signs is opened at the wrong
    // place; refuse such a pin when a project directory like that turns up
    public static string CommandLine(
        string terminal,
        string title,
        string folder,
        string pi,
        string transcript)
    {
        var tab = string.Join(
            ' ',
            Quote(terminal),
            "-w 0",
            "new-tab",
            "--title",
            Quote(title),
            "--startingDirectory",
            Quote(folder),
            "--");

        if (pi.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            return string.Join(' ', tab, Quote(pi), "--session", Quote(transcript));
        }

        // cmd strips the first and last quote of a /c payload, so the payload is
        // wrapped once more and everything inside it is quoted as it truly needs.
        var inner = Quote(pi) + " --session " + Quote(transcript);
        return string.Join(' ', tab, Quote(ComSpec()), "/d /c", "\"" + inner + "\"");
    }

    /// <summary>The command interpreter Windows says to use.</summary>
    public static string ComSpec()
    {
        if (Environment.GetEnvironmentVariable("ComSpec") is { Length: > 0 } comspec)
        {
            return comspec;
        }

        var root = Environment.GetFolderPath(Environment.SpecialFolder.System);
        return Path.Combine(root, "cmd.exe");
    }

    /// <summary>
    /// The tab's title: the session's name, or the folder's last segment, with
    /// anything that could reach a terminal stripped. A title is text a terminal
    /// executes, so a name carrying an escape sequence cannot arrive here intact.
    /// </summary>
    public static string Title(Pin pin)
    {
        var source = string.IsNullOrWhiteSpace(pin.Name) ? Leaf(pin.Folder) : pin.Name!;
        var kept = new char[Math.Min(source.Length, MaxTitleLength)];
        var used = 0;
        foreach (var c in source)
        {
            if (used == kept.Length)
            {
                break;
            }

            if (c < 0x20 || c == 0x7F || c == '"')
            {
                continue;
            }

            kept[used++] = c;
        }

        return used == 0 ? "picadence" : new string(kept, 0, used).Trim();
    }

    /// <summary>
    /// One argument, quoted the way Windows parses it: an argument with a space is
    /// wrapped, and the backslashes before a closing quote are doubled.
    /// </summary>
    public static string Quote(string value)
    {
        if (value.Length == 0)
        {
            return "\"\"";
        }

        if (!value.Any(c => c is ' ' or '\t' or '"'))
        {
            return value;
        }

        var trailing = 0;
        for (var index = value.Length - 1; index >= 0 && value[index] == '\\'; index--)
        {
            trailing++;
        }

        var escaped = value.Replace("\"", "\\\"");
        return "\"" + escaped + new string('\\', trailing) + "\"";
    }

    /// <summary>The last segment of a path, for a row that has to name a place briefly.</summary>
    public static string Leaf(string folder)
    {
        try
        {
            var trimmed = folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var leaf = Path.GetFileName(trimmed);
            return leaf.Length > 0 ? leaf : "picadence";
        }
        catch (ArgumentException)
        {
            return "picadence";
        }
    }
}
