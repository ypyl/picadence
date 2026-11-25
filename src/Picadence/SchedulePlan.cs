// One tick of the clock: which armed sessions deserve a ping right now, which
// records move, and what the panel should show. Pure, so the arithmetic is proved
// by tests instead of by waiting an hour.
using System.Globalization;

namespace Picadence;

/// <summary>What a due window does: send the words, or start a tab and send them.</summary>
public enum Wake
{
    Send,
    StartAndSend,
}

/// <summary>A ping that is due: the record it was planned from, when it was
/// planned, the words, and how it is delivered.</summary>
public sealed record Ping(Schedule Schedule, string Text, DateTimeOffset At, Wake Wake, Launch? Launch);

/// <summary>What one tick decided. Program writes the pings, then the records.</summary>
public sealed record ScheduleTick(
    IReadOnlyList<Ping> Pings,
    IReadOnlyList<Schedule> Written,
    IReadOnlyDictionary<SessionRef, Schedule> View);

public static class SchedulePlan
{
    /// <summary>
    /// How long a start is remembered, so one window cannot open two tabs while the
    /// first is still writing its state.
    /// </summary>
    public static readonly TimeSpan StartGrace = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Plans one tick. A cadence with no live session waits, unless the user kept
    /// that session: a kept session can be started, so its window is delivered by
    /// opening one tab and letting the extension pick the ping up.
    /// </summary>
    public static ScheduleTick Plan(
        IReadOnlyList<AgentSession> sessions,
        IReadOnlyList<Schedule> schedules,
        DateTimeOffset now,
        IReadOnlyList<KeptSession>? kept = null,
        Func<string, DateTimeOffset?>? startedAt = null)
    {
        var pings = new List<Ping>();
        var written = new Dictionary<string, Schedule>(StringComparer.Ordinal);
        var view = new Dictionary<SessionRef, Schedule>();

        var live = new Dictionary<string, AgentSession>(StringComparer.Ordinal);
        foreach (var session in sessions)
        {
            live.TryAdd(session.Reference.SessionId, session);
        }

        var keptById = new Dictionary<string, KeptSession>(StringComparer.Ordinal);
        foreach (var entry in kept ?? [])
        {
            keptById[entry.Pin.SessionId] = entry;
        }

        foreach (var schedule in schedules)
        {
            if (!live.TryGetValue(schedule.SessionId, out var session))
            {
                // A kept session's cadence is what opens its tab, so its row has to
                // say when. No process holds the record, so it is keyed by session id
                // alone, which is the only part of the reference that exists.
                if (PlanCold(schedule, keptById, startedAt, now, pings, written) is { } cold)
                {
                    view[new SessionRef(0, schedule.SessionId)] = cold;
                }
                continue;
            }

            var current = schedule;
            if (!string.Equals(current.Name, session.Name, StringComparison.Ordinal)
                || !string.Equals(current.Folder, session.Folder, StringComparison.Ordinal))
            {
                current = current with { Name = session.Name, Folder = session.Folder };
                written[current.SessionId] = current;
            }

            if (current.IsDue(now) && IsFree(session.State))
            {
                var missed = current.Every.MissedBetween(current.NextAt, now, current.AnchorDay);
                pings.Add(new Ping(current, Text(current, missed, now), now, Wake.Send, null));
                current = current.AfterFiring(now);
                written[current.SessionId] = current;
            }

            view[session.Reference] = current;
        }

        return new ScheduleTick(pings, [.. written.Values], view);
    }

    /// <summary>
    /// A window for a session that is not running. Nothing happens unless the user
    /// kept it, because a pin is how they said they wanted it back. A start that
    /// cannot work is not attempted: the window counts as missed and is scheduled
    /// forward like any other, so a broken pin does not become a tray that tries
    /// every second. Returns the record the row should read, or null when there is
    /// no row, which is every session the user did not keep.
    /// </summary>
    private static Schedule? PlanCold(
        Schedule schedule,
        IReadOnlyDictionary<string, KeptSession> kept,
        Func<string, DateTimeOffset?>? startedAt,
        DateTimeOffset now,
        List<Ping> pings,
        Dictionary<string, Schedule> written)
    {
        if (!kept.TryGetValue(schedule.SessionId, out var entry))
        {
            // No row: the user never kept this session, so its cadence has nowhere
            // to be shown and nothing to start.
            return null;
        }

        if (!schedule.IsDue(now))
        {
            return schedule;
        }

        // A tab that was started seconds ago has not written its state yet. Waiting
        // is what keeps one window to one tab.
        if (startedAt?.Invoke(schedule.SessionId) is { } started && now - started < StartGrace)
        {
            return schedule;
        }

        var missed = schedule.Every.MissedBetween(schedule.NextAt, now, schedule.AnchorDay);
        if (entry.Plan is { CanRestart: true, Launch: { } launch })
        {
            pings.Add(new Ping(
                schedule,
                Text(schedule, missed, now),
                now,
                Wake.StartAndSend,
                launch));
        }

        var next = schedule.AfterFiring(now);
        written[schedule.SessionId] = next;
        return next;
    }

    /// <summary>
    /// A session that is working or waiting for the user is having a turn already,
    /// so its window is skipped rather than queued behind the run.
    /// </summary>
    public static bool IsFree(AgentState state) =>
        state is AgentState.Idle or AgentState.Done or AgentState.Error;

    /// <summary>
    /// The words pi reads. It names picadence, the time and the cadence, and it asks
    /// for the session's own standing instruction. It carries no instruction of its
    /// own: what to do is the session's business.
    /// </summary>
    public static string Text(Schedule schedule, int missed, DateTimeOffset at)
    {
        var when = at.ToString("HH:mm", CultureInfo.InvariantCulture);
        var head = missed <= 0
            ? string.Create(CultureInfo.InvariantCulture, $"picadence ping - {when}, {schedule.Every.Label()}")
            : string.Create(
                CultureInfo.InvariantCulture,
                $"picadence ping - {when}, {schedule.Every.Label()}, {MissedWords(missed)} missed");

        return string.Join(
            '\n',
            head,
            "Automated wake-up from the picadence tray app. Not a message from the user.",
            "Follow this session's standing instructions.");
    }

    private static string MissedWords(int missed) => missed >= CadenceExtensions.MaxMissed
        ? string.Create(CultureInfo.InvariantCulture, $"at least {missed} windows")
        : string.Create(CultureInfo.InvariantCulture, $"{missed} window{(missed == 1 ? string.Empty : "s")}");
}
