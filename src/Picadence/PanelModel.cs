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
    Pin,
    Start,
    Flyout,
    Quit,
}

/// <summary>The controls at the foot of the panel. Rows are labels, not controls.</summary>
public enum PanelAction
{
    Quit,
}

/// <summary>One control in the panel. Quit is a glyph, drawn by the painter.</summary>
public sealed record PanelActionRow(
    PanelAction Action,
    DipRect Bounds,
    DipRect LabelBounds,
    string? Glyph);

/// <summary>A row is either a session that is running or one the user kept.</summary>
public enum PanelRowKind
{
    Live,
    Kept,
}

public sealed record PanelRow(
    DipRect Bounds,
    DipRect AgeBounds,
    DipRect PinBounds,
    DipRect StartBounds,
    SessionRef Reference,
    AgentState State,
    string Folder,
    string? Name,
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

    /// <summary>Where the summary stops, so the X in the header has room.</summary>
    public required DipRect SummaryBounds { get; init; }
    public string? EmptyText { get; init; }
    public string? EmptyHint { get; init; }
    public DipRect EmptyBounds { get; init; }
    public IReadOnlyList<PanelRow> Rows { get; init; } = [];
    public IReadOnlyList<PanelActionRow> Actions { get; init; } = [];
    /// <summary>How many rows the list has, and which one is at the top.</summary>
    public int TotalRows { get; init; }

    public int FirstRow { get; init; }

    /// <summary>What is missing, and what that costs.</summary>
    public string? Notice { get; init; }

    public DipRect NoticeBounds { get; init; }

    /// <summary>Where the list is, when there is more of it than fits.</summary>
    public string? ScrollText { get; init; }

    public DipRect ScrollBounds { get; init; }

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
                return PanelHit.Quit;
            }
        }

        foreach (var row in Rows)
        {
            if (row.StartBounds.Width > 0 && row.StartBounds.Contains(x, y))
            {
                return PanelHit.Start;
            }

            if (row.PinBounds.Contains(x, y))
            {
                return PanelHit.Pin;
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
    /// <summary>How far the hover fill sits inside the row it belongs to.</summary>
    public const double ActionInset = 2;

    /// <summary>
    /// A rough width for text, used only when nobody passed a real measurement. The
    /// tray passes one; tests use this so a layout test does not need a font.
    /// </summary>
    public const double EstimatePerCharacter = 6.2;
    public const double RowPadding = 12;
    /// <summary>The age column: at least this wide, and never wider than its text.</summary>
    public const double MinAgeWidth = 24;

    /// <summary>A cap, so one long phrase cannot push the pin off its column.</summary>
    public const double MaxAgeWidth = 52;

    /// <summary>The pin control on a row: a target a mouse can hit, beside the age.</summary>
    public const double PinSize = 22;

    /// <summary>The gap between the pin and the age it sits before.</summary>
    public const double PinGap = 6;

    /// <summary>An outline star, for a session that is not kept. Segoe MDL2 Assets.</summary>
    public const string KeepGlyph = "\uE734";

    /// <summary>A solid star, for a session that is kept. Segoe MDL2 Assets.</summary>
    public const string KeptGlyph = "\uE735";

    /// <summary>A play triangle, for starting a kept session. Segoe MDL2 Assets.</summary>
    public const string StartGlyph = "\uE768";

    /// <summary>An X, for the control that closes the panel and stops the tray.</summary>
    public const string QuitGlyph = "\uE8BB";

    /// <summary>The X in the header: a 22 square target, which a mouse can hit.</summary>
    public const double HeaderControl = 22;
    /// <summary>How many rows one screen shows. The rest are a scroll away.</summary>
    public const int VisibleRows = 5;

    /// <summary>The first row shown for a scroll position, kept inside the list.</summary>
    public static int FirstRow(int scroll, int total) =>
        Math.Clamp(scroll, 0, Math.Max(0, total - VisibleRows));

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
        IReadOnlyDictionary<SessionRef, Schedule>? schedules = null,
        SessionRef? flyoutFor = null,
        IReadOnlyList<KeptSession>? kept = null,
        int scroll = 0,
        Dependencies? dependencies = null,
        Func<string, double>? measure = null)
    {
        var present = dependencies ?? Dependencies.Present;

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

        // Kept sessions that are not running come after the live ones, oldest
        // first. Together they are one list, and the window shows five of it.
        var liveIds = new HashSet<string>(
            ordered.Select(session => session.Reference.SessionId), StringComparer.Ordinal);
        var keptRows = keptById.Values
            .Where(entry => !liveIds.Contains(entry.Pin.SessionId))
            .OrderBy(entry => entry.Pin.KeptAt)
            .ThenBy(entry => entry.Pin.SessionId, StringComparer.Ordinal)
            .ToList();

        var total = ordered.Count + keptRows.Count;
        var first = FirstRow(scroll, total);
        var shown = ordered.Skip(first).Take(VisibleRows).ToList();
        var keptFirst = Math.Max(0, first - ordered.Count);
        var shownKept = keptRows
            .Skip(keptFirst)
            .Take(Math.Max(0, VisibleRows - shown.Count))
            .ToList();

        var inner = Width - (2 * Padding);
        var header = new DipRect(Padding, Padding, inner, HeaderHeight);
        var y = Padding + HeaderHeight;

        // The age column is measured once from the widest age on screen, so the pins
        // line up in a column and a short age does not open a hole before its pin.
        var widest = shownKept
            .Select(entry => Age.Format(now - entry.Pin.KeptAt))
            .Concat(shown.Select(session => Age.Format(now - session.UpdatedAt)))
            .Select(age => Measure(age, measure))
            .DefaultIfEmpty(0)
            .Max();
        var ageWidth = Math.Clamp(widest, MinAgeWidth, MaxAgeWidth);

        var rows = new List<PanelRow>(shown.Count);
        foreach (var session in shown)
        {
            y += RowSpacing;
            var bounds = new DipRect(Padding, y, inner, RowHeight);
            rows.Add(new PanelRow(
                bounds,
                new DipRect(
                    bounds.X + bounds.Width - RowPadding - ageWidth,
                    bounds.Y + 15,
                    ageWidth,
                    16),
                PinOf(bounds, ageWidth),
                StartBounds: default,
                session.Reference,
                session.State,
                session.Folder,
                NameOf(session.Name, session.Folder),
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
                    bounds.X + bounds.Width - RowPadding - ageWidth,
                    bounds.Y + 15,
                    ageWidth,
                    16),
                PinOf(bounds, ageWidth),
                entry.Plan.CanRestart ? StartOf(PinOf(bounds, ageWidth)) : default,
                new SessionRef(0, entry.Pin.SessionId),
                AgentState.Idle,
                Restart.Leaf(entry.Pin.Folder),
                NameOf(entry.Pin.Name, Restart.Leaf(entry.Pin.Folder)),
                "not running",
                entry.Plan.Note,
                Age.Format(now - entry.Pin.KeptAt),
                ScheduleText(entry.Pin.SessionId, armedById, now),
                PanelRowKind.Kept,
                Keeping: true,
                entry.Plan.CanRestart));
            y += RowHeight;
        }

        var scrollText = total > VisibleRows
            ? string.Create(
                CultureInfo.InvariantCulture,
                $"{first + 1}\u2013{first + rows.Count} of {total}  \u00b7  scroll")
            : null;
        var scrollBounds = default(DipRect);
        if (scrollText is not null)
        {
            y += RowSpacing;
            scrollBounds = new DipRect(Padding + 24, y, inner - 24, 18);
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

        // The pi half comes first when both apply: without it nothing is reported at
        // all, and a terminal that cannot start a session matters less than a product
        // that cannot see one.
        var notice = !present.Pi
            ? null
            : present.Half switch
            {
                PiHalf.Missing => "the pi half is not installed \u00b7 " + PiExtension.Command,
                PiHalf.Declared =>
                    "the pi half is installed and has not reported \u00b7 it reports when a pi session starts",
                _ => present.Terminal
                    ? null
                    : "windows terminal is not installed \u00b7 a session cannot be started",
            };
        var noticeBounds = default(DipRect);
        if (notice is not null)
        {
            y += RowSpacing;
            noticeBounds = new DipRect(Padding, y, inner, 18);
            y += 18;
        }

        y += Padding;

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
            SummaryBounds = new DipRect(
                header.X + 60,
                header.Y,
                Math.Max(0, header.Width - 60 - HeaderControl - 8),
                header.Height),
            EmptyText = empty ? (present.Pi ? "No sessions are running" : "pi is not installed") : null,
            EmptyHint = empty
                ? (present.Pi ? "Start pi to see it here" : "Install it from pi.dev, then start a session")
                : null,
            EmptyBounds = emptyBounds,
            Rows = rows,
            Actions =
            [
                new PanelActionRow(
                    PanelAction.Quit,
                    new DipRect(
                        Padding + inner - HeaderControl,
                        Padding + ((HeaderHeight - HeaderControl) / 2),
                        HeaderControl,
                        HeaderControl),
                    new DipRect(
                        Padding + inner - HeaderControl + 4,
                        Padding + ((HeaderHeight - HeaderControl) / 2) + 4,
                        HeaderControl - 8,
                        HeaderControl - 8),
                    QuitGlyph),
            ],
            TotalRows = total,
            FirstRow = first,
            Notice = notice,
            NoticeBounds = noticeBounds,
            ScrollText = scrollText,
            ScrollBounds = scrollBounds,
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

    /// <summary>
    /// The session's own name, when it has one and when it is not simply the folder
    /// again: a row that repeats itself spends a line saying nothing.
    /// </summary>
    public static string? NameOf(string? name, string folder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var trimmed = name.Trim();
        return string.Equals(trimmed, folder, StringComparison.OrdinalIgnoreCase) ? null : trimmed;
    }

    /// <summary>Where the start control sits: immediately left of the pin.</summary>
    private static DipRect StartOf(DipRect pin) => new(
        pin.X - PinGap - PinSize,
        pin.Y,
        PinSize,
        PinSize);

    /// <summary>Where the pin sits on a row: before the age, centred vertically.</summary>
    private static DipRect PinOf(DipRect row, double ageWidth) => new(
        row.X + row.Width - RowPadding - ageWidth - PinGap - PinSize,
        row.Y + ((RowHeight - PinSize) / 2),
        PinSize,
        PinSize);

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
        var itemCount = row.Kind == PanelRowKind.Live ? CadenceExtensions.Offered.Length : 0;
        if (itemCount == 0)
        {
            // A kept row's actions are its own controls, so it has no menu.
            return null;
        }

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

        }

        var headerText = row.Kind == PanelRowKind.Live ? FlyoutHeader : KeptFlyoutHeader;
        return new PanelFlyout(index, new DipRect(x, y, FlyoutWidth, height), headerText, header, items);
    }

    private static double Measure(string text, Func<string, double>? measure) =>
        measure?.Invoke(text) ?? (text.Length * EstimatePerCharacter);

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
        hit is PanelHit.Quit or PanelHit.Flyout or PanelHit.Pin or PanelHit.Start
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
