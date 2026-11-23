// What this machine has, of the two things the tray depends on. pi is what reports
// a session and Windows Terminal is what can start one, so a missing copy changes
// what the panel should say rather than whether the tray runs.
namespace Picadence;

public sealed record Dependencies(bool Pi, bool Terminal)
{
    /// <summary>Both, as this machine answers right now.</summary>
    public static Dependencies Detect() =>
        new(Paths.PiLauncher() is not null, Paths.WindowsTerminal() is not null);

    /// <summary>The state a machine in good order is in.</summary>
    public static Dependencies Present { get; } = new(Pi: true, Terminal: true);
}
