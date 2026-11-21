// The cadences the tray keeps. One file per session, named by session id, and
// the tray is the only writer. Nothing here is ever removed except on request:
// a cadence that fires once a year looks abandoned for 364 days.
namespace Piclaw;

public sealed class ScheduleStore(string directory)
{
    /// <summary>A schedule file is a line or two. Anything larger is not one.</summary>
    private const long MaxFileBytes = 64 * 1024;

    public string Directory { get; } = directory;

    public IReadOnlyList<Schedule> ReadAll()
    {
        if (!System.IO.Directory.Exists(Directory))
        {
            return [];
        }

        var schedules = new List<Schedule>();
        foreach (var path in System.IO.Directory.EnumerateFiles(Directory, "*.json"))
        {
            var name = Path.GetFileName(path);
            var sessionId = name[..^".json".Length];
            if (!SessionRef.IsPlainStem(sessionId))
            {
                continue;
            }

            string json;
            try
            {
                if (new FileInfo(path).Length > MaxFileBytes)
                {
                    continue;
                }

                json = File.ReadAllText(path);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            if (ScheduleFile.TryRead(json, sessionId, out var schedule))
            {
                schedules.Add(schedule);
            }
        }

        return schedules;
    }

    /// <summary>Finds one cadence by session id, or nothing when it is not armed.</summary>
    public bool TryGet(string sessionId, out Schedule schedule)
    {
        schedule = default!;
        if (!SessionRef.IsPlainStem(sessionId))
        {
            return false;
        }

        try
        {
            var path = PathFor(sessionId);
            if (!File.Exists(path) || new FileInfo(path).Length > MaxFileBytes)
            {
                return false;
            }

            return ScheduleFile.TryRead(File.ReadAllText(path), sessionId, out schedule);
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>Writes in one step, so a reader never sees half a file.</summary>
    public void Write(Schedule schedule)
    {
        if (!SessionRef.IsPlainStem(schedule.SessionId))
        {
            throw new ArgumentException("session id is not a plain stem", nameof(schedule));
        }

        System.IO.Directory.CreateDirectory(Directory);
        var target = PathFor(schedule.SessionId);
        var temporary = $"{target}.{Environment.ProcessId}.tmp";
        File.WriteAllText(temporary, ScheduleFile.ToJson(schedule), new System.Text.UTF8Encoding(false));
        File.Move(temporary, target, true);
    }

    /// <summary>Turns a cadence off, which is the only way one is ever removed.</summary>
    public void Remove(string sessionId)
    {
        if (!SessionRef.IsPlainStem(sessionId))
        {
            return;
        }

        try
        {
            File.Delete(PathFor(sessionId));
        }
        catch (IOException)
        {
            // A file that cannot be removed is retried the next time it is asked for.
        }
        catch (UnauthorizedAccessException)
        {
            // Same.
        }
    }

    private string PathFor(string sessionId) => Path.Combine(Directory, sessionId + ".json");
}
