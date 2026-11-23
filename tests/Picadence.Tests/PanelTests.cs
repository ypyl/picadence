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

    [Fact(DisplayName = "the panel shows five rows and says where the rest are")]
    public void The_panel_shows_five_rows_and_says_where_the_rest_are()
    {
        var sessions = Enumerable.Range(1, 12)
            .Select(pid => Session(pid, AgentState.Working, secondsAgo: pid))
            .ToList();

        var layout = PanelModel.Build(sessions, Now, false);

        Assert.Equal(PanelModel.VisibleRows, layout.Rows.Count);
        Assert.Equal(12, layout.TotalRows);
        Assert.Equal(0, layout.FirstRow);
        Assert.Equal("1\u20135 of 12  \u00b7  scroll", layout.ScrollText);

        // And nothing to say when everything fits.
        var small = PanelModel.Build(sessions.Take(5).ToList(), Now, false);
        Assert.Equal(5, small.Rows.Count);
        Assert.Null(small.ScrollText);
    }

    [Fact(DisplayName = "scrolling reaches the rows past the fifth")]
    public void Scrolling_reaches_the_rows_past_the_fifth()
    {
        var sessions = Enumerable.Range(1, 12)
            .Select(pid => Session(pid, AgentState.Working, secondsAgo: pid))
            .ToList();

        // Newest first, so the last row of the list is the oldest session.
        var end = PanelModel.Build(sessions, Now, false, scroll: 99);

        Assert.Equal(7, end.FirstRow);
        Assert.Equal(PanelModel.VisibleRows, end.Rows.Count);
        Assert.Equal("8\u201312 of 12  \u00b7  scroll", end.ScrollText);
        Assert.Equal("picadence", end.Rows[^1].Folder);
        Assert.Equal(PanelModel.FirstRow(99, 12), end.FirstRow);

        // A list that fits does not scroll at all.
        Assert.Equal(0, PanelModel.FirstRow(-3, 4));
        Assert.Equal(0, PanelModel.FirstRow(0, 0));
    }

    [Fact(DisplayName = "kept rows are a scroll away, not a casualty of the window")]
    public void Kept_rows_are_a_scroll_away_not_a_casualty_of_the_window()
    {
        var sessions = Enumerable.Range(1, 5)
            .Select(pid => Session(pid, AgentState.Working, secondsAgo: pid))
            .ToList();
        var kept = new[]
        {
            new KeptSession(
                new Pin("01a1fe3b-e1c8-71eb-afd8-f796496f0f78", "docs-site", @"C:\Users\me\docs-site", null, Now.AddHours(-1)),
                new RestartPlan(RestartRefusal.None, new Launch("wt.exe", "wt.exe", @"C:\Users\me\docs-site"), string.Empty)),
        };

        var top = PanelModel.Build(sessions, Now, false, kept: kept);
        Assert.Equal(PanelModel.VisibleRows, top.Rows.Count);
        Assert.DoesNotContain(top.Rows, row => row.Kind == PanelRowKind.Kept);

        var scrolled = PanelModel.Build(sessions, Now, false, kept: kept, scroll: 5);
        Assert.Equal(PanelRowKind.Kept, scrolled.Rows[^1].Kind);
        Assert.Equal(5, scrolled.Rows.Count);
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

    [Fact(DisplayName = "the two controls share one row, the checkbox left and Quit right")]
    public void The_two_controls_share_one_row_the_checkbox_left_and_quit_right()
    {
        var layout = PanelModel.Build([Session(1, AgentState.Working)], Now, true);

        var checkbox = layout.Actions.Single(action => action.Action == PanelAction.Autostart);
        var quit = layout.Actions.Single(action => action.Action == PanelAction.Quit);

        Assert.Equal(2, layout.Actions.Count);
        Assert.Equal(checkbox.Bounds.Y, quit.Bounds.Y);
        Assert.Equal(checkbox.Bounds.Height, quit.Bounds.Height);
        Assert.True(checkbox.Bounds.X < quit.Bounds.X, "the checkbox is the left one");
        Assert.True(
            quit.Bounds.X + quit.Bounds.Width > checkbox.Bounds.X + checkbox.Bounds.Width,
            "Quit sits to its right");
        Assert.Equal(PanelModel.Padding, checkbox.Bounds.X);
        Assert.True(
            Math.Abs(quit.Bounds.X + quit.Bounds.Width - (PanelModel.Width - PanelModel.Padding)) < 0.001,
            "Quit ends at the panel's right inset");
        Assert.False(checkbox.Bounds.Contains(quit.Bounds.X, quit.Bounds.Y), "and they do not overlap");

        // Neither control hangs off the panel, and the content stays inside.
        Assert.All(layout.Actions, action =>
        {
            Assert.InRange(action.LabelBounds.X, action.Bounds.X, action.Bounds.X + action.Bounds.Width);
            Assert.True(
                action.LabelBounds.X + action.LabelBounds.Width <= action.Bounds.X + action.Bounds.Width,
                $"{action.Action}: the content hangs past its control");
            Assert.Equal(PanelModel.ActionHeight, action.Bounds.Height);
        });
    }

    [Fact(DisplayName = "a control is as wide as what it says")]
    public void A_control_is_as_wide_as_what_it_says()
    {
        // A wider measurement makes a wider checkbox, and the X stays one glyph wide.
        var narrow = PanelModel.Build([], Now, false, measure: _ => 40);
        var wide = PanelModel.Build([], Now, false, measure: _ => 120);

        var narrowBox = narrow.Actions.Single(action => action.Action == PanelAction.Autostart);
        var wideBox = wide.Actions.Single(action => action.Action == PanelAction.Autostart);

        Assert.Equal(80, wideBox.Bounds.Width - narrowBox.Bounds.Width);
        Assert.True(
            wideBox.LabelBounds.Width - narrowBox.LabelBounds.Width == 80,
            "the label gets the room, not the inset");

        var quit = wide.Actions.Single(action => action.Action == PanelAction.Quit);
        Assert.Equal(
            (2 * (PanelModel.ActionInset + PanelModel.ActionPadding)) + PanelModel.IconSize,
            quit.Bounds.Width);

        // And the panel does not widen for either of them.
        Assert.Equal(PanelModel.Width, wide.Width);
    }

    [Fact]
    public void The_checkbox_shows_whether_starting_with_windows_is_on()
    {
        var on = PanelModel.Build([], Now, true).Actions.Single(action => action.Action == PanelAction.Autostart);
        var off = PanelModel.Build([], Now, false).Actions.Single(action => action.Action == PanelAction.Autostart);
        var quit = PanelModel.Build([], Now, false).Actions.Single(action => action.Action == PanelAction.Quit);

        Assert.True(on.Checked);
        Assert.False(off.Checked);
        Assert.Equal("Autostart", on.Label);
        Assert.Equal(PanelModel.CheckSize, on.BoxBounds.Width);
        Assert.True(on.BoxBounds.Height > 0, "the box has a height");
        Assert.Equal(on.BoxBounds.Y + (on.BoxBounds.Height / 2), on.Bounds.Y + (on.Bounds.Height / 2));

        // Quit is one glyph: no words, no box, nothing checked.
        Assert.Equal(string.Empty, quit.Label);
        Assert.Equal(PanelModel.QuitGlyph, quit.Glyph);
        Assert.Equal(default, quit.BoxBounds);
        Assert.False(quit.Checked);
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

        // The cadences and nothing else: keeping moved onto the row's pin.
        Assert.Equal(6, flyout.Items.Count);
        Assert.All(flyout.Items, item => Assert.Equal(PanelFlyoutAction.Cadence, item.Action));
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

        foreach (var session in sessions)
        {
            var layout = PanelModel.Build(sessions, Now, false, flyoutFor: session.Reference);
            if (layout.Flyout is null)
            {
                // Past the fifth row the list is a scroll away, and a row that is not
                // on screen has nothing to open.
                Assert.DoesNotContain(layout.Rows, row => row.Reference == session.Reference);
                continue;
            }

            var flyout = layout.Flyout;

            Assert.True(flyout.Bounds.X >= 0, $"{session.Folder}: the list starts at {flyout.Bounds.X}");
            Assert.True(
                flyout.Bounds.X + flyout.Bounds.Width <= layout.Width,
                $"{session.Folder}: the list ends at {flyout.Bounds.X + flyout.Bounds.Width}");
            Assert.True(flyout.Bounds.Y >= 0, $"{session.Folder}: the list starts at {flyout.Bounds.Y}");
            Assert.True(
                flyout.Bounds.Y + flyout.Bounds.Height <= layout.Height,
                $"{session.Folder}: the list ends at {flyout.Bounds.Y + flyout.Bounds.Height}");
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

public class PanelKeptTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 22, 20, 0, 0, TimeSpan.Zero);
    private const string KeptId = "01a1fe3b-e1c8-71eb-afd8-f796496f0f78";

    private static AgentSession Live(int pid, string sessionId = "live-1") =>
        new(new SessionRef(pid, sessionId), "trayclaw", "trayclaw", null, AgentState.Working, null, Now.AddSeconds(-30));

    private static Pin Pin(string sessionId = KeptId, string folder = @"C:\Users\me\docs-site") =>
        new(sessionId, "docs-site", folder, @"C:\Users\me\.pi\agent\sessions\x\2026-11-22T09-00-00-000Z_" + sessionId + ".jsonl", Now.AddHours(-2));

    private static KeptSession Kept(Pin pin, RestartRefusal refusal = RestartRefusal.None, string note = "") =>
        new(pin, new RestartPlan(refusal, refusal == RestartRefusal.None ? new Launch("wt.exe", "wt.exe", pin.Folder) : null, note));

    [Fact(DisplayName = "a kept session stays after it ends")]
    public void A_kept_session_stays_after_it_ends()
    {
        var layout = PanelModel.Build([], Now, false, kept: [Kept(Pin())]);

        var row = Assert.Single(layout.Rows);
        Assert.Equal(PanelRowKind.Kept, row.Kind);
        Assert.Equal("not running", row.StateWord);
        // A row names a place briefly, the way a live row does.
        Assert.Equal("docs-site", row.Folder);
        // A plain age: the word "kept" was the widest phrase in the column, and the
        // row already says it is not running under a solid star.
        Assert.Equal("2h", row.Age);
        Assert.Equal("nothing running", layout.Summary);

        // Kept rows come after the ones that are running.
        var withLive = PanelModel.Build([Live(9)], Now, false, kept: [Kept(Pin())]);
        Assert.Equal(PanelRowKind.Live, withLive.Rows[0].Kind);
        Assert.Equal(PanelRowKind.Kept, withLive.Rows[1].Kind);
    }

    [Fact(DisplayName = "a kept session that is running is one row, not two")]
    public void A_kept_session_that_is_running_is_one_row_not_two()
    {
        var pin = Pin("live-1");
        var layout = PanelModel.Build([Live(9)], Now, false, kept: [Kept(pin)]);

        var row = Assert.Single(layout.Rows);
        Assert.Equal(PanelRowKind.Live, row.Kind);
        Assert.True(row.Keeping);
    }

    [Fact(DisplayName = "a missing conversation is named")]
    public void A_missing_conversation_is_named()
    {
        var pin = Pin();
        var layout = PanelModel.Build(
            [], Now, false, flyoutFor: new SessionRef(0, KeptId),
            kept: [Kept(pin, RestartRefusal.NoConversation, "the conversation is gone")]);

        var row = Assert.Single(layout.Rows);
        Assert.Equal("the conversation is gone", row.Detail);
        Assert.False(row.CanRestart);

        // Nothing left to offer: no menu at all, and the row says why.
        Assert.Null(layout.Flyout);
    }

    [Fact(DisplayName = "a missing terminal is named")]
    public void A_missing_terminal_is_named()
    {
        var layout = PanelModel.Build(
            [], Now, false, kept: [Kept(Pin(), RestartRefusal.NoTerminal, "windows terminal is not installed")]);

        Assert.Equal("windows terminal is not installed", Assert.Single(layout.Rows).Detail);
    }

    [Fact(DisplayName = "a kept row has no menu, because its actions are on it")]
    public void A_kept_row_has_no_menu_because_its_actions_are_on_it()
    {
        var layout = PanelModel.Build([], Now, false, flyoutFor: new SessionRef(0, KeptId), kept: [Kept(Pin())]);

        Assert.Null(layout.Flyout);
        var row = Assert.Single(layout.Rows);
        Assert.True(row.CanRestart);
        Assert.Equal(string.Empty, row.Detail);
    }
}

public class PanelPinTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 22, 20, 0, 0, TimeSpan.Zero);
    private const string KeptId = "01a1fe3b-e1c8-71eb-afd8-f796496f0f78";

    private static AgentSession Live(int pid) =>
        new(new SessionRef(pid, $"session-{pid}"), "trayclaw", "trayclaw", null, AgentState.Done, null, Now);

    private static KeptSession Kept(string sessionId = KeptId) => new(
        new Pin(sessionId, "docs-site", @"C:\Users\me\docs-site", null, Now.AddHours(-1)),
        new RestartPlan(
            RestartRefusal.None,
            new Launch("wt.exe", "wt.exe", @"C:\Users\me\docs-site"),
            string.Empty));

    [Fact(DisplayName = "the pin control keeps and unkeeps a session")]
    public void The_pin_control_keeps_and_unkeeps_a_session()
    {
        var layout = PanelModel.Build([Live(1)], Now, false);
        var row = Assert.Single(layout.Rows);

        Assert.False(row.Keeping);
        Assert.Equal(
            PanelHit.Pin,
            layout.HitTest(
                row.PinBounds.X + (row.PinBounds.Width / 2),
                row.PinBounds.Y + (row.PinBounds.Height / 2)));

        // Kept, and the same click again is the way back.
        // The same session, this time kept: one row, and the pin says so.
        var kept = PanelModel.Build([Live(1)], Now, false, kept: [Kept("session-1")]);
        Assert.True(Assert.Single(kept.Rows).Keeping);
        Assert.Equal(
            PanelHit.Pin,
            kept.HitTest(kept.Rows[0].PinBounds.X + 4, kept.Rows[0].PinBounds.Y + 4));
    }

    [Fact(DisplayName = "the pin sits inside the row and before the age")]
    public void The_pin_sits_inside_the_row_and_before_the_age()
    {
        var layout = PanelModel.Build([Live(1)], Now, false);

        var row = Assert.Single(layout.Rows);
        Assert.True(row.Bounds.Contains(row.PinBounds.X, row.PinBounds.Y), "inside the row");
        Assert.True(
            row.Bounds.Contains(row.PinBounds.X + row.PinBounds.Width, row.PinBounds.Y),
            "and its right edge too");
        Assert.True(row.PinBounds.X + row.PinBounds.Width <= row.AgeBounds.X, "before the age");
        Assert.Equal(PanelModel.PinSize, row.PinBounds.Width);
    }

    [Fact(DisplayName = "a row body is not a control")]
    public void A_row_body_is_not_a_control()
    {
        var layout = PanelModel.Build([Live(1)], Now, false);
        var row = Assert.Single(layout.Rows);

        Assert.Equal(PanelHit.Row, layout.HitTest(row.Bounds.X + 40, row.Bounds.Y + 10));
        Assert.Equal(PanelPointer.Arrow, PanelCursor.For(PanelHit.Row));
        Assert.Equal(PanelPointer.Hand, PanelCursor.For(PanelHit.Pin));
    }

    [Fact(DisplayName = "the start control restarts a kept session")]
    public void The_start_control_restarts_a_kept_session()
    {
        var layout = PanelModel.Build([], Now, false, kept: [Kept()]);
        var row = Assert.Single(layout.Rows);

        Assert.True(row.StartBounds.Width > 0);
        Assert.Equal(PanelHit.Start, layout.HitTest(
            row.StartBounds.X + (row.StartBounds.Width / 2),
            row.StartBounds.Y + (row.StartBounds.Height / 2)));
        Assert.Equal(PanelPointer.Hand, PanelCursor.For(PanelHit.Start));

        // Left of the pin, inside the row, and the pin still answers as the pin.
        Assert.True(row.StartBounds.X + row.StartBounds.Width <= row.PinBounds.X, "before the pin");
        Assert.True(row.Bounds.Contains(row.StartBounds.X, row.StartBounds.Y), "inside the row");
        Assert.Equal(PanelHit.Pin, layout.HitTest(row.PinBounds.X + 4, row.PinBounds.Y + 4));

        // And the row's own body still does nothing.
        Assert.Equal(PanelHit.Row, layout.HitTest(row.Bounds.X + 40, row.Bounds.Y + 10));
    }

    [Fact(DisplayName = "a kept row that cannot restart has no start control")]
    public void A_kept_row_that_cannot_restart_has_no_start_control()
    {
        var pin = new Pin(
            "01a1fe3b-e1c8-71eb-afd8-f796496f0f78",
            "docs-site",
            @"C:\Users\me\docs-site",
            null,
            Now.AddHours(-1));
        var layout = PanelModel.Build(
            [],
            Now,
            false,
            kept:
            [
                new KeptSession(
                    pin,
                    new RestartPlan(RestartRefusal.NoConversation, null, "the conversation is gone")),
            ]);

        var row = Assert.Single(layout.Rows);
        Assert.Equal(default, row.StartBounds);
        Assert.Equal(PanelHit.Pin, layout.HitTest(row.PinBounds.X + 4, row.PinBounds.Y + 4));
    }

    [Fact(DisplayName = "a kept row shows the solid star and an unkept row the outline")]
    public void A_kept_row_shows_the_solid_star_and_an_unkept_row_the_outline()
    {
        // Stars, not pins: at 12 pixels a pin reads as a scribble and a star reads as
        // a star, and the outline still says "not kept" by its shape alone.
        Assert.Equal("\uE734", PanelModel.KeepGlyph);
        Assert.Equal("\uE735", PanelModel.KeptGlyph);
        Assert.NotEqual(PanelModel.KeepGlyph, PanelModel.KeptGlyph);
    }
}

