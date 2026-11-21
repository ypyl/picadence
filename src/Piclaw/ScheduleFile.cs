// Reads and writes one schedule file. A field of the wrong type, an unknown
// cadence, or a session id that does not match the file name is a rejection, so
// one bad file cannot invent a schedule.
using System.Text;
using System.Text.Json;

namespace Piclaw;

public static class ScheduleFile
{
    private const int MaxTextLength = 60;

    public static bool TryRead(string json, string sessionId, out Schedule schedule)
    {
        schedule = default!;
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

            var stored = Text(root, "sessionId");
            if (!string.Equals(stored, sessionId, StringComparison.Ordinal))
            {
                return false;
            }

            if (!CadenceExtensions.TryParse(Text(root, "every"), out var every) || every == Cadence.Off)
            {
                return false;
            }

            if (Timestamp(root, "nextAt") is not { } nextAt)
            {
                return false;
            }

            schedule = new Schedule(
                sessionId,
                Truncate(Text(root, "name")),
                Truncate(Text(root, "folder")),
                every,
                nextAt,
                Timestamp(root, "lastIssuedAt"),
                Integer(root, "missed"));
            return true;
        }
    }

    public static string ToJson(Schedule schedule)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("sessionId", schedule.SessionId);
            if (schedule.Name is not null)
            {
                writer.WriteString("name", schedule.Name);
            }

            if (schedule.Folder is not null)
            {
                writer.WriteString("folder", schedule.Folder);
            }

            writer.WriteString("every", schedule.Every.Key());
            writer.WriteString("nextAt", schedule.NextAt.ToUniversalTime().ToString("O"));
            if (schedule.LastIssuedAt is { } lastIssuedAt)
            {
                writer.WriteString("lastIssuedAt", lastIssuedAt.ToUniversalTime().ToString("O"));
            }

            writer.WriteNumber("missed", schedule.Missed);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray()) + "\n";
    }

    private static string? Text(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static DateTimeOffset? Timestamp(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value)
        && value.ValueKind == JsonValueKind.String
        && value.TryGetDateTimeOffset(out var parsed)
            ? parsed
            : null;

    private static int Integer(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value)
        && value.ValueKind == JsonValueKind.Number
        && value.TryGetInt32(out var parsed)
        && parsed >= 0
            ? parsed
            : 0;

    private static string? Truncate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= MaxTextLength ? trimmed : trimmed[..MaxTextLength];
    }
}
