// Where a session's conversation lives. pi writes one file per session and folder,
// named <timestamp>_<session-id>.jsonl, and the folder slug is pi's rule to derive,
// not ours to copy: so the search runs over the folders that exist.
namespace Picadence;

public static class Transcripts
{
    /// <summary>
    /// The conversation file for a session, or null when there is none. Newest wins
    /// when two files claim the same id, because a resumed session can be written
    /// twice and the later file is the one that carries the turn.
    /// </summary>
    public static string? Find(string root, string sessionId)
    {
        if (!SessionRef.IsPlainStem(sessionId) || !Directory.Exists(root))
        {
            return null;
        }

        string? newest = null;
        var newestAt = DateTime.MinValue;

        try
        {
            foreach (var folder in Directory.EnumerateDirectories(root))
            {
                foreach (var path in Directory.EnumerateFiles(folder, "*" + sessionId + ".jsonl"))
                {
                    // The glob lets a longer id through; the name has to end with
                    // the separator and this id exactly.
                    if (!Path.GetFileName(path).EndsWith("_" + sessionId + ".jsonl", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var at = File.GetLastWriteTimeUtc(path);
                    if (newest is null || at > newestAt)
                    {
                        newest = path;
                        newestAt = at;
                    }
                }
            }
        }
        catch (IOException)
        {
            return newest;
        }
        catch (UnauthorizedAccessException)
        {
            return newest;
        }

        return newest;
    }

    /// <summary>
    /// True when a path is a file pi could have written: inside the sessions root
    /// after resolution, ending in <c>.jsonl</c>, and not a reparse point that
    /// points somewhere else.
    /// </summary>
    public static bool IsInside(string root, string? path)
    {
        if (path is not { Length: > 0 }
            || !path.EndsWith(".jsonl", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            var rootFull = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
            var pathFull = Path.GetFullPath(path);
            if (!pathFull.StartsWith(rootFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return !File.GetAttributes(pathFull).HasFlag(FileAttributes.ReparsePoint);
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
