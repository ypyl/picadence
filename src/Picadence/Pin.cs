// A session the user asked the tray to keep. It is keyed by session id and holds
// the conversation's path, so it outlives the process that was running it, a tray
// restart and a reboot. Nothing here is derived from a process.
namespace Picadence;

public sealed record Pin(
    string SessionId,
    string? Name,
    string Folder,
    string? Transcript,
    DateTimeOffset KeptAt)
{
    /// <summary>
    /// The pin after hearing from its session, or null when it already agrees. A
    /// session can be renamed while it runs, and the pin is what the row falls back
    /// to when the tab closes, so the pin has to follow the session it names.
    /// </summary>
    public Pin? Follow(AgentSession session)
    {
        var name = session.Name;
        var folder = session.Cwd is { Length: > 0 } cwd ? cwd : Folder;

        if (string.Equals(Name, name, StringComparison.Ordinal) && SameFolder(Folder, folder))
        {
            return null;
        }

        return this with { Name = name, Folder = folder };
    }

    /// <summary>Two paths are the same place whether or not one ends in a separator.</summary>
    private static bool SameFolder(string first, string second) =>
        string.Equals(
            first.TrimEnd('\\', '/'),
            second.TrimEnd('\\', '/'),
            StringComparison.OrdinalIgnoreCase);
}
