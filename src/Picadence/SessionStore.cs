// Reads the state directory and decides what is still alive. Liveness is passed
// in, so a test can end a session without starting a process.
//
// pi writes a state file only when something changes, and the tray looks once a
// second, so almost every file is the one it already parsed. Each parse is kept
// and reused until the file's length or write time moves.
namespace Picadence;

public sealed class SessionStore(string directory, Func<int, DateTime, DateTimeOffset?, bool> isAlive)
{
    /// <summary>
    /// A state file is a few hundred bytes. Anything larger is not one, and
    /// reading it would be the cheapest way to hurt the tray.
    /// </summary>
    private const long MaxFileBytes = 64 * 1024;

    /// <summary>What was last parsed from a file, and when it was last seen.</summary>
    private sealed class Parsed
    {
        public required long Length { get; init; }

        public required DateTime WriteTimeUtc { get; init; }

        public required AgentSession Session { get; init; }

        /// <summary>The tick this entry was last used on, so a stale one is dropped.</summary>
        public long Seen { get; set; }
    }

    private readonly Dictionary<string, Parsed> _parsed = new(StringComparer.Ordinal);
    private long _tick;

    public string Directory { get; } = directory;

    /// <summary>
    /// Every session whose process is still running, and no other. State left by
    /// an ended process is removed on the way past, which is the only write the
    /// tray performs here.
    /// </summary>
    /// <summary>
    /// True when a process that started at <paramref name="createdUtc"/> could be the
    /// one that wrote a state file at <paramref name="writtenUtc"/>. A process cannot
    /// write a file before it starts, so a later start means the pid was reused.
    /// </summary>
    public static bool StartedBefore(DateTime createdUtc, DateTime writtenUtc) =>
        createdUtc <= writtenUtc.AddSeconds(2);

    /// <summary>
    /// True when the kernel's start time is the one the file claims. Two clocks write
    /// these, so they agree to within a couple of seconds rather than exactly.
    /// </summary>
    public static bool StartedTogether(DateTime createdUtc, DateTime claimedUtc) =>
        (createdUtc - claimedUtc).Duration() <= TimeSpan.FromSeconds(2);

    public IReadOnlyList<AgentSession> ReadAll(DateTimeOffset now)
    {
        if (!System.IO.Directory.Exists(Directory))
        {
            return [];
        }

        var tick = ++_tick;
        var sessions = new List<AgentSession>();
        foreach (var path in System.IO.Directory.EnumerateFiles(Directory, "*.json"))
        {
            var name = Path.GetFileName(path);
            if (!SessionRef.TryParse(name, out var reference))
            {
                continue;
            }

            long length;
            DateTime writeTimeUtc;
            try
            {
                // One stat. Both values come from the same snapshot, which is what
                // makes the pair a usable identity for the file.
                var info = new FileInfo(path);
                length = info.Length;
                writeTimeUtc = info.LastWriteTimeUtc;
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }


            if (length > MaxFileBytes)
            {
                continue;
            }

            if (_parsed.TryGetValue(name, out var parsed)
                && parsed.Length == length
                && parsed.WriteTimeUtc == writeTimeUtc)
            {
                parsed.Seen = tick;

                // The question comes after the parse, because the answer needs what
                // the file says about the process that wrote it.
                if (!isAlive(parsed.Session.Reference.Pid, writeTimeUtc, parsed.Session.StartedAt))
                {
                    Remove(path);
                    _parsed.Remove(name);
                    continue;
                }

                sessions.Add(parsed.Session);
                continue;
            }

            string json;
            try
            {
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

            if (StateFile.TryRead(json, reference, writeTimeUtc, out var session))
            {
                if (!isAlive(session.Reference.Pid, writeTimeUtc, session.StartedAt))
                {
                    Remove(path);
                    _parsed.Remove(name);
                    continue;
                }

                _parsed[name] = new Parsed
                {
                    Length = length,
                    WriteTimeUtc = writeTimeUtc,
                    Session = session,
                    Seen = tick,
                };
                sessions.Add(session);
            }
            else
            {
                _parsed.Remove(name);
            }
        }

        DropUnseen(tick, sessions.Count);
        return sessions;
    }

    /// <summary>
    /// Forgets what is no longer on disk. Without this a tray left running for a
    /// week would keep one entry per session it has ever seen.
    /// </summary>
    private void DropUnseen(long tick, int live)
    {
        if (_parsed.Count == live)
        {
            return;
        }

        List<string>? gone = null;
        foreach (var (name, parsed) in _parsed)
        {
            if (parsed.Seen != tick)
            {
                (gone ??= []).Add(name);
            }
        }

        foreach (var name in gone ?? [])
        {
            _parsed.Remove(name);
        }
    }

    private static void Remove(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // A file that cannot be removed is ignored now and retried next tick.
        }
        catch (UnauthorizedAccessException)
        {
            // Same.
        }
    }
}
