// What a process decides when it starts. A command shell that runs the .NET tool
// waits for the batch file the CLI installs, so a start from one hands the tray to
// a copy with no console and leaves. Everything here is a decision over three
// facts; Program makes the Win32 calls that carry it out.
namespace Picadence;

/// <summary>The three things a starting process can do.</summary>
internal enum StartupAction
{
    /// <summary>This process becomes the tray.</summary>
    RunHere,

    /// <summary>Start a copy with no console, then leave.</summary>
    Detach,

    /// <summary>A tray is already running: say so and leave.</summary>
    AlreadyRunning,
}

internal static class Startup
{
    /// <summary>The argument a detached copy carries, so it never starts another.</summary>
    internal const string DetachedMarker = "--tray";

    /// <summary>The name one tray holds. Local is one session, which is one person.</summary>
    internal const string MutexName = @"Local\PicadenceTray";

    /// <summary>What a second start says, in the terminal that asked for it.</summary>
    internal const string AlreadyRunningNotice = "picadence is already running";

    /// <summary>Whether this process is the copy a start already handed the tray to.</summary>
    internal static bool IsDetachedCopy(IReadOnlyList<string> args)
    {
        foreach (var arg in args)
        {
            if (string.Equals(arg, DetachedMarker, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// What to do, from what is true now. A tray already up wins over everything,
    /// so a second start says so where it was typed instead of starting a copy that
    /// would only find the same tray and quit.
    /// </summary>
    /// <param name="trayRunning">A process already holds the name one tray holds.</param>
    /// <param name="fromWaitingShell">A command shell started this process and waits for it.</param>
    /// <param name="detachedCopy">This process carries the marker of a detached copy.</param>
    internal static StartupAction Decide(bool trayRunning, bool fromWaitingShell, bool detachedCopy)
    {
        if (trayRunning)
        {
            return StartupAction.AlreadyRunning;
        }

        return fromWaitingShell && !detachedCopy ? StartupAction.Detach : StartupAction.RunHere;
    }
}
