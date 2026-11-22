// The panel's content and its geometry, in device-independent pixels. Painting
// scales these once; placement clamps them; the hit test answers what a click
// landed on. None of it touches Windows.
using System.Globalization;

namespace Picadence;

/// <summary>A rectangle in DIPs, top-left origin.</summary>
public readonly record struct DipRect(double X, double Y, double Width, double Height)
{
    public bool Contains(double x, double y) =>
        x >= X && x < X + Width && y >= Y && y < Y + Height;
}

public enum PanelHit
{
    None,
    Row,
    Flyout,
    Autostart,
    Quit,
}

/// <summary>The controls at the foot of the panel. Rows are labels, not controls.</summary>
public enum PanelAction
{
    Autostart,
    Quit,
}

public sealed record PanelActionRow(
    PanelAction Action,
    DipRect Bounds,
    DipRect LabelBounds,
    DipRect ValueBounds,
    string Label,
    string Value);

/// <summary>A row is either a session that is running or one the user kept.</summary>
public enum PanelRowKind
{
    Live,
    Kept,
}

public sealed record PanelRow(
    DipRect Bounds,
    DipRect AgeBounds,
    SessionRef Reference,
    AgentState State,
    string Folder,
    string StateWord,
    string Detail,
    string Age,
    string? Schedule,
    PanelRowKind Kind,
    bool Keeping,
    bool CanRestart);

/// <summary>A kept session, with the answer to whether it can be restarted.</summary>
public sealed record KeptSession(Pin Pin, RestartPlan Plan);

/// <summary>What a flyout item does: set a cadence, keep, stop keeping, restart.</summary>
public enum PanelFlyoutAction
{
    Cadence,
    Keep,
    StopKeeping,
    Restart,
}

/// <summary>One item in the list a right click opens on a row.</summary>
public sealed record PanelFlyoutItem(
    PanelFlyoutAction Action,
    Cadence Cadence,
    DipRect Bounds,
    string Label,
    bool Selected);

/// <summary>The list a right click opens, painted inside the panel's own window.</summary>
public sealed record PanelFlyout(
    int RowIndex,
    DipRect Bounds,
    string Header,
    DipRect HeaderBounds,
    IReadOnlyList<PanelFlyoutItem> Items);

public sealed class PanelLayout
{
    public required double Width { get; init; }
    public required double Height { get; init; }
    public required string Title { get; init; }
    public required DipRect HeaderBounds { get; init; }
    public required string Summary { get; init; }
    public string? EmptyText { get; init; }
    public string? EmptyHint { get; init; }
    public DipRect EmptyBounds { get; init; }
    public IReadOnlyList<PanelRow> Rows { get; init; } = [];
    public IReadOnlyList<PanelActionRow> Actions { get; init; } = [];
    public string? MoreText { get; init; }
    public DipRect MoreBounds { get; init; }

    /// <summary>The cadence list, when a row has been right-clicked.</summary>
    public PanelFlyout? Flyout { get; init; }

    public PanelHit HitTest(double x, double y)
    {
        // The flyout is painted over the rows, so it answers first: a click that
        // lands on it belongs to it and to nothing underneath.
        if (Flyout is { } flyout && flyout.Bounds.Contains(x, y))
        {
            return PanelHit.Flyout;
        }

        foreach (var action in Actions)
        {
            if (action.Bounds.Contains(x, y))
            {
                return action.Action == PanelAction.Quit ? PanelHit.Quit : PanelHit.Autostart;
            }
        }

        return Rows.Any(row => row.Bounds.Contains(x, y)) ? PanelHit.Row : PanelHit.None;
    }
}

public static class PanelModel
{
    public const double Width = 320;
    public const double Padding = 12;
    public const double Gap = 8;
    public const double HeaderHeight = 40;
    public const double RowHeight = 44;
    public const double RowSpacing = 8;
    /// <summary>A control is a button: Fluent's medium height, with its content inset.</summary>
    public const double ActionHeight = 32;

    public const double ActionSpacing = 2;

    /// <summary>How far the hover fill sits inside the row it belongs to.</summary>
    public const double ActionInset = 2;

    /// <summary>How far the label and the value sit inside the hover fill.</summary>
    public const double ActionPadding = 12;

    public const double ActionValueWidth = 48;
    public const double RowPadding = 12;
    public const double AgeWidth = 44;
    public const int MaxRows = 8;

    /// <summary>The cadence list: wide enough for "every 15 minutes", six rows tall.</summary>
    public const double FlyoutWidth = 180;

    public const double FlyoutItemHeight = 30;
    public const double FlyoutPadding = 6;

    /// <summary>The heading's own row, so the list says what it is.</summary>
    public const double FlyoutHeaderHeight = 24;

