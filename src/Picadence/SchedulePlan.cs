// One tick of the clock: which armed sessions deserve a ping right now, which
// records move, and what the panel should show. Pure, so the arithmetic is proved
// by tests instead of by waiting an hour.
using System.Globalization;

namespace Picadence;

/// <summary>A ping that is due: the record it was planned from, when it was
/// planned, and the words.</summary>
public sealed record Ping(Schedule Schedule, string Text, DateTimeOffset At);

/// <summary>What one tick decided. Program writes the pings, then the records.</summary>
public sealed record ScheduleTick(
    IReadOnlyList<Ping> Pings,
    IReadOnlyList<Schedule> Written,
    IReadOnlyDictionary<SessionRef, Schedule> View);

public static class SchedulePlan
{
    /// <summary>
    /// Plans one tick. A cadence with no live session is left exactly as it is, so
    /// its next time stays in the past and the window is reported as missed when
    /// the session comes back.
    /// </summary>
    public static ScheduleTick Plan(
        IReadOnlyList<AgentSession> sessions,
        IReadOnlyList<Schedule> schedules,
        DateTimeOffset now)
    {
        var pings = new List<Ping>();
        var written = new Dictionary<string, Schedule>(StringComparer.Ordinal);
        var view = new Dictionary<SessionRef, Schedule>();

        var live = new Dictionary<string, AgentSession>(StringComparer.Ordinal);
        foreach (var session in sessions)
        {
            live.TryAdd(session.Reference.SessionId, session);
        }

        foreach (var schedule in schedules)
        {
            if (!live.TryGetValue(schedule.SessionId, out var session))
            {
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
                var missed = current.Every.MissedBetween(current.NextAt, now);
                pings.Add(new Ping(current, Text(current, missed, now), now));
                current = current.AfterFiring(now);
                written[current.SessionId] = current;
            }

            view[session.Reference] = current;
        }

        return new ScheduleTick(pings, [.. written.Values], view);
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
