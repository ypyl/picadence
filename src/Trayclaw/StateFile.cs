// Reads the JSON the pi extension writes. Unknown fields are ignored, a field of
// the wrong type is ignored, and a body without a usable state is rejected, so
// one bad file cannot take the tray down or invent a row.
using System.Text.Json;

namespace Trayclaw;

public static class StateFile
{
    private const int MaxFolderLength = 40;

    public static bool TryRead(
        string json,
        SessionRef reference,
        DateTimeOffset fallbackUpdatedAt,
        out AgentSession session)
    {
        session = default!;
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

            if (!AgentStateExtensions.TryParse(Text(root, "state"), out var state))
            {
                return false;
            }

            var folder = Text(root, "folder") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = FolderOf(Text(root, "cwd"));
            }

            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = "unknown";
            }

            folder = Truncate(folder, MaxFolderLength) ?? "unknown";
            var updatedAt = Timestamp(root, "updatedAt") ?? fallbackUpdatedAt;

            session = new AgentSession(
                reference,
                folder,
                Truncate(Text(root, "name"), 60),
                Truncate(Text(root, "model"), 60),
                state,
                Truncate(Text(root, "detail"), 40),
                updatedAt);
            return true;
        }
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

    private static string? FolderOf(string? cwd)
    {
        if (string.IsNullOrWhiteSpace(cwd))
        {
            return null;
        }

        var trimmed = cwd.TrimEnd('\\', '/');
        var separator = trimmed.LastIndexOfAny(['\\', '/']);
        return separator >= 0 && separator < trimmed.Length - 1
            ? trimmed[(separator + 1)..]
            : trimmed;
    }

    private static string? Truncate(string? value, int length)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= length ? trimmed : trimmed[..length];
    }
}