    /// <summary>One device-independent pixel between the heading and the cadences.</summary>
    public const double FlyoutRule = 1;

    public const string FlyoutHeader = "Repeat ping";

    /// <summary>The heading for a session that is kept and not running.</summary>
    public const string KeptFlyoutHeader = "Kept session";

    /// <summary>
    /// The panel for these sessions. Rows are newest first, capped, and the last
    /// row says how many are not shown rather than growing the panel past the
    /// screen. A right click on a row adds the cadence list, which is painted over
    /// the rows inside the same window.
    /// </summary>
    public static PanelLayout Build(
        IReadOnlyList<AgentSession> sessions,
        DateTimeOffset now,
        bool autostartOn,
        IReadOnlyDictionary<SessionRef, Schedule>? schedules = null,
        SessionRef? flyoutFor = null,
        IReadOnlyList<KeptSession>? kept = null)
    {
        var keptById = new Dictionary<string, KeptSession>(StringComparer.Ordinal);
        foreach (var entry in kept ?? [])
        {
            keptById[entry.Pin.SessionId] = entry;
        }

        var armedById = new Dictionary<string, Schedule>(StringComparer.Ordinal);
        foreach (var schedule in schedules?.Values ?? [])
        {
            armedById[schedule.SessionId] = schedule;
        }

        var ordered = sessions
            .OrderByDescending(session => session.UpdatedAt)
            .ThenBy(session => session.Folder, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var shown = ordered.Take(MaxRows).ToList();

        // Kept sessions that are not running come after the live ones, oldest
        // first, and share the same row budget so the panel stays one screen.
        var liveIds = new HashSet<string>(
            ordered.Select(session => session.Reference.SessionId), StringComparer.Ordinal);
        var keptRows = keptById.Values
            .Where(entry => !liveIds.Contains(entry.Pin.SessionId))
            .OrderBy(entry => entry.Pin.KeptAt)
            .ThenBy(entry => entry.Pin.SessionId, StringComparer.Ordinal)
            .ToList();
        var keptRoom = Math.Max(0, MaxRows - shown.Count);
        var shownKept = keptRows.Take(keptRoom).ToList();
        var hidden = (ordered.Count - shown.Count) + (keptRows.Count - shownKept.Count);
        var inner = Width - (2 * Padding);
        var header = new DipRect(Padding, Padding, inner, HeaderHeight);
        var y = Padding + HeaderHeight;

        var rows = new List<PanelRow>(shown.Count);
        foreach (var session in shown)
        {
            y += RowSpacing;
            var bounds = new DipRect(Padding, y, inner, RowHeight);
            rows.Add(new PanelRow(
                bounds,
                new DipRect(
                    bounds.X + bounds.Width - RowPadding - AgeWidth,
                    bounds.Y + 15,
                    AgeWidth,
                    16),
                session.Reference,
                session.State,
                session.Folder,
                session.State.Describe(),
                session.Detail ?? string.Empty,
                Age.Format(now - session.UpdatedAt),
                ScheduleText(session.Reference.SessionId, armedById, now),
                PanelRowKind.Live,
                Keeping: keptById.ContainsKey(session.Reference.SessionId),
                CanRestart: false));
            y += RowHeight;
        }

        foreach (var entry in shownKept)
        {
            y += RowSpacing;
            var bounds = new DipRect(Padding, y, inner, RowHeight);
            rows.Add(new PanelRow(
                bounds,
                new DipRect(
                    bounds.X + bounds.Width - RowPadding - AgeWidth,
                    bounds.Y + 15,
                    AgeWidth,
                    16),
                new SessionRef(0, entry.Pin.SessionId),
                AgentState.Idle,
                entry.Pin.Folder,
                "not running",
                entry.Plan.Note,
                "kept " + Age.Format(now - entry.Pin.KeptAt),
                ScheduleText(entry.Pin.SessionId, armedById, now),
                PanelRowKind.Kept,
                Keeping: true,
                entry.Plan.CanRestart));
            y += RowHeight;
        }

        var moreText = hidden > 0 ? string.Create(CultureInfo.InvariantCulture, $"+{hidden} more") : null;
        var moreBounds = default(DipRect);
        if (moreText is not null)
        {
            y += RowSpacing;
            moreBounds = new DipRect(Padding + 24, y, inner - 24, 18);
            y += 18;
        }

        var empty = rows.Count == 0;
        var emptyBounds = default(DipRect);
        if (empty)
        {
            y += RowSpacing;
            emptyBounds = new DipRect(Padding, y, inner, RowHeight);
            y += RowHeight;
        }

        y += RowSpacing;
        var actions = new List<PanelActionRow>(2)
        {
            Action(
                PanelAction.Autostart,
                new DipRect(Padding, y, inner, ActionHeight),
                "Start with Windows",
                autostartOn ? "on" : "off"),
        };
        y += ActionHeight + ActionSpacing;
        actions.Add(Action(
            PanelAction.Quit,
            new DipRect(Padding, y, inner, ActionHeight),
            "Quit",
            string.Empty));
        y += ActionHeight + Padding;

        var counts = sessions
            .GroupBy(session => session.State)
            .ToDictionary(group => group.Key, group => group.Count());

        var flyoutRow = flyoutFor is { } openSession ? RowOf(rows, openSession) : null;
        var flyout = Flyout(rows, armedById, flyoutRow, y);

        // A list that is taller than the rows it covers grows the panel, because a
        // window cannot paint outside itself. With one session that is the normal
        // case: 190 DIPs of panel against 211 DIPs of list.
        var height = flyout is { } open ? Math.Max(y, open.Bounds.Y + open.Bounds.Height + Padding) : y;

        return new PanelLayout
        {
            Width = Width,
            Height = height,
            HeaderBounds = header,
            Title = "picadence",
            Summary = Summary(counts, sessions.Count),
            EmptyText = empty ? "No sessions are running" : null,
            EmptyHint = empty ? "Start pi to see it here" : null,
            EmptyBounds = emptyBounds,
            Rows = rows,
            Actions = actions,
            MoreText = moreText,
            MoreBounds = moreBounds,
            Flyout = flyout,
        };
    }

    /// <summary>
    /// The row a session sits on. The list follows the session, not the row number,
    /// so a state change that reorders the rows cannot move an open list to another
    /// session.
    /// </summary>
    private static int? RowOf(IReadOnlyList<PanelRow> rows, SessionRef reference)
    {
        for (var index = 0; index < rows.Count; index++)
        {
            if (rows[index].Reference == reference)
            {
                return index;
            }
        }

        return null;
    }

    /// <summary>The cadence on a row, or null when that session is not armed.</summary>
    private static string? ScheduleText(
        string sessionId, IReadOnlyDictionary<string, Schedule>? schedules, DateTimeOffset now)
    {
        if (schedules is null || !schedules.TryGetValue(sessionId, out var schedule))
        {
            return null;
        }

        var until = schedule.NextAt - now;
        if (until < TimeSpan.Zero)
        {
            until = TimeSpan.Zero;
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{schedule.Every.Label()}, next in {Age.Format(until)}");
    }

    /// <summary>
    /// The list a right click opens, anchored at the row and clamped inside the
    /// panel, so it can never cross an edge the panel itself avoids.
    /// </summary>
    private static PanelFlyout? Flyout(
        IReadOnlyList<PanelRow> rows,
        IReadOnlyDictionary<string, Schedule>? schedules,
        int? rowIndex,
        double panelHeight)
    {
        if (rowIndex is not { } index || index < 0 || index >= rows.Count)
        {
            return null;
        }

        var row = rows[index];
        var cadences = row.Kind == PanelRowKind.Live ? CadenceExtensions.Offered.Length : 0;
        var restart = row.Kind == PanelRowKind.Kept && row.CanRestart ? 1 : 0;
        var itemCount = cadences + restart + 1;
        var height = (2 * FlyoutPadding) + FlyoutHeaderHeight + FlyoutRule + (itemCount * FlyoutItemHeight);
        var x = Math.Clamp(
            row.Bounds.X + row.Bounds.Width - FlyoutWidth,
            Padding,
            Math.Max(Padding, Width - Padding - FlyoutWidth));
        var y = Math.Clamp(
            row.Bounds.Y,
            Padding,
            Math.Max(Padding, panelHeight - height - Padding));

        var header = new DipRect(
            x + FlyoutPadding,
            y + FlyoutPadding,
            FlyoutWidth - (2 * FlyoutPadding),
            FlyoutHeaderHeight);

        var items = new List<PanelFlyoutItem>(itemCount);
        var itemY = y + FlyoutPadding + FlyoutHeaderHeight + FlyoutRule;
        var bounds = new DipRect(x + FlyoutPadding, itemY, FlyoutWidth - (2 * FlyoutPadding), FlyoutItemHeight);

        if (row.Kind == PanelRowKind.Live)
        {
            var current = schedules is not null && schedules.TryGetValue(row.Reference.SessionId, out var armed)
                ? armed.Every
                : Cadence.Off;

            foreach (var cadence in CadenceExtensions.Offered)
            {
                items.Add(new PanelFlyoutItem(
                    PanelFlyoutAction.Cadence,
                    cadence,
                    bounds,
                    cadence.Label(),
                    cadence == current));
                itemY += FlyoutItemHeight;
                bounds = new DipRect(x + FlyoutPadding, itemY, FlyoutWidth - (2 * FlyoutPadding), FlyoutItemHeight);
            }

            items.Add(new PanelFlyoutItem(
                row.Keeping ? PanelFlyoutAction.StopKeeping : PanelFlyoutAction.Keep,
                Cadence.Off,
                bounds,
                row.Keeping ? "Stop keeping" : "Keep",
                Selected: false));
        }
        else
        {
            // A session that is not running can only be restarted or dropped, and
            // the restart is offered only when it would work. What is missing is
            // said on the row, not in a greyed-out menu item nobody can press.
            if (row.CanRestart)
            {
                items.Add(new PanelFlyoutItem(
                    PanelFlyoutAction.Restart,
                    Cadence.Off,
                    bounds,
                    "Restart",
                    Selected: false));
                itemY += FlyoutItemHeight;
                bounds = new DipRect(x + FlyoutPadding, itemY, FlyoutWidth - (2 * FlyoutPadding), FlyoutItemHeight);
            }

            items.Add(new PanelFlyoutItem(
                PanelFlyoutAction.StopKeeping,
                Cadence.Off,
                bounds,
                "Stop keeping",
                Selected: false));
        }

        var headerText = row.Kind == PanelRowKind.Live ? FlyoutHeader : KeptFlyoutHeader;
        return new PanelFlyout(index, new DipRect(x, y, FlyoutWidth, height), headerText, header, items);
    }

    /// <summary>
    /// One control, laid out the way a Fluent button is: the fill sits inside the
    /// row, and the label and value sit inside the fill so neither touches its edge.
    /// </summary>
    private static PanelActionRow Action(PanelAction action, DipRect bounds, string label, string value)
    {
        var contentLeft = bounds.X + ActionInset + ActionPadding;
        var contentRight = bounds.X + bounds.Width - ActionInset - ActionPadding;
        var labelWidth = Math.Max(0, contentRight - ActionValueWidth - contentLeft);
        var valueLeft = Math.Max(contentLeft, contentRight - ActionValueWidth);

        return new PanelActionRow(
            action,
            bounds,
            new DipRect(contentLeft, bounds.Y, labelWidth, bounds.Height),
            new DipRect(valueLeft, bounds.Y, Math.Max(0, contentRight - valueLeft), bounds.Height),
            label,
            value);
    }

    private static string Summary(IReadOnlyDictionary<AgentState, int> counts, int total)
    {
        if (total == 0)
        {
            return "nothing running";
        }

        var parts = new List<string>(4);
        foreach (var state in new[] { AgentState.Error, AgentState.Blocked, AgentState.Working, AgentState.Done })
        {
            if (counts.TryGetValue(state, out var count) && count > 0)
            {
                parts.Add(string.Create(CultureInfo.InvariantCulture, $"{count} {state.Describe()}"));
            }
        }

        return parts.Count == 0
            ? "nothing needs you"
            : string.Join(", ", parts);
    }
}

/// <summary>Where the panel goes, given the icon and the monitor it lives on.</summary>
public static class PanelPlacement
{
    /// <summary>
    /// Right-aligned to the icon, above it, flipped below when the top would
    /// leave the work area, then clamped so no edge crosses. All arguments are in
    /// the same unit, pixels of the monitor the panel opens on.
    /// </summary>
    public static (double X, double Y) Place(
        double width, double height, double gap, DipRect icon, DipRect workArea)
    {
        var x = icon.X + icon.Width - width;
        var y = icon.Y - gap - height;

        if (y < workArea.Y)
        {
            y = icon.Y + icon.Height + gap;
        }

        x = Math.Clamp(x, workArea.X, Math.Max(workArea.X, workArea.X + workArea.Width - width));
        y = Math.Clamp(y, workArea.Y, Math.Max(workArea.Y, workArea.Y + workArea.Height - height));

        return (x, y);
    }
}

/// <summary>The pointer a hit asks for, so the button reads as clickable.</summary>
public enum PanelPointer
{
    Arrow,
    Hand,
}

public static class PanelCursor
{
    public static PanelPointer For(PanelHit hit) =>
        hit is PanelHit.Quit or PanelHit.Autostart or PanelHit.Flyout
            ? PanelPointer.Hand
            : PanelPointer.Arrow;
}

public enum PanelInput
{
    IconClicked,
    RowClicked,
    Escape,
    FocusLost,
}

/// <summary>The whole opening and closing rule, in one function.</summary>
public static class PanelController
{
    public static bool After(bool isOpen, PanelInput input) => input switch
    {
        PanelInput.IconClicked => !isOpen,
        PanelInput.RowClicked => isOpen,
        _ => false,
    };
}
