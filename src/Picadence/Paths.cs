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
}
