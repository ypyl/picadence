namespace Picadence.Tests;

public sealed class SessionStoreTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 21, 0, 0, TimeSpan.Zero);
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        "picadence-store-" + Guid.NewGuid().ToString("N"));

    public SessionStoreTests()
    {
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private string WriteFile(int pid, string state, string folder = "picadence")
    {
        var name = SessionRef.TryFileName(pid, $"session-{pid}", out var fileName)
            ? fileName
            : throw new InvalidOperationException("the test wrote an unusable name");
        var json = $$"""{"pid":{{pid}},"state":"{{state}}","folder":"{{folder}}","updatedAt":"{{Now:O}}"}""";
        File.WriteAllText(Path.Combine(_directory, name), json);
        return name;
    }

    [Fact(DisplayName = "an ended session is not shown and its state is removed")]
    public void An_ended_session_is_not_shown_and_its_state_is_removed()
    {
        var liveFile = WriteFile(100, "working", folder: "live");
        var deadFile = WriteFile(200, "blocked", folder: "dead");
        var livePath = Path.Combine(_directory, liveFile);
        var before = File.ReadAllText(livePath);

        var store = new SessionStore(_directory, (pid, _, _) => pid == 100);
        var sessions = store.ReadAll();

        var only = Assert.Single(sessions);
        Assert.Equal(100, only.Pid);
        Assert.Equal("live", only.Folder);
        Assert.Equal(AgentState.Working, only.State);

        Assert.False(File.Exists(Path.Combine(_directory, deadFile)), "the ended session's file is removed");
        Assert.True(File.Exists(livePath), "a live session's file is left alone");
        Assert.Equal(before, File.ReadAllText(livePath));
    }

    [Fact(DisplayName = "an unchanged state file is not read again")]
    public void An_unchanged_state_file_is_not_read_again()
    {
        var name = WriteFile(500, "working");
        var path = Path.Combine(_directory, name);
        var store = new SessionStore(_directory, (_, _, _) => true);

        var first = Assert.Single(store.ReadAll());
        var second = Assert.Single(store.ReadAll());
        Assert.Same(first, second);

        // A different length is a different file, so it is parsed again.
        WriteFile(500, "idle");
        var third = Assert.Single(store.ReadAll());
        Assert.NotSame(second, third);
        Assert.Equal(AgentState.Idle, third.State);

        // The same bytes with a new write time are also parsed again, which is how
        // a transition the shell reports late still arrives on the next tick.
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(1));
        var fourth = Assert.Single(store.ReadAll());
        Assert.NotSame(third, fourth);
        Assert.Equal(AgentState.Idle, fourth.State);
    }

    [Fact]
    public void A_file_that_cannot_be_parsed_is_ignored()
    {
        var name = SessionRef.TryFileName(300, "session-300", out var fileName)
            ? fileName
            : throw new InvalidOperationException();
        File.WriteAllText(Path.Combine(_directory, name), "{ this is not json");
        File.WriteAllText(Path.Combine(_directory, "not-a-session.json"), "{}");

        var store = new SessionStore(_directory, (_, _, _) => true);

        Assert.Empty(store.ReadAll());
    }

    [Fact]
    public void A_file_too_large_to_be_state_is_skipped()
    {
        var name = SessionRef.TryFileName(400, "session-400", out var fileName)
            ? fileName
            : throw new InvalidOperationException();
        var padding = new string('x', 128 * 1024);
        var json = "{\"state\":\"working\",\"detail\":\"" + padding + "\"}";
        File.WriteAllText(Path.Combine(_directory, name), json);

        var store = new SessionStore(_directory, (_, _, _) => true);

        Assert.Empty(store.ReadAll());
    }

    [Fact]
    public void A_missing_directory_reads_as_nothing()
    {
        var missing = Path.Combine(_directory, "absent");
        var store = new SessionStore(missing, (_, _, _) => true);

        Assert.Empty(store.ReadAll());
    }
}

