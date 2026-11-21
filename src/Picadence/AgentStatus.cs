// What the icon says: which state wins. The panel carries the detail and the
// counts, and lives in PanelModel. Program.cs turns both into Win32 calls and
// nothing else.
using System.Globalization;

namespace Picadence;

public static class AgentStatus
{
    /// <summary>The order the tooltip uses for states.</summary>
    private static readonly AgentState[] BySeverity =
    [
        AgentState.Error,
        AgentState.Blocked,
        AgentState.Working,
        AgentState.Done,
        AgentState.Idle,
    ];

    /// <summary>
    /// The most urgent state among the sessions, which is what the icon shows.
    /// Urgency descends from a failure to a quiet session.
    /// </summary>
    public static AgentState Aggregate(IEnumerable<AgentSession> sessions)
    {
        var present = new HashSet<AgentState>();
        foreach (var session in sessions)
        {
            present.Add(session.State);
        }

        foreach (var state in BySeverity)
        {
            if (present.Contains(state))
            {
                return state;
            }
        }

        return AgentState.Idle;
    }

}

/// <summary>How long ago a session changed, in the shortest readable form.</summary>
public static class Age
{
    public static string Format(TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero)
        {
            elapsed = TimeSpan.Zero;
        }

        if (elapsed.TotalSeconds < 60)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{(int)elapsed.TotalSeconds}s");
        }

        if (elapsed.TotalMinutes < 60)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{(int)elapsed.TotalMinutes}m");
        }

        if (elapsed.TotalHours < 24)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{(int)elapsed.TotalHours}h");
        }

        return string.Create(CultureInfo.InvariantCulture, $"{(int)elapsed.TotalDays}d");
    }
}

/// <summary>The one number the product promises: how stale the tray can be.</summary>
public static class TraySettings
{
    // shortcut: poll every second instead of watching the directory, switch to a
    // change notification when one machine has to carry more than about twenty sessions
    public const int PollMilliseconds = 1000;
}
