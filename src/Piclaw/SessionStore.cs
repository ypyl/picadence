// Reads the state directory and decides what is still alive. Liveness is passed
// in, so a test can end a session without starting a process.
namespace Piclaw;

public sealed class SessionStore(string directory, Func<int, bool> isAlive)
{
    /// <summary>
    /// A state file is a few hundred bytes. Anything larger is not one, and
    /// reading it would be the cheapest way to hurt the tray.
    /// </summary>
    private const long MaxFileBytes = 64 * 1024;

    public string Directory { get; } = directory;

    /// <summary>
    /// Every session whose process is still running, and no other. State left by
    /// an ended process is removed on the way past, which is the only write the
    /// tray ever performs.
    /// </summary>
    public IReadOnlyList<AgentSession> ReadAll(DateTimeOffset now)
    {
        if (!System.IO.Directory.Exists(Directory))
        {
            return [];
        }

        var sessions = new List<AgentSession>();
        foreach (var path in System.IO.Directory.EnumerateFiles(Directory, "*.json"))
        {
            if (!SessionRef.TryParse(Path.GetFileName(path), out var reference))
            {
                continue;
            }

            if (!isAlive(reference.Pid))
            {
                Remove(path);
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

            if (StateFile.TryRead(json, reference, File.GetLastWriteTimeUtc(path), out var session))
            {
                sessions.Add(session);
            }
        }

        return sessions;
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
