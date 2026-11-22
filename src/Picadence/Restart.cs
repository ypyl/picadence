// The one program the tray may start. Everything is decided by a pure function so
// each refusal is a test, and the command line is built from validated parts
// rather than from text as given.
namespace Picadence;

/// <summary>Why a restart is not offered. Each one has a sentence the row shows.</summary>
public enum RestartRefusal
{
    None,
    Running,
    NoConversation,
    NoFolder,
    NoTerminal,
}

/// <summary>The program, its arguments, and where it runs, before quoting.</summary>
public sealed record Launch(string File, IReadOnlyList<string> Arguments, string WorkingDirectory);

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

        return new RestartPlan(
            RestartRefusal.None,
            new Launch(program, Arguments(pin, transcript), pin.Folder),
            string.Empty);
    }

    /// <summary>The same decision with the machine answering the questions.</summary>
    public static RestartPlan View(Pin pin, bool running) =>
        Plan(pin, running, Paths.WindowsTerminal(), File.Exists, Directory.Exists, Paths.PiSessions);

    /// <summary>
    /// The arguments a restart passes: one new tab in the session's folder, running
    /// pi on the conversation the pin points at. Nothing here is a shell string.
    /// </summary>
    public static IReadOnlyList<string> Arguments(Pin pin, string transcript) =>
    [
        "-w", "0",
        "new-tab",
        "--title", Title(pin),
        "--startingDirectory", pin.Folder,
        "--",
        "pi", "--session", transcript,
    ];

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
    /// One command line, quoted the way Windows parses it: an argument with a space
    /// is wrapped, and the backslashes before a closing quote are doubled.
    /// </summary>
    public static string CommandLine(Launch launch)
    {
        var parts = new List<string>(launch.Arguments.Count + 1) { Quote(launch.File) };
        foreach (var argument in launch.Arguments)
        {
            parts.Add(Quote(argument));
        }

        return string.Join(' ', parts);
    }

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
