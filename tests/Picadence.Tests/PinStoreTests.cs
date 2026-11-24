namespace Picadence.Tests;

public class PinStoreTests : IDisposable
{
    private static readonly DateTimeOffset Kept = new(2026, 11, 22, 19, 31, 4, TimeSpan.Zero);

    private readonly string _directory = Path.Combine(
        Path.GetTempPath(), "picadence-pins-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try
        {
            Directory.Delete(_directory, recursive: true);
        }
        catch (IOException)
        {
            // The assertions already ran; a leaked temp folder is not a failure.
        }
    }

    private Pin Sample(string sessionId = "01a1fe3b-e1c8-71eb-afd8-f796496f0f78", string? transcript = null) =>
        new(
            sessionId,
            "trayclaw",
            Path.Combine(Path.GetTempPath(), "trayclaw"),
            transcript ?? Path.Combine(Path.GetTempPath(), "sessions", $"2026-11-22T09-14-02-118Z_{sessionId}.jsonl"),
            Kept);

    [Fact(DisplayName = "a kept session outlives the tray")]
    public void A_kept_session_outlives_the_tray()
    {
        var pin = Sample();
        new PinStore(_directory).Write(pin);

        // A second store stands in for the tray after a restart: nothing is held in
        // memory between the two, so what comes back came off the disk.
        var reread = Assert.Single(new PinStore(_directory).ReadAll());

        Assert.Equal(pin.SessionId, reread.SessionId);
        Assert.Equal(pin.Folder, reread.Folder);
        Assert.Equal(pin.Transcript, reread.Transcript);
        Assert.Equal(pin.Name, reread.Name);
        Assert.Equal(pin.KeptAt.ToUniversalTime(), reread.KeptAt.ToUniversalTime());
    }

    [Fact(DisplayName = "stopping the keeping removes the row")]
    public void Stopping_the_keeping_removes_the_row()
    {
        var store = new PinStore(_directory);
        store.Write(Sample());
        Assert.Single(store.ReadAll());

        store.Remove(Sample().SessionId);

        Assert.Empty(new PinStore(_directory).ReadAll());
    }

    [Theory(DisplayName = "a pin that cannot be read exactly is not used")]
    [InlineData("not json")]
    [InlineData("{\"sessionId\":\"someone-else\",\"folder\":\"C:\\\\x\",\"keptAt\":\"2026-11-22T19:31:04Z\"}")]
    [InlineData("{\"sessionId\":\"pinned\",\"folder\":\"C:\\\\x\",\"transcript\":\"C:\\\\x\\\\a.txt\",\"keptAt\":\"2026-11-22T19:31:04Z\"}")]
    [InlineData("{\"sessionId\":\"pinned\",\"folder\":\"C:\\\\x\"}")]
    public void A_pin_that_cannot_be_read_exactly_is_not_used(string json)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, "pinned.json"), json);

        Assert.Empty(new PinStore(_directory).ReadAll());
    }

    [Fact(DisplayName = "a kept session is read back with the folder pi reported")]
    public void A_kept_session_is_read_back_with_the_folder_pi_reported()
    {
        // The row shows the folder's name, and the pin has to keep the place. This
        // is the chain a click on the pin walks: the state file, the pin, the read.
        const string json = @"{""state"":""done"",""folder"":""trayclaw"",""cwd"":""C:\\Users\\me\\projects\\trayclaw""}";
        var reference = new SessionRef(1234, Sample().SessionId);

        Assert.True(StateFile.TryRead(json, reference, Kept, out var session));

        var pin = new Pin(session.Reference.SessionId, session.Name, session.Cwd!, null, Kept);
        new PinStore(_directory).Write(pin);

        var reread = Assert.Single(new PinStore(_directory).ReadAll());
        Assert.Equal(@"C:\Users\me\projects\trayclaw", reread.Folder);
        Assert.True(Path.IsPathRooted(reread.Folder), "a folder name is not a place: [" + reread.Folder + "]");
    }

    [Fact(DisplayName = "a pin with a folder that is not a place is read, and refused later")]
    public void A_pin_with_a_folder_that_is_not_a_place_is_read_and_refused_later()
    {
        // An older extension reported only the folder's name. The pin still shows,
        // and the restart says what is wrong instead of the click doing nothing.
        Directory.CreateDirectory(_directory);
        var pin = new Pin("pinned", "x", "relative", null, Kept);
        File.WriteAllText(Path.Combine(_directory, "pinned.json"), PinFile.ToJson(pin));

        Assert.Single(new PinStore(_directory).ReadAll());

        var plan = Restart.Plan(pin, running: false, terminal: "wt.exe", pi: "pi.cmd", _ => true, _ => true, _directory);
        Assert.Equal(RestartRefusal.NoFolder, plan.Refusal);
        Assert.Equal("the folder is gone", plan.Note);
    }

    [Fact(DisplayName = "a pin whose file name is not a session id is skipped")]
    public void A_pin_whose_file_name_is_not_a_session_id_is_skipped()
    {
        Directory.CreateDirectory(_directory);
        var pin = Sample("pinned");
        File.WriteAllText(Path.Combine(_directory, "..-pinned.json"), PinFile.ToJson(pin));

        Assert.Empty(new PinStore(_directory).ReadAll());
    }
}

public class PinFollowTests
{
    private static readonly DateTimeOffset Kept = new(2026, 11, 24, 0, 20, 0, TimeSpan.Zero);

    private static Pin Pin(string? name = null, string folder = @"C:\Users\me\docs-site") =>
        new("01a2053f-ad54-71e5-ac0b-5e0e9c7a00a6", name, folder, null, Kept);

    private static AgentSession Session(string? name, string? cwd = @"C:\Users\me\docs-site") =>
        new(new SessionRef(1, "01a2053f-ad54-71e5-ac0b-5e0e9c7a00a6"), "docs-site", name, null,
            AgentState.Done, null, Kept)
        { Cwd = cwd };

    [Fact(DisplayName = "a pin learns the name its session took after it was kept")]
    public void A_pin_learns_the_name_its_session_took_after_it_was_kept()
    {
        // Pinned before the session had a name, renamed while it ran, tab closed: the
        // kept row has to show what the session became, not what it was.
        var followed = Pin().Follow(Session("Adopting Aspire research"));

        Assert.NotNull(followed);
        Assert.Equal("Adopting Aspire research", followed.Name);
    }

    [Fact(DisplayName = "a pin that already agrees is not rewritten")]
    public void A_pin_that_already_agrees_is_not_rewritten()
    {
        Assert.Null(Pin("Adopting Aspire research").Follow(Session("Adopting Aspire research")));
        Assert.Null(Pin(null).Follow(Session(null)));

        // Case and a trailing slash are not a change.
        Assert.Null(Pin("x", @"C:\Users\me\docs-site").Follow(Session("x", @"C:\Users\me\docs-site\")));
    }

    [Fact(DisplayName = "a session without a folder of its own keeps the pin's")]
    public void A_session_without_a_folder_of_its_own_keeps_the_pins()
    {
        var followed = Pin().Follow(Session("named", cwd: null));

        Assert.NotNull(followed);
        Assert.Equal(@"C:\Users\me\docs-site", followed.Folder);
        Assert.Equal("named", followed.Name);
    }
}