public class StalePidTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(), "picadence-stale-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try
        {
            Directory.Delete(_directory, recursive: true);
        }
        catch (IOException)
        {
            // A leaked temp folder is not a test failure.
        }
    }

    private string Write(int pid, string sessionId)
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, $"{pid}-{sessionId}.json");
        File.WriteAllText(
            path,
            "{\"pid\":" + pid + ",\"sessionId\":\"" + sessionId +
            "\",\"folder\":\"folio\",\"state\":\"done\",\"updatedAt\":\"2026-11-23T09:15:20Z\"}");
        return path;
    }

    [Fact(DisplayName = "a reused process id is not a live session")]
    public void A_reused_process_id_is_not_a_live_session()
    {
        // Windows gave this number to a process that started long after the session
        // wrote its file, so that process cannot be the session.
        var path = Write(29240, "01a1fe23-73f9-76de-ab0c-d28fa6358e3f");
        var written = File.GetLastWriteTimeUtc(path);
        var reused = written.AddHours(2);

        var store = new SessionStore(
            _directory,
            (_, fileWritten, _) => SessionStore.StartedBefore(reused, fileWritten));

        Assert.Empty(store.ReadAll());
        Assert.False(File.Exists(path), "the state a dead session left behind is removed");
    }

    [Fact(DisplayName = "a session that started before its file is shown")]
    public void A_session_that_started_before_its_file_is_shown()
    {
        var path = Write(100, "session-1");
        var written = File.GetLastWriteTimeUtc(path);

        var store = new SessionStore(
            _directory,
            (_, fileWritten, _) => SessionStore.StartedBefore(written.AddHours(-3), fileWritten));

        Assert.Single(store.ReadAll());
        Assert.True(File.Exists(path));
    }

    [Theory(DisplayName = "a process may start a moment after its file is stamped")]
    [InlineData(-1.0, true)]
    [InlineData(0.0, true)]
    [InlineData(1.0, true)]
    [InlineData(3.0, false)]
    public void A_process_may_start_a_moment_after_its_file_is_stamped(double offsetSeconds, bool expected)
    {
        var written = new DateTime(2026, 11, 23, 9, 15, 20, DateTimeKind.Utc);

        Assert.Equal(expected, SessionStore.StartedBefore(written.AddSeconds(offsetSeconds), written));
    }
}

public class StartedTogetherTests
{
    private static readonly DateTime Started = new(2026, 11, 23, 9, 15, 12, DateTimeKind.Utc);

    [Theory(DisplayName = "the start time in the file has to be the process's own")]
    [InlineData(0.0, true)]
    [InlineData(1.5, true)]
    [InlineData(-1.5, true)]
    [InlineData(30.0, false)]
    [InlineData(-30.0, false)]
    public void The_start_time_in_the_file_has_to_be_the_processs_own(double offsetSeconds, bool expected)
    {
        // Two clocks write these: the extension derives it from uptime, the kernel
        // reports it exactly. A couple of seconds apart is the same process.
        Assert.Equal(expected, SessionStore.StartedTogether(Started.AddSeconds(offsetSeconds), Started));
    }

    [Fact(DisplayName = "a file that names a different start is not this session")]
    public void A_file_that_names_a_different_start_is_not_this_session()
    {
        var directory = Path.Combine(Path.GetTempPath(), "picadence-start-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "29240-01a1fe23-73f9-76de-ab0c-d28fa6358e3f.json");
            File.WriteAllText(
                path,
                "{\"pid\":29240,\"sessionId\":\"01a1fe23-73f9-76de-ab0c-d28fa6358e3f\",\"folder\":\"folio\"," +
                "\"state\":\"done\",\"startedAt\":\"2026-11-23T09:15:12.000Z\",\"updatedAt\":\"2026-11-23T09:15:20Z\"}");

            // The process holding 29240 now started six days earlier: not the writer.
            var store = new SessionStore(
                directory,
                (_, _, claimed) => claimed is { } start
                    && SessionStore.StartedTogether(Started.AddDays(-6), start.UtcDateTime));

            Assert.Empty(store.ReadAll());
            Assert.False(File.Exists(path));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact(DisplayName = "a file from an older extension still falls back to the write time")]
    public void A_file_from_an_older_extension_still_falls_back_to_the_write_time()
    {
        var directory = Path.Combine(Path.GetTempPath(), "picadence-old-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "100-session-1.json");
            File.WriteAllText(
                path,
                "{\"pid\":100,\"sessionId\":\"session-1\",\"folder\":\"x\",\"state\":\"idle\"}");

            var written = File.GetLastWriteTimeUtc(path);
            DateTimeOffset? seen = null;
            var store = new SessionStore(
                directory,
                (_, fileWritten, claimed) =>
                {
                    seen = claimed;
                    return SessionStore.StartedBefore(written.AddMinutes(-5), fileWritten);
                });

            Assert.Single(store.ReadAll());
            Assert.Null(seen);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
