// Where the three lanes live. One place, so the tray, the extension's contract
// and the tests cannot drift apart.
namespace Picadence;

public static class Paths
{
    /// <summary>Everything picadence keeps for itself, under the user's own profile.</summary>
    public static string Root => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "picadence");

    /// <summary>One state file per session, written by pi and read by the tray.</summary>
    public static string Agents => Path.Combine(Root, "agents");

    /// <summary>One cadence per session, owned by the tray.</summary>
    public static string Schedules => Path.Combine(Root, "schedules");

    /// <summary>The door into a session: written by the tray, consumed by pi.</summary>
    public static string Pings => Path.Combine(Root, "pings");

    /// <summary>One kept session per session id, owned by the tray.</summary>
    public static string Pins => Path.Combine(Root, "pins");

    /// <summary>Where pi keeps its conversations, one file per session and folder.</summary>
    public static string PiSessions => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".pi",
        "agent",
        "sessions");

    /// <summary>
    /// Windows Terminal when it is installed: the Store alias first, because that
    /// is what a shell finds, then the directories on the path.
    /// </summary>
    public static string? WindowsTerminal()
    {
        var alias = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft",
            "WindowsApps",
            "wt.exe");
        if (File.Exists(alias))
        {
            return alias;
        }

        foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                     .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var candidate = Path.Combine(directory.Trim('"'), "wt.exe");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
            catch (ArgumentException)
            {
                // A path entry that is not a path is not a reason to stop looking.
            }
        }

        return null;
    }
}
