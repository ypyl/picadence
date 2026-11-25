// The four rows of the start decision. They are the whole of it, and Program reads
// the answer rather than deciding anything itself.
namespace Picadence.Tests;

public class StartupTests
{
    /// <summary>A shortcut, a login entry, a build: nothing is waiting for this process.</summary>
    [Fact(DisplayName = "a start from a shortcut runs the tray in place")]
    public void A_start_from_a_shortcut_runs_the_tray_in_place() =>
        Assert.Equal(StartupAction.RunHere, Startup.Decide(
            trayRunning: false,
            fromWaitingShell: false,
            detachedCopy: false));

    /// <summary>The tool's batch file: cmd waits for what it starts, so the tray leaves.</summary>
    [Fact]
    public void A_start_from_a_waiting_shell_detaches() =>
        Assert.Equal(StartupAction.Detach, Startup.Decide(
            trayRunning: false,
            fromWaitingShell: true,
            detachedCopy: false));

    /// <summary>One icon means one tray, so the second start is turned away where it was typed.</summary>
    [Fact(DisplayName = "a second start adds no second icon")]
    public void A_second_start_adds_no_second_icon() =>
        Assert.Equal(StartupAction.AlreadyRunning, Startup.Decide(
            trayRunning: true,
            fromWaitingShell: false,
            detachedCopy: false));

    /// <summary>A tray already up wins over the shell, so the start says so instead of starting a copy.</summary>
    [Fact]
    public void A_second_start_from_a_shell_says_so_rather_than_detaching() =>
        Assert.Equal(StartupAction.AlreadyRunning, Startup.Decide(
            trayRunning: true,
            fromWaitingShell: true,
            detachedCopy: false));

    /// <summary>The copy exists to escape the shell, so it must not carry the marker back out.</summary>
    [Fact]
    public void The_detached_copy_runs_where_it_lands() =>
        Assert.Equal(StartupAction.RunHere, Startup.Decide(
            trayRunning: false,
            fromWaitingShell: true,
            detachedCopy: true));

    [Fact]
    public void The_marker_is_read_from_the_arguments() =>
        Assert.True(Startup.IsDetachedCopy([Startup.DetachedMarker]));
}
