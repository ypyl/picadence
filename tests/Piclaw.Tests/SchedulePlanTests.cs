namespace Piclaw.Tests;

public class SchedulePlanTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 21, 14, 3, 0, TimeSpan.Zero);

    private static AgentSession Session(
        string sessionId, AgentState state, string folder = "triage", int pid = 100, string? name = "news-watch") =>
        new(new SessionRef(pid, sessionId), folder, name, null, state, null, Now.AddMinutes(-1));

    private static Schedule Armed(
        string sessionId, Cadence every = Cadence.Hourly, DateTimeOffset? nextAt = null) =>
        new(sessionId, "news-watch", "triage", every, nextAt ?? Now.AddMinutes(-1), null, 0);

    [Fact(DisplayName = "a due cadence wakes an idle session")]
    public void A_due_cadence_wakes_an_idle_session()
    {
        var schedule = Armed("9f2c-1a");
        var tick = SchedulePlan.Plan([Session("9f2c-1a", AgentState.Idle)], [schedule], Now);

        var ping = Assert.Single(tick.Pings);
        Assert.Equal("9f2c-1a", ping.Schedule.SessionId);
        Assert.Same(schedule, ping.Schedule);

        var written = Assert.Single(tick.Written);
        Assert.Equal(Now, written.LastIssuedAt);
        Assert.Equal(Now.AddHours(1), written.NextAt);
        Assert.Equal(0, written.Missed);

        Assert.True(tick.View.TryGetValue(new SessionRef(100, "9f2c-1a"), out var view));
        Assert.Equal(written.NextAt, view.NextAt);
    }

    [Fact(DisplayName = "a busy session is skipped and counted as missed")]
    public void A_busy_session_is_skipped_and_counted_as_missed()
    {
        var schedule = Armed("9f2c-1a");

        foreach (var state in new[] { AgentState.Working, AgentState.Blocked })
        {
            var tick = SchedulePlan.Plan([Session("9f2c-1a", state)], [schedule], Now);
            Assert.Empty(tick.Pings);
            Assert.Empty(tick.Written);
        }

        // One window later the session is free, and the window it lost is reported
        // rather than quietly dropped.
        var later = Now.AddHours(1).AddMinutes(1);
        var after = SchedulePlan.Plan([Session("9f2c-1a", AgentState.Idle)], [schedule], later);

        var ping = Assert.Single(after.Pings);
        Assert.Contains("1 window missed", ping.Text, StringComparison.Ordinal);
        Assert.Equal(later.AddHours(1), Assert.Single(after.Written).NextAt);
    }

    [Fact(DisplayName = "a cadence never wakes another session")]
    public void A_cadence_never_wakes_another_session()
    {
        var tick = SchedulePlan.Plan(
            [
                Session("9f2c-1a", AgentState.Idle),
                Session("other-2b", AgentState.Idle, folder: "ops", pid: 200, name: "ops"),
            ],
            [Armed("9f2c-1a")],
            Now);

        var ping = Assert.Single(tick.Pings);
        Assert.Equal("9f2c-1a", ping.Schedule.SessionId);
        Assert.DoesNotContain(tick.Written, schedule => schedule.SessionId == "other-2b");
        Assert.False(tick.View.ContainsKey(new SessionRef(200, "other-2b")));
    }

    [Fact(DisplayName = "a gap produces one wake-up and says how many windows were missed")]
    public void A_gap_produces_one_wake_up_and_says_how_many_windows_were_missed()
    {
        var schedule = Armed("9f2c-1a", Cadence.Hourly, Now.AddDays(-1));

        var tick = SchedulePlan.Plan([Session("9f2c-1a", AgentState.Idle)], [schedule], Now);

        var ping = Assert.Single(tick.Pings);
        Assert.Contains("23 windows missed", ping.Text, StringComparison.Ordinal);
        Assert.Equal(23, Assert.Single(tick.Written).Missed);
        Assert.Equal(Now.AddHours(1), Assert.Single(tick.Written).NextAt);
    }

    [Fact(DisplayName = "an unarmed session is never woken")]
    public void An_unarmed_session_is_never_woken()
    {
        var tick = SchedulePlan.Plan([Session("9f2c-1a", AgentState.Idle)], [], Now);

        Assert.Empty(tick.Pings);
        Assert.Empty(tick.Written);
        Assert.Empty(tick.View);
    }

    [Fact(DisplayName = "the tray never starts a session")]
    public void The_tray_never_starts_a_session()
    {
        var schedule = Armed("9f2c-1a", Cadence.Every15Minutes, Now.AddDays(-30));

        var tick = SchedulePlan.Plan([], [schedule], Now);

        Assert.Empty(tick.Pings);
        Assert.Empty(tick.Written);
        Assert.Empty(tick.View);
        // The window is not lost: the record has not moved, so it is still due
        // when that session is resumed.
        Assert.True(schedule.IsDue(Now));
    }

    [Fact(DisplayName = "the wake-up names piclaw and the cadence and gives no instruction")]
    public void The_wake_up_names_piclaw_and_the_cadence_and_gives_no_instruction()
    {
        var schedule = Armed("9f2c-1a");
        var tick = SchedulePlan.Plan([Session("9f2c-1a", AgentState.Idle)], [schedule], Now);

        var ping = Assert.Single(tick.Pings);
        var lines = ping.Text.Split('\n');

        Assert.Equal(3, lines.Length);
        Assert.Equal("piclaw ping - 14:03, every hour", lines[0]);
        Assert.Equal("Automated wake-up from the piclaw tray app. Not a message from the user.", lines[1]);
        Assert.Equal("Follow this session's standing instructions.", lines[2]);
        Assert.DoesNotContain("check", ping.Text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("should", ping.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void The_ping_file_carries_the_words_and_the_moment()
    {
        var schedule = Armed("9f2c-1a", Cadence.Hourly, Now.AddHours(-2));
        var ping = Assert.Single(SchedulePlan.Plan([Session("9f2c-1a", AgentState.Idle)], [schedule], Now).Pings);

        var json = PingFile.ToJson(ping);

        Assert.Contains("\"at\":\"2026-11-21T14:03:00", json, StringComparison.Ordinal);
        Assert.Contains("\"every\":\"1h\"", json, StringComparison.Ordinal);
        Assert.Contains("\"missed\":1", json, StringComparison.Ordinal);
        Assert.Contains("piclaw ping - 14:03, every hour", json, StringComparison.Ordinal);
    }

    [Fact]
    public void A_ping_file_is_written_for_the_session_it_belongs_to()
    {
        var root = Path.Combine(Path.GetTempPath(), "piclaw-pings-" + Guid.NewGuid().ToString("N"));
        try
        {
            var schedule = Armed("9f2c-1a");
            var ping = Assert.Single(SchedulePlan.Plan([Session("9f2c-1a", AgentState.Idle)], [schedule], Now).Pings);

            Assert.True(PingFile.Write(Path.Combine(root, "pings"), ping));

            var written = File.ReadAllText(Path.Combine(root, "pings", "9f2c-1a.json"));
            Assert.Contains("piclaw ping", written, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void A_ping_that_cannot_be_written_is_reported_so_the_window_can_be_retried()
    {
        var root = Path.Combine(Path.GetTempPath(), "piclaw-blocked-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(root);
            var blocker = Path.Combine(root, "blocked");
            File.WriteAllText(blocker, "not a directory");
            var schedule = Armed("9f2c-1a");
            var ping = Assert.Single(SchedulePlan.Plan([Session("9f2c-1a", AgentState.Idle)], [schedule], Now).Pings);

            Assert.False(PingFile.Write(Path.Combine(blocker, "pings"), ping));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void The_label_follows_the_session_and_a_finished_one_still_counts_as_free()
    {
        var moved = Armed("9f2c-1a") with { Folder = "old-folder", Name = "old-name" };

        var tick = SchedulePlan.Plan(
            [Session("9f2c-1a", AgentState.Done, folder: "new-folder", name: "new-name")],
            [moved],
            Now);

        var written = Assert.Single(tick.Written);
        Assert.Equal("new-folder", written.Folder);
        Assert.Equal("new-name", written.Name);
        Assert.Single(tick.Pings);
    }
}
