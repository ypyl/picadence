namespace Piclaw.Tests;

public class ScheduleStoreTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 11, 21, 14, 3, 0, TimeSpan.Zero);

    private readonly string _root = Path.Combine(
        Path.GetTempPath(), "piclaw-schedules-" + Guid.NewGuid().ToString("N"));

    private string Schedules => Path.Combine(_root, "schedules");

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            System.IO.Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    private static Schedule Armed(string sessionId, Cadence every, DateTimeOffset nextAt) =>
        new(sessionId, "news-watch", "triage", every, nextAt, null, 0);

    [Fact]
    public void A_cadence_survives_the_round_trip_to_disk()
    {
        var store = new ScheduleStore(Schedules);
        var schedule = Armed("9f2c-1a", Cadence.Hourly, Now) with
        {
            LastIssuedAt = Now.AddHours(-1),
            Missed = 3,
        };

        store.Write(schedule);
        var read = Assert.Single(store.ReadAll());

        Assert.Equal(schedule.SessionId, read.SessionId);
        Assert.Equal(schedule.Name, read.Name);
        Assert.Equal(schedule.Folder, read.Folder);
        Assert.Equal(schedule.Every, read.Every);
        Assert.Equal(schedule.NextAt, read.NextAt);
        Assert.Equal(schedule.LastIssuedAt, read.LastIssuedAt);
        Assert.Equal(3, read.Missed);
        Assert.True(store.TryGet("9f2c-1a", out var single));
        Assert.Equal(schedule, single);
    }

    [Fact]
    public void An_unreadable_or_oversized_file_is_ignored()
    {
        System.IO.Directory.CreateDirectory(Schedules);
        File.WriteAllText(Path.Combine(Schedules, "bad-1.json"), "not json");
        File.WriteAllText(Path.Combine(Schedules, "wrong-2.json"), "{\"sessionId\":\"other\",\"every\":\"1h\",\"nextAt\":\"2026-11-21T14:03:00Z\"}");
        File.WriteAllText(Path.Combine(Schedules, "big-3.json"), new string('x', 70 * 1024));
        File.WriteAllText(Path.Combine(Schedules, "unknown-4.json"), "{\"sessionId\":\"unknown-4\",\"every\":\"9y\",\"nextAt\":\"2026-11-21T14:03:00Z\"}");

        var store = new ScheduleStore(Schedules);

        Assert.Empty(store.ReadAll());
        Assert.False(store.TryGet("bad-1", out _));
    }

    [Fact(DisplayName = "an unused cadence is still remembered a year later")]
    public void An_unused_cadence_is_still_remembered_a_year_later()
    {
        var store = new ScheduleStore(Schedules);
        var armed = Armed("9f2c-1a", Cadence.Weekly, Now);
        store.Write(armed);

        // Four hundred days later, with a new store instance, as a restarted tray
        // would read it.
        var later = nowAfter(TimeSpan.FromDays(400));
        var reread = Assert.Single(new ScheduleStore(Schedules).ReadAll());

        Assert.Equal(armed, reread);
        Assert.True(reread.IsDue(later));
        Assert.Equal(57, Cadence.Weekly.MissedBetween(reread.NextAt, later));
    }

    [Fact(DisplayName = "turning a cadence off removes it")]
    public void Turning_a_cadence_off_removes_it()
    {
        var store = new ScheduleStore(Schedules);
        store.Write(Armed("9f2c-1a", Cadence.Hourly, Now));

        store.Remove("9f2c-1a");

        Assert.Empty(store.ReadAll());
        Assert.False(store.TryGet("9f2c-1a", out _));
    }

    [Fact]
    public void A_session_id_that_could_be_a_path_is_refused()
    {
        var store = new ScheduleStore(Schedules);

        Assert.False(store.TryGet("../escape", out _));
        Assert.Throws<ArgumentException>(() => store.Write(Armed("../escape", Cadence.Hourly, Now)));
        store.Remove("../escape");
    }

    private static DateTimeOffset nowAfter(TimeSpan time) => Now + time;
}
