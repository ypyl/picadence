namespace Picadence.Tests;

public class StateFileTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 21, 0, 0, TimeSpan.Zero);
    private static readonly SessionRef Reference = new(1234, "abc-123");

    [Fact]
    public void A_full_body_is_read()
    {
        const string json = """
        {
          "pid": 1234,
          "sessionId": "abc-123",
          "folder": "picadence",
          "cwd": "C:\\Users\\ypyl\\projects\\picadence",
          "name": "bootstrap",
          "model": "opencode-go/deepseek",
          "state": "blocked",
          "detail": "Confirm",
          "updatedAt": "2026-11-20T20:59:00.000Z"
        }
        """;

        Assert.True(StateFile.TryRead(json, Reference, Now, out var session));
        Assert.Equal(AgentState.Blocked, session.State);
        Assert.Equal("picadence", session.Folder);
        Assert.Equal("bootstrap", session.Name);
        Assert.Equal("Confirm", session.Detail);
        Assert.Equal(2026, session.UpdatedAt.Year);
    }

    [Fact]
    public void Unknown_fields_are_ignored()
    {
        const string json = """{"state":"working","somethingNew":{"deep":[1,2,3]},"folder":"x"}""";

        Assert.True(StateFile.TryRead(json, Reference, Now, out var session));
        Assert.Equal(AgentState.Working, session.State);
    }

    [Fact]
    public void A_folder_is_derived_from_the_working_directory_when_missing()
    {
        const string json = """{"state":"idle","cwd":"C:\\Users\\ypyl\\projects\\picadence"}""";

        Assert.True(StateFile.TryRead(json, Reference, Now, out var session));
        Assert.Equal("picadence", session.Folder);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"state":"sleeping"}""")]
    [InlineData("""{"state":42}""")]
    [InlineData("not json at all")]
    [InlineData("")]
    [InlineData("[]")]
    public void A_body_without_a_usable_state_is_rejected(string json)
    {
        Assert.False(StateFile.TryRead(json, Reference, Now, out _));
    }

    [Fact]
    public void A_field_of_the_wrong_type_is_ignored_rather_than_fatal()
    {
        const string json = """{"state":"done","folder":17,"detail":["a"],"updatedAt":"yesterday-ish"}""";

        Assert.True(StateFile.TryRead(json, Reference, Now, out var session));
        Assert.Equal(AgentState.Done, session.State);
        Assert.Equal("unknown", session.Folder);
        Assert.Null(session.Detail);
        Assert.Equal(Now, session.UpdatedAt);
    }
}
