namespace Picadence.Tests;

public class PanelModelTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 21, 0, 0, TimeSpan.Zero);

    private static AgentSession Session(
        int pid, AgentState state, int secondsAgo = 0, string folder = "picadence", string? detail = null) =>
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
            Now,
            false);

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
            var layout = PanelModel.Build([Session(1, state)], Now, false);
            Assert.Equal(state.Describe(), layout.Rows[0].StateWord);
            Assert.Equal(PanelTheme.State(state), PanelTheme.State(layout.Rows[0].State));
        }
    }

    [Fact(DisplayName = "a session row takes no action")]
    public void A_session_row_takes_no_action()
    {
        var layout = PanelModel.Build([Session(1, AgentState.Working)], Now, false);
        var row = layout.Rows[0].Bounds;

        Assert.Equal(PanelHit.Row, layout.HitTest(row.X + 4, row.Y + 4));
        Assert.Equal(PanelHit.None, layout.HitTest(4, 4));
        var quit = layout.Actions.Single(action => action.Action == PanelAction.Quit).Bounds;
        Assert.Equal(PanelHit.Quit, layout.HitTest(quit.X + 2, quit.Y + 2));
        Assert.True(PanelController.After(true, PanelInput.RowClicked));
    }

    [Fact(DisplayName = "an empty panel says nothing is running")]
    public void An_empty_panel_says_nothing_is_running()
    {
        var layout = PanelModel.Build([], Now, false);

        Assert.Empty(layout.Rows);
        Assert.Equal("No sessions are running", layout.EmptyText);
        Assert.False(string.IsNullOrEmpty(layout.EmptyHint));
        var quit = layout.Actions.Single(action => action.Action == PanelAction.Quit).Bounds;
        Assert.Equal(PanelHit.Quit, layout.HitTest(quit.X + 2, quit.Y + 2));
    }

    [Fact(DisplayName = "the panel updates while it stays open")]
    public void The_panel_updates_while_it_stays_open()
    {
        var before = PanelModel.Build([Session(1, AgentState.Working)], Now, false);
        var after = PanelModel.Build([Session(1, AgentState.Blocked)], Now.AddSeconds(3), false);

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

        var layout = PanelModel.Build(sessions, Now, false);

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
            Now,
            false);

        Assert.All(layout.Rows, row =>
        {
            var margin = (row.Bounds.X + row.Bounds.Width) - (row.AgeBounds.X + row.AgeBounds.Width);
            Assert.True(margin >= 8, $"the age is {margin} DIPs from the row edge");
            Assert.True(row.AgeBounds.X > row.Bounds.X);
        });
    }

    [Fact]
    public void A_control_keeps_its_content_inside_its_edges()
    {
        var layout = PanelModel.Build([Session(1, AgentState.Working)], Now, true);

        Assert.All(layout.Actions, action =>
        {
            var labelInset = action.LabelBounds.X - action.Bounds.X;
            var valueInset = (action.Bounds.X + action.Bounds.Width) - (action.ValueBounds.X + action.ValueBounds.Width);
            var centreInset = action.Bounds.Y + (action.Bounds.Height / 2);

            Assert.True(labelInset >= 12, $"{action.Action}: the label is {labelInset} DIPs from the edge");
            Assert.True(valueInset >= 12, $"{action.Action}: the value is {valueInset} DIPs from the edge");
            Assert.Equal(PanelModel.ActionHeight, action.Bounds.Height);
            Assert.InRange(centreInset, action.Bounds.Y, action.Bounds.Y + action.Bounds.Height);
        });
    }

    [Fact]
    public void A_control_labels_its_value_or_leaves_it_empty()
    {
        var on = PanelModel.Build([], Now, true).Actions.Single(action => action.Action == PanelAction.Autostart);
        var off = PanelModel.Build([], Now, false).Actions.Single(action => action.Action == PanelAction.Autostart);
        var quit = PanelModel.Build([], Now, false).Actions.Single(action => action.Action == PanelAction.Quit);

        Assert.Equal("on", on.Value);
        Assert.Equal("off", off.Value);
        Assert.Equal("Quit", quit.Label);
        Assert.Empty(quit.Value);
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
            Now,
            false);

        Assert.Equal("1 waiting, 2 working", layout.Summary);
        Assert.Equal("nothing running", PanelModel.Build([], Now, false).Summary);
    }

    [Fact]
    public void Rows_do_not_overlap_and_the_panel_grows_with_them()
    {
        var one = PanelModel.Build([Session(1, AgentState.Working)], Now, false);
        var two = PanelModel.Build([Session(1, AgentState.Working), Session(2, AgentState.Idle, secondsAgo: 1)], Now, false);

        Assert.True(two.Height > one.Height);
        for (var i = 1; i < two.Rows.Count; i++)
        {
            Assert.True(two.Rows[i].Bounds.Y >= two.Rows[i - 1].Bounds.Y + two.Rows[i - 1].Bounds.Height);
        }

        Assert.True(two.Actions[0].Bounds.Y >= two.Rows[^1].Bounds.Y + two.Rows[^1].Bounds.Height);
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
        new(new SessionRef(pid, $"session-{pid}"), "picadence", null, null, AgentState.Working, null, Now);

    [Fact]
    public void The_quit_button_offers_a_hand_and_a_row_does_not()
    {
        var layout = PanelModel.Build([Session(1)], Now, false);
        var quit = layout.Actions.Single(action => action.Action == PanelAction.Quit).Bounds;
        var autostart = layout.Actions.Single(action => action.Action == PanelAction.Autostart).Bounds;
        var row = layout.Rows[0].Bounds;

        Assert.Equal(PanelHit.Quit, layout.HitTest(quit.X + 4, quit.Y + 4));
        Assert.Equal(PanelHit.Autostart, layout.HitTest(autostart.X + 4, autostart.Y + 4));
        Assert.Equal(PanelPointer.Hand, PanelCursor.For(PanelHit.Autostart));
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
        Assert.Equal("PicadencePanel", windowClass.lpszClassName);
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

public class PanelFlyoutTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 21, 14, 3, 0, TimeSpan.Zero);

    private static AgentSession Session(int pid, string folder = "picadence", int secondsAgo = 0) =>
        new(new SessionRef(pid, $"session-{pid}"), folder, $"name-{pid}", null, AgentState.Working, null,
            Now.AddSeconds(-secondsAgo));

    private static Schedule Armed(int pid, Cadence every, DateTimeOffset nextAt) =>
        new($"session-{pid}", $"name-{pid}", "picadence", every, nextAt, null, 0);

    private static IReadOnlyDictionary<SessionRef, Schedule> Cadences(params Schedule[] schedules) =>
        schedules.ToDictionary(schedule => new SessionRef(PidOf(schedule.SessionId), schedule.SessionId));

    private static int PidOf(string sessionId) => int.Parse(sessionId["session-".Length..], System.Globalization.CultureInfo.InvariantCulture);

    [Fact(DisplayName = "arming a row sets the cadence and shows what comes next")]
    public void Arming_a_row_sets_the_cadence_and_shows_what_comes_next()
    {
        var session = Session(1);
        var schedule = Armed(1, Cadence.Hourly, Now.AddMinutes(24));

        var layout = PanelModel.Build([session], Now, false, Cadences(schedule));
        Assert.Equal("every hour, next in 24m", Assert.Single(layout.Rows).Schedule);
        Assert.Null(layout.Flyout);

        // A right click on the row opens the list, with that cadence chosen.
        var flyoutLayout = PanelModel.Build([session], Now, false, Cadences(schedule), flyoutFor: session.Reference);
        var flyout = Assert.IsType<PanelFlyout>(flyoutLayout.Flyout);

        Assert.Equal(6, flyout.Items.Count);
        var chosen = Assert.Single(flyout.Items, item => item.Selected);
        Assert.Equal(Cadence.Hourly, chosen.Cadence);
        Assert.Equal("every hour", chosen.Label);
        Assert.Contains(flyout.Items, item => item.Cadence == Cadence.Off);
        Assert.Equal(PanelHit.Flyout, flyoutLayout.HitTest(flyout.Bounds.X + 4, flyout.Bounds.Y + 4));
    }

    [Fact(DisplayName = "the cadence list says what it is")]
    public void The_cadence_list_says_what_it_is()
    {
        var layout = PanelModel.Build([Session(1)], Now, false, flyoutFor: Session(1).Reference);
        var flyout = Assert.IsType<PanelFlyout>(layout.Flyout);

        Assert.Equal("Repeat ping", flyout.Header);
        Assert.True(
            flyout.Bounds.Contains(flyout.HeaderBounds.X + 2, flyout.HeaderBounds.Y + 2),
            "the heading is inside the list");

        var headerBottom = flyout.HeaderBounds.Y + flyout.HeaderBounds.Height;
        Assert.True(
            headerBottom <= flyout.Items[0].Bounds.Y,
            $"the first cadence starts at {flyout.Items[0].Bounds.Y}, the heading ends at {headerBottom}");

        // A click on the heading is a click on the list and not on a cadence, so it
        // closes the list instead of arming something.
        var headerPoint = (X: flyout.HeaderBounds.X + 4, Y: flyout.HeaderBounds.Y + 4);
        Assert.Equal(PanelHit.Flyout, layout.HitTest(headerPoint.X, headerPoint.Y));
        Assert.DoesNotContain(flyout.Items, item => item.Bounds.Contains(headerPoint.X, headerPoint.Y));
    }

    [Fact]
    public void An_unarmed_row_says_nothing_about_a_cadence()
    {
        var layout = PanelModel.Build([Session(1)], Now, false);

        Assert.Null(Assert.Single(layout.Rows).Schedule);
        var flyout = Assert.IsType<PanelFlyout>(PanelModel.Build([Session(1)], Now, false, Cadences(), flyoutFor: Session(1).Reference).Flyout);
        Assert.Equal("every 15 minutes", flyout.Items[0].Label);
        Assert.Equal(Cadence.Off, Assert.Single(flyout.Items, item => item.Selected).Cadence);
    }

    [Fact]
    public void The_flyout_stays_inside_the_panel()
    {
        // One row is the awkward case: the list is taller than the panel it sits
        // in, so the panel has to grow rather than clip it.
        foreach (var count in new[] { 1, 2, 8 })
        {
            AssertFlyoutFits(count);
        }
    }

    private static void AssertFlyoutFits(int count)
    {
        var sessions = Enumerable.Range(1, count).Select(pid => Session(pid, secondsAgo: pid)).ToList();

        for (var row = 0; row < sessions.Count; row++)
        {
            var layout = PanelModel.Build(sessions, Now, false, flyoutFor: sessions[row].Reference);
            var flyout = Assert.IsType<PanelFlyout>(layout.Flyout);

            Assert.True(flyout.Bounds.X >= 0, $"row {row}: the list starts at {flyout.Bounds.X}");
            Assert.True(
                flyout.Bounds.X + flyout.Bounds.Width <= layout.Width,
                $"row {row}: the list ends at {flyout.Bounds.X + flyout.Bounds.Width}");
            Assert.True(flyout.Bounds.Y >= 0, $"row {row}: the list starts at {flyout.Bounds.Y}");
            Assert.True(
                flyout.Bounds.Y + flyout.Bounds.Height <= layout.Height,
                $"row {row}: the list ends at {flyout.Bounds.Y + flyout.Bounds.Height}");
            Assert.All(flyout.Items, item => Assert.True(flyout.Bounds.Contains(
                item.Bounds.X, item.Bounds.Y)));
        }
    }

    [Fact]
    public void The_flyout_answers_before_the_row_under_it()
    {
        var layout = PanelModel.Build([Session(1), Session(2, secondsAgo: 1)], Now, false, flyoutFor: new SessionRef(1, "session-1"));
        var flyout = Assert.IsType<PanelFlyout>(layout.Flyout);
        var item = flyout.Items[1];

        Assert.Equal(PanelHit.Flyout, layout.HitTest(item.Bounds.X + 2, item.Bounds.Y + 2));
        Assert.Equal(PanelPointer.Hand, PanelCursor.For(PanelHit.Flyout));

        // A row the list does not cover still answers as a row.
        var uncoveredRow = layout.Rows[^1];
        Assert.Equal(
            PanelHit.Row,
            layout.HitTest(uncoveredRow.Bounds.X + 2, uncoveredRow.Bounds.Y + uncoveredRow.Bounds.Height - 2));
    }
}
