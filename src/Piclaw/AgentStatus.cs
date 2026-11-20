// What the icon says: which state wins and what the tooltip counts. The panel
// carries the detail and lives in PanelModel. Program.cs turns both into Win32
// calls and nothing else.
using System.Globalization;
using System.Text;

namespace Piclaw;

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

    /// <summary>The tooltip: one phrase per state that has at least one session.</summary>
    public static string Tooltip(IEnumerable<AgentSession> sessions)
    {
        var counts = new Dictionary<AgentState, int>();
        var total = 0;
        foreach (var session in sessions)
        {
            counts[session.State] = counts.GetValueOrDefault(session.State) + 1;
            total++;
        }

        if (total == 0)
        {
            return "piclaw - no agent sessions";
        }

        var text = new StringBuilder("piclaw - ");
        foreach (var state in BySeverity)
        {
            if (!counts.TryGetValue(state, out var count) || count == 0)
            {
                continue;
            }

            if (text.Length > "piclaw - ".Length)
            {
                text.Append(", ");
            }

            text.Append(count.ToString(CultureInfo.InvariantCulture))
                .Append(' ')
                .Append(state.Describe());
        }

        return text.Length <= 120 ? text.ToString() : string.Concat(text.ToString().AsSpan(0, 117), "...");
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
