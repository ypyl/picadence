namespace Trayclaw.Tests;

public sealed class SessionStoreTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 21, 0, 0, TimeSpan.Zero);
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        "trayclaw-store-" + Guid.NewGuid().ToString("N"));

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

    private string WriteFile(int pid, string state, string folder = "trayclaw")
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

        var store = new SessionStore(_directory, pid => pid == 100);
        var sessions = store.ReadAll(Now);

        var only = Assert.Single(sessions);
        Assert.Equal(100, only.Pid);
        Assert.Equal("live", only.Folder);
        Assert.Equal(AgentState.Working, only.State);

        Assert.False(File.Exists(Path.Combine(_directory, deadFile)), "the ended session's file is removed");
        Assert.True(File.Exists(livePath), "a live session's file is left alone");
        Assert.Equal(before, File.ReadAllText(livePath));
    }

    [Fact]
    public void A_file_that_cannot_be_parsed_is_ignored()
    {
        var name = SessionRef.TryFileName(300, "session-300", out var fileName)
            ? fileName
            : throw new InvalidOperationException();
        File.WriteAllText(Path.Combine(_directory, name), "{ this is not json");
        File.WriteAllText(Path.Combine(_directory, "not-a-session.json"), "{}");

        var store = new SessionStore(_directory, _ => true);

        Assert.Empty(store.ReadAll(Now));
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

        var store = new SessionStore(_directory, _ => true);

        Assert.Empty(store.ReadAll(Now));
    }

    [Fact]
    public void A_missing_directory_reads_as_nothing()
    {
        var missing = Path.Combine(_directory, "absent");
        var store = new SessionStore(missing, _ => true);

        Assert.Empty(store.ReadAll(Now));
    }
}
