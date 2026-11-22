// Reads and writes one pin file. A field of the wrong shape, or a session id that
// does not match the file name, is a rejection: a pin decides what may be started,
// so a file that cannot be read exactly is not repaired and not used.
using System.Text;
using System.Text.Json;

namespace Picadence;

public static class PinFile
{
    private const int MaxTextLength = 400;

    public static bool TryRead(string json, string sessionId, out Pin pin)
    {
        pin = default!;
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            return false;
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!string.Equals(Text(root, "sessionId"), sessionId, StringComparison.Ordinal))
            {
                return false;
            }

            var folder = Text(root, "folder");
            if (!IsUsableFolder(folder))
            {
                return false;
            }

            var transcript = Text(root, "transcript");
            if (transcript is not null && !IsUsableTranscript(transcript))
            {
                return false;
            }

            if (!DateTimeOffset.TryParse(
                    Text(root, "keptAt"),
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var keptAt))
            {
                return false;
            }

            pin = new Pin(sessionId, Text(root, "name"), folder!, transcript, keptAt);
            return true;
        }
    }

    public static string ToJson(Pin pin)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("sessionId", pin.SessionId);
            if (pin.Name is { Length: > 0 } name)
            {
                writer.WriteString("name", name);
            }

            writer.WriteString("folder", pin.Folder);
            if (pin.Transcript is { Length: > 0 } transcript)
            {
                writer.WriteString("transcript", transcript);
            }

            writer.WriteString("keptAt", pin.KeptAt.ToUniversalTime().ToString("O"));
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray()) + "\n";
    }

    /// <summary>
    /// A folder is kept when it is a path with no control characters, so it cannot
    /// carry a line to a terminal. Whether it is absolute, and whether it still
    /// exists, are questions for the restart, which says so on the row.
    /// </summary>
    public static bool IsUsableFolder(string? folder) =>
        folder is { Length: > 0 and <= MaxTextLength } && !HasControl(folder);

    /// <summary>A conversation is an absolute path to a file pi wrote, and nothing
    /// else. Whether it still exists is a question for the restart, not for the read.</summary>
    public static bool IsUsableTranscript(string? transcript) =>
        transcript is { Length: > 0 and <= MaxTextLength }
        && Path.IsPathRooted(transcript)
        && transcript.EndsWith(".jsonl", StringComparison.OrdinalIgnoreCase)
        && !HasControl(transcript);

    private static bool HasControl(string value)
    {
        foreach (var c in value)
        {
            if (char.IsControl(c))
            {
                return true;
            }
        }

        return false;
    }

    private static string? Text(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
