// One cadence, as it is kept on disk. It belongs to a session id and to nothing
// else: a cadence survives a reboot, a tray restart and a resumed session, and it
// fires only into the session that was armed.
namespace Picadence;

/// <summary>
/// One armed session. <paramref name="Name"/> and <paramref name="Folder"/> are a
/// label for whoever opens the file; nothing decides anything from them.
/// </summary>
public sealed record Schedule(
    string SessionId,
    string? Name,
    string? Folder,
    Cadence Every,
    DateTimeOffset NextAt,
    DateTimeOffset? LastIssuedAt,
    int Missed)
{
    /// <summary>
    /// The record after a ping is issued at <paramref name="now"/>. The next time
    /// is measured from now and never from the missed one, which is what makes a
    /// burst impossible after a long gap.
    /// </summary>
    public Schedule AfterFiring(DateTimeOffset now) => this with
    {
        LastIssuedAt = now,
        Missed = Every.MissedBetween(NextAt, now),
        NextAt = Every.Advance(now),
    };

    /// <summary>True when this cadence is due at <paramref name="now"/>.</summary>
    public bool IsDue(DateTimeOffset now) => Every != Cadence.Off && NextAt <= now;
}
