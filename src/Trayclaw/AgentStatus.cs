// Everything the tray decides that a machine can check: which state wins, what
// the tooltip says, and which rows the menu carries. Program.cs turns the result
// into Win32 calls and nothing else.
using System.Globalization;
using System.Text;

namespace Trayclaw;

public enum MenuEntryKind
{
    Session,
    Separator,
    Quit,
}

/// <summary>One row of the tray menu.</summary>
public sealed record MenuEntry(MenuEntryKind Kind, string Label, bool Enabled, int CommandId);

public static class AgentStatus
{
    /// <summary>The order the tray menu and the tooltip use for states.</summary>
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
            return "trayclaw - no agent sessions";
        }

        var text = new StringBuilder("trayclaw - ");
        foreach (var state in BySeverity)
        {
            if (!counts.TryGetValue(state, out var count) || count == 0)
            {
                continue;
            }

            if (text.Length > "trayclaw - ".Length)
            {
                text.Append(", ");
            }

            text.Append(count.ToString(CultureInfo.InvariantCulture))
                .Append(' ')
                .Append(state.Describe());
        }

        return text.Length <= 120 ? text.ToString() : string.Concat(text.ToString().AsSpan(0, 117), "...");
    }

    /// <summary>
    /// The menu rows: the session that changed most recently first, then a
    /// separator, then the way out. A session row is a label, never a command.
    /// </summary>
    public static IReadOnlyList<MenuEntry> Menu(IEnumerable<AgentSession> sessions, DateTimeOffset now, int quitCommandId)
    {
        var rows = sessions
            .OrderByDescending(session => session.UpdatedAt)
            .ThenBy(session => session.Folder, StringComparer.OrdinalIgnoreCase)
            .Select(session => new MenuEntry(
                MenuEntryKind.Session,
                Label(session, now),
                Enabled: false,
                CommandId: 0))
            .ToList();

        if (rows.Count > 0)
        {
            rows.Add(new MenuEntry(MenuEntryKind.Separator, string.Empty, Enabled: false, CommandId: 0));
        }

        rows.Add(new MenuEntry(MenuEntryKind.Quit, "Quit", Enabled: true, quitCommandId));
        return rows;
    }

    private static string Label(AgentSession session, DateTimeOffset now)
    {
        var text = new StringBuilder(session.Folder)
            .Append(" - ")
            .Append(session.State.Describe());

        if (!string.IsNullOrEmpty(session.Detail))
        {
            text.Append(" (").Append(session.Detail).Append(')');
        }

        return text.Append(" - ").Append(Age.Format(now - session.UpdatedAt)).ToString();
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
