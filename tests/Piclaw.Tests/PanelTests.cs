namespace Piclaw.Tests;

public class PanelModelTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 21, 0, 0, TimeSpan.Zero);

    private static AgentSession Session(
        int pid, AgentState state, int secondsAgo = 0, string folder = "piclaw", string? detail = null) =>
        new(
            new SessionRef(pid, $"session-{pid}"),
            folder,
            Name: null,
            Model: null,
            state,
            detail,
            Now.AddSeconds(-secondsAgo));

    [Fact(DisplayName = "the panel lists each session with its folder, state, and age")]
    public void The_panel_lists_each_session_with_folder_state_and_age()
    {
        var layout = PanelModel.Build(
            [
                Session(1, AgentState.Working, secondsAgo: 120, folder: "older"),
                Session(2, AgentState.Blocked, secondsAgo: 5, folder: "newer", detail: "Confirm"),
            ],
            Now);

        Assert.Equal(2, layout.Rows.Count);
        Assert.Equal("newer", layout.Rows[0].Folder);
        Assert.Equal("waiting", layout.Rows[0].StateWord);
        Assert.Equal("Confirm", layout.Rows[0].Detail);
        Assert.Equal("5s", layout.Rows[0].Age);
        Assert.Equal(AgentState.Blocked, layout.Rows[0].State);

        Assert.Equal("older", layout.Rows[1].Folder);
        Assert.Equal("2m", layout.Rows[1].Age);
        Assert.True(layout.Rows[0].Bounds.Y < layout.Rows[1].Bounds.Y, "newest first, top to bottom");
    }

    [Fact(DisplayName = "every state is named as well as coloured")]
    public void Every_state_is_named_as_well_as_coloured()
    {
        foreach (var state in Enum.GetValues<AgentState>())
        {
            var layout = PanelModel.Build([Session(1, state)], Now);
            Assert.Equal(state.Describe(), layout.Rows[0].StateWord);
            Assert.Equal(PanelTheme.State(state), PanelTheme.State(layout.Rows[0].State));
        }
    }

    [Fact(DisplayName = "a session row takes no action")]
    public void A_session_row_takes_no_action()
    {
        var layout = PanelModel.Build([Session(1, AgentState.Working)], Now);
        var row = layout.Rows[0].Bounds;

        Assert.Equal(PanelHit.Row, layout.HitTest(row.X + 4, row.Y + 4));
        Assert.Equal(PanelHit.None, layout.HitTest(4, 4));
        Assert.Equal(PanelHit.Quit, layout.HitTest(
            layout.QuitBounds.X + 2, layout.QuitBounds.Y + 2));
        Assert.True(PanelController.After(true, PanelInput.RowClicked));
    }

    [Fact(DisplayName = "an empty panel says nothing is running")]
    public void An_empty_panel_says_nothing_is_running()
    {
        var layout = PanelModel.Build([], Now);

        Assert.Empty(layout.Rows);
        Assert.Equal("No sessions are running", layout.EmptyText);
        Assert.False(string.IsNullOrEmpty(layout.EmptyHint));
        Assert.True(layout.QuitBounds.Width > 0, "Quit is still there");
        Assert.Equal(PanelHit.Quit, layout.HitTest(
            layout.QuitBounds.X + 2, layout.QuitBounds.Y + 2));
    }

    [Fact(DisplayName = "the panel updates while it stays open")]
    public void The_panel_updates_while_it_stays_open()
    {
        var before = PanelModel.Build([Session(1, AgentState.Working)], Now);
        var after = PanelModel.Build([Session(1, AgentState.Blocked)], Now.AddSeconds(3));

        Assert.Equal("working", before.Rows[0].StateWord);
        Assert.Equal("waiting", after.Rows[0].StateWord);
        Assert.InRange(TraySettings.PollMilliseconds, 1, 2000);
    }

    [Fact]
    public void The_panel_caps_the_rows_and_says_how_many_are_hidden()
    {
        var sessions = Enumerable.Range(1, 12)
            .Select(pid => Session(pid, AgentState.Working, secondsAgo: pid))
            .ToList();

        var layout = PanelModel.Build(sessions, Now);

        Assert.Equal(PanelModel.MaxRows, layout.Rows.Count);
        Assert.Equal("+4 more", layout.MoreText);
    }

    [Fact]
    public void The_age_keeps_a_margin_from_the_row_edge()
    {
        var layout = PanelModel.Build(
            [
                Session(1, AgentState.Working, secondsAgo: 1),
                Session(2, AgentState.Blocked, secondsAgo: 90, detail: "Confirm"),
                Session(3, AgentState.Error, secondsAgo: 7200),
            ],
            Now);

        Assert.All(layout.Rows, row =>
        {
            var margin = (row.Bounds.X + row.Bounds.Width) - (row.AgeBounds.X + row.AgeBounds.Width);
            Assert.True(margin >= 8, $"the age is {margin} DIPs from the row edge");
            Assert.True(row.AgeBounds.X > row.Bounds.X);
        });
    }

    [Fact]
    public void The_header_summarises_the_sessions()
    {
        var layout = PanelModel.Build(
            [
                Session(1, AgentState.Working),
                Session(2, AgentState.Working),
                Session(3, AgentState.Blocked),
            ],
            Now);

        Assert.Equal("1 waiting, 2 working", layout.Summary);
        Assert.Equal("nothing running", PanelModel.Build([], Now).Summary);
    }

    [Fact]
    public void Rows_do_not_overlap_and_the_panel_grows_with_them()
    {
        var one = PanelModel.Build([Session(1, AgentState.Working)], Now);
        var two = PanelModel.Build([Session(1, AgentState.Working), Session(2, AgentState.Idle, secondsAgo: 1)], Now);

        Assert.True(two.Height > one.Height);
        for (var i = 1; i < two.Rows.Count; i++)
        {
            Assert.True(two.Rows[i].Bounds.Y >= two.Rows[i - 1].Bounds.Y + two.Rows[i - 1].Bounds.Height);
        }

        Assert.True(two.QuitBounds.Y >= two.Rows[^1].Bounds.Y + two.Rows[^1].Bounds.Height);
        Assert.True(
            two.Rows[0].Bounds.Y >= two.HeaderBounds.Y + two.HeaderBounds.Height,
            "the first row starts below the header");
    }
}

