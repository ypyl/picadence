// What this machine has, of the three things picadence needs. pi is what reports a
// session, Windows Terminal is what can start one, and the pi half is the extension
// that does the reporting; a missing one changes what the panel says rather than
// whether the tray runs.
namespace Picadence;

public sealed record Dependencies(bool Pi, bool Terminal, PiHalf Half)
{
    /// <summary>What this machine has, with the pi half assumed working.</summary>
    public static Dependencies Detect() =>
        new(Paths.PiLauncher() is not null, Paths.WindowsTerminal() is not null, PiHalf.Working);

    /// <summary>The state a machine in good order is in.</summary>
    public static Dependencies Present { get; } = new(Pi: true, Terminal: true, PiHalf.Working);
}
