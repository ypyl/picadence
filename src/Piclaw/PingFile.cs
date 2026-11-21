// The door into a session. The tray writes one of these, the extension already
// running in that session reads it, sends the words, and deletes the file. It is
// the only thing this process ever puts anywhere pi looks.
using System.Text;
using System.Text.Json;

namespace Piclaw;

public static class PingFile
{
    /// <summary>
    /// A ping is written in one step, so the reader never sees half of it. False
    /// means the write failed, and the caller must not move the record: the window
    /// is then tried again on the next tick.
    /// </summary>
    public static bool Write(string directory, Ping ping)
    {
        try
        {
            Directory.CreateDirectory(directory);
            var target = Path.Combine(directory, ping.Schedule.SessionId + ".json");
            var temporary = $"{target}.{Environment.ProcessId}.tmp";
            File.WriteAllText(temporary, ToJson(ping), new UTF8Encoding(false));
            File.Move(temporary, target, true);
            return true;
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

    public static string ToJson(Ping ping)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("at", ping.At.ToUniversalTime().ToString("O"));
            writer.WriteString("every", ping.Schedule.Every.Key());
            writer.WriteNumber("missed", ping.Schedule.Every.MissedBetween(ping.Schedule.NextAt, ping.At));
            writer.WriteString("prompt", ping.Text);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray()) + "\n";
    }
}