public class PanelControllerTests
{
    [Fact(DisplayName = "a click opens and closes the panel")]
    public void A_click_opens_and_closes_the_panel()
    {
        var opened = PanelController.After(false, PanelInput.IconClicked);
        Assert.True(opened);
        Assert.False(PanelController.After(opened, PanelInput.IconClicked));
    }

    [Fact(DisplayName = "losing focus closes the panel")]
    public void Losing_focus_closes_the_panel()
    {
        Assert.False(PanelController.After(true, PanelInput.FocusLost));
        Assert.False(PanelController.After(false, PanelInput.FocusLost));
    }

    [Fact(DisplayName = "Escape closes the panel")]
    public void Escape_closes_the_panel()
    {
        Assert.False(PanelController.After(true, PanelInput.Escape));
    }
}

public class PanelPlacementTests
{
    private static readonly DipRect WorkArea = new(0, 0, 1920, 1040);

    [Fact(DisplayName = "the panel stays inside the work area")]
    public void The_panel_stays_inside_the_work_area()
    {
        // Bottom right, the usual case: above the icon, right edges aligned.
        var bottomRight = new DipRect(1840, 1000, 24, 24);
        var placed = PanelPlacement.Place(320, 300, 8, bottomRight, WorkArea);
        Assert.Equal(1544, placed.X);
        Assert.Equal(692, placed.Y);

        foreach (var icon in new[]
        {
            new DipRect(0, 0, 24, 24),              // top left
            new DipRect(1896, 0, 24, 24),           // top right
            new DipRect(0, 1016, 24, 24),           // bottom left
            new DipRect(1896, 1016, 24, 24),        // bottom right
            new DipRect(1900, 400, 20, 20),         // past the right edge
        })
        {
            var at = PanelPlacement.Place(320, 300, 8, icon, WorkArea);
            Assert.InRange(at.X, WorkArea.X, WorkArea.X + WorkArea.Width - 320);
            Assert.InRange(at.Y, WorkArea.Y, WorkArea.Y + WorkArea.Height - 300);
        }
    }

