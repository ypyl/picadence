// The panel's content and its geometry, in device-independent pixels. Painting
// scales these once; placement clamps them; the hit test answers what a click
// landed on. None of it touches Windows.
using System.Globalization;

namespace Piclaw;

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

public sealed record PanelRow(
    DipRect Bounds,
    DipRect AgeBounds,
    AgentState State,
    string Folder,
    string StateWord,
    string Detail,
    string Age);

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

    public PanelHit HitTest(double x, double y)
    {
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

    /// <summary>
    /// The panel for these sessions. Rows are newest first, capped, and the last
    /// row says how many are not shown rather than growing the panel past the
    /// screen.
    /// </summary>
    public static PanelLayout Build(IReadOnlyList<AgentSession> sessions, DateTimeOffset now, bool autostartOn)
    {
        var ordered = sessions
            .OrderByDescending(session => session.UpdatedAt)
            .ThenBy(session => session.Folder, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var shown = ordered.Take(MaxRows).ToList();
        var hidden = ordered.Count - shown.Count;
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
                session.State,
                session.Folder,
                session.State.Describe(),
                session.Detail ?? string.Empty,
                Age.Format(now - session.UpdatedAt)));
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

        return new PanelLayout
        {
            Width = Width,
            Height = y,
            HeaderBounds = header,
            Title = "piclaw",
            Summary = Summary(counts, sessions.Count),
            EmptyText = empty ? "No sessions are running" : null,
            EmptyHint = empty ? "Start pi to see it here" : null,
            EmptyBounds = emptyBounds,
            Rows = rows,
            Actions = actions,
            MoreText = moreText,
            MoreBounds = moreBounds,
        };
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
        hit is PanelHit.Quit or PanelHit.Autostart ? PanelPointer.Hand : PanelPointer.Arrow;
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