public class PanelDependencyTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 22, 23, 0, 0, TimeSpan.Zero);

    private static AgentSession Session(int pid) =>
        new(new SessionRef(pid, $"session-{pid}"), "trayclaw", "trayclaw", null, AgentState.Working, null, Now);

    [Fact(DisplayName = "a missing pi is named instead of an empty list")]
    public void A_missing_pi_is_named_instead_of_an_empty_list()
    {
        var layout = PanelModel.Build([], Now, false, dependencies: new Dependencies(Pi: false, Terminal: true));

        Assert.Empty(layout.Rows);
        Assert.Equal("pi is not installed", layout.EmptyText);
        Assert.Contains("pi.dev", layout.EmptyHint, StringComparison.Ordinal);
        Assert.Null(layout.Notice);

        // And with pi present, a quiet machine says it is quiet.
        var quiet = PanelModel.Build([], Now, false, dependencies: Dependencies.Present);
        Assert.Equal("No sessions are running", quiet.EmptyText);
        Assert.Equal("Start pi to see it here", quiet.EmptyHint);
    }

    [Fact(DisplayName = "a missing terminal is named beside the sessions")]
    public void A_missing_terminal_is_named_beside_the_sessions()
    {
        var layout = PanelModel.Build(
            [Session(1)],
            Now,
            false,
            dependencies: new Dependencies(Pi: true, Terminal: false));

        Assert.Single(layout.Rows);
        Assert.Equal(
            "windows terminal is not installed \u00b7 a session cannot be started",
            layout.Notice);
        Assert.True(layout.NoticeBounds.Y > layout.Rows[^1].Bounds.Y, "below the list");
        Assert.True(
            layout.NoticeBounds.Y < layout.Actions[0].Bounds.Y,
            "and above the controls");

        // Both present, and there is nothing to say.
        var fine = PanelModel.Build([Session(1)], Now, false, dependencies: Dependencies.Present);
        Assert.Null(fine.Notice);
        Assert.Equal(default, fine.NoticeBounds);
    }

    [Fact(DisplayName = "a missing dependency does not stop a session being kept and started")]
    public void A_missing_dependency_does_not_stop_the_rest_of_the_panel()
    {
        var pin = new Pin(
            "01a1fe3b-e1c8-71eb-afd8-f796496f0f78",
            "docs-site",
            @"C:\Users\me\docs-site",
            null,
            Now.AddHours(-1));
        var kept = new KeptSession(
            pin,
            new RestartPlan(RestartRefusal.NoTerminal, null, "windows terminal is not installed"));

        var layout = PanelModel.Build(
            [],
            Now,
            false,
            kept: [kept],
            dependencies: new Dependencies(Pi: true, Terminal: false));

        var row = Assert.Single(layout.Rows);
        Assert.Equal("not running", row.StateWord);
        Assert.Equal("windows terminal is not installed", row.Detail);
        Assert.Equal(default, row.StartBounds);
        Assert.NotNull(layout.Notice);
        Assert.Equal(2, layout.Actions.Count);
    }
}