    [Fact(DisplayName = "the panel stays inside the work area")]
    public void The_panel_flips_below_when_there_is_no_room_above()
    {
        var icon = new DipRect(1800, 20, 24, 24);
        var placed = PanelPlacement.Place(320, 300, 8, icon, WorkArea);

        Assert.Equal(52, placed.Y);
    }

    [Fact(DisplayName = "the panel stays inside the work area")]
    public void A_panel_taller_than_the_screen_is_pinned_to_the_top()
    {
        var icon = new DipRect(1800, 1000, 24, 24);
        var placed = PanelPlacement.Place(320, 2000, 8, icon, WorkArea);

        Assert.Equal(WorkArea.Y, placed.Y);
    }
}

public class PanelHoverTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 21, 0, 0, TimeSpan.Zero);

    private static AgentSession Session(int pid) =>
        new(new SessionRef(pid, $"session-{pid}"), "piclaw", null, null, AgentState.Working, null, Now);

    [Fact]
    public void The_quit_button_offers_a_hand_and_a_row_does_not()
    {
        var layout = PanelModel.Build([Session(1)], Now);
        var quit = layout.QuitBounds;
        var row = layout.Rows[0].Bounds;

        Assert.Equal(PanelHit.Quit, layout.HitTest(quit.X + 4, quit.Y + 4));
        Assert.Equal(PanelPointer.Hand, PanelCursor.For(PanelHit.Quit));
        Assert.Equal(PanelPointer.Arrow, PanelCursor.For(layout.HitTest(row.X + 4, row.Y + 4)));
        Assert.Equal(PanelPointer.Arrow, PanelCursor.For(PanelHit.None));
    }

    [Fact]
    public void The_hover_surface_is_not_the_surface_it_sits_on()
    {
        Assert.NotEqual(PanelTheme.Surface, PanelTheme.Hover);
        Assert.NotEqual(PanelTheme.Row, PanelTheme.Hover);
    }
}

public class PanelWindowClassTests
{
    [Fact]
    public void The_panel_class_names_the_arrow_cursor()
    {
        var windowClass = Panel.CreateWindowClass(0, 0);

        Assert.NotEqual(0, windowClass.hCursor);
        Assert.Equal(Native.LoadCursorW(0, Native.IDC_ARROW), windowClass.hCursor);
        Assert.Equal("PiclawPanel", windowClass.lpszClassName);
    }
}

public class PanelThemeTests
{
    [Fact(DisplayName = "the text keeps its contrast")]
    public void The_text_keeps_its_contrast()
    {
        foreach (var (text, background) in PanelTheme.Pairs())
        {
            var ratio = PanelTheme.ContrastRatio(text, background);
            Assert.True(
                ratio >= PanelTheme.MinimumContrast,
                $"{text} on {background} is {ratio:F1}:1, below {PanelTheme.MinimumContrast}");
        }
    }

    [Fact(DisplayName = "the text keeps its contrast")]
    public void The_contrast_maths_matches_the_published_values()
    {
        Assert.Equal(21.0, PanelTheme.ContrastRatio(
            Rgb.FromHex("#FFFFFF"), Rgb.FromHex("#000000")), 1);
        Assert.Equal(1.0, PanelTheme.ContrastRatio(
            Rgb.FromHex("#202020"), Rgb.FromHex("#202020")), 2);
    }
}
