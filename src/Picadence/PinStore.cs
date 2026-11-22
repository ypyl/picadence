// The sessions the user kept. One file per session id, named like the cadences,
// and the tray is the only writer. Nothing here is ever removed except on request,
// which is the difference between a kept session and a remembered one.
namespace Picadence;

public sealed class PinStore(string directory)
{
    /// <summary>A pin file is a line or two. Anything larger is not one.</summary>
    private const long MaxFileBytes = 64 * 1024;

    public string Directory { get; } = directory;

    public IReadOnlyList<Pin> ReadAll()
    {
        if (!System.IO.Directory.Exists(Directory))
        {
            return [];
        }

        var pins = new List<Pin>();
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

            if (PinFile.TryRead(json, sessionId, out var pin))
            {
                pins.Add(pin);
            }
        }

        return pins;
    }

    /// <summary>Writes in one step, so a reader never sees half a file.</summary>
    public void Write(Pin pin)
    {
        if (!SessionRef.IsPlainStem(pin.SessionId))
        {
            throw new ArgumentException("session id is not a plain stem", nameof(pin));
        }

        System.IO.Directory.CreateDirectory(Directory);
        var target = PathFor(pin.SessionId);
        var temporary = $"{target}.{Environment.ProcessId}.tmp";
        File.WriteAllText(temporary, PinFile.ToJson(pin), new System.Text.UTF8Encoding(false));
        File.Move(temporary, target, true);
    }

    /// <summary>Stops keeping a session, which is the only way a pin is ever removed.</summary>
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
