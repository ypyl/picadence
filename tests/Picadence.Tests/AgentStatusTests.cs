namespace Picadence.Tests;

public class AgentStatusTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 21, 0, 0, TimeSpan.Zero);

    private static AgentSession Session(
        int pid,
        AgentState state,
        int secondsAgo = 0,
        string folder = "picadence",
        string? detail = null) =>
        new(
            new SessionRef(pid, $"session-{pid}"),
            folder,
            Name: null,
            Model: null,
            state,
            detail,
            Now.AddSeconds(-secondsAgo));

    [Fact(DisplayName = "the most urgent state wins")]
    public void The_most_urgent_state_wins()
    {
        AgentSession[] sessions =
        [
            Session(1, AgentState.Working),
            Session(2, AgentState.Blocked),
            Session(3, AgentState.Error),
            Session(4, AgentState.Done),
        ];

        Assert.Equal(AgentState.Error, AgentStatus.Aggregate(sessions));
    }

    [Theory(DisplayName = "the most urgent state wins")]
    [InlineData(AgentState.Working, AgentState.Blocked, AgentState.Blocked)]
    [InlineData(AgentState.Working, AgentState.Done, AgentState.Working)]
    [InlineData(AgentState.Done, AgentState.Idle, AgentState.Done)]
    public void The_most_urgent_state_wins_for_a_pair(AgentState left, AgentState right, AgentState expected)
    {
        Assert.Equal(expected, AgentStatus.Aggregate([Session(1, left), Session(2, right)]));
    }

    [Fact(DisplayName = "no live session leaves the indicator quiet")]
    public void No_live_session_leaves_the_indicator_quiet()
    {
        Assert.Equal(AgentState.Idle, AgentStatus.Aggregate([]));
    }

    [Fact(DisplayName = "the panel summarizes the sessions that are running")]
    public void The_panel_summarizes_the_sessions_that_are_running()
    {
        AgentSession[] sessions =
        [
            Session(1, AgentState.Working),
            Session(2, AgentState.Working),
            Session(3, AgentState.Blocked),
        ];

        var summary = PanelModel.Build(sessions, Now, false).Summary;

        Assert.Contains("1 waiting", summary, StringComparison.Ordinal);
        Assert.Contains("2 working", summary, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "a state change is shown within two seconds")]
    public void A_state_change_is_shown_within_two_seconds()
    {
        Assert.InRange(TraySettings.PollMilliseconds, 1, 2000);
    }

    [Theory]
    [InlineData(0, "0s")]
    [InlineData(5, "5s")]
    [InlineData(59, "59s")]
    [InlineData(60, "1m")]
    [InlineData(3_600, "1h")]
    [InlineData(90_000, "1d")]
    public void Age_is_short_and_readable(int seconds, string expected)
    {
        Assert.Equal(expected, Age.Format(TimeSpan.FromSeconds(seconds)));
    }

    [Fact]
    public void Age_of_the_future_is_not_negative()
    {
        Assert.Equal("0s", Age.Format(TimeSpan.FromSeconds(-30)));
    }
}
