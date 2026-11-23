// Whether the pi half is there. pi keeps what it has installed in its own settings
// file, which is the only place an answer can come from: a package that is declared
// can still have failed to install, and a project-local one never reaches the user
// file at all. So the answer is read, never written, and it never overrules the
// evidence of a session that has actually reported.
using System.Text.Json;

namespace Picadence;

/// <summary>What can be said about the pi half, in the order it can be believed.</summary>
public enum PiHalf
{
    /// <summary>A session has reported, so the extension is running.</summary>
    Working,

    /// <summary>pi's settings list picadence, but nothing has reported yet.</summary>
    Declared,

    /// <summary>pi's settings do not mention picadence.</summary>
    Missing,
}

public static class PiExtension
{
    /// <summary>The command the panel names when the half is missing.</summary>
    public const string Command = "pi install git:github.com/ypyl/picadence";

    private const string Mention = "picadence";

    /// <summary>
    /// The answer for this moment: a report outranks everything, then the settings
    /// files, which are the user's own and one per folder the tray knows about.
    /// </summary>
    public static PiHalf Check(bool anythingReported, IEnumerable<string> settingsFiles)
    {
        if (anythingReported)
        {
            return PiHalf.Working;
        }

        foreach (var file in settingsFiles)
        {
            if (Declares(file))
            {
                return PiHalf.Declared;
            }
        }

        return PiHalf.Missing;
    }

    /// <summary>The files pi would read: the user's own, then one per project folder.</summary>
    public static IEnumerable<string> SettingsFiles(IEnumerable<string> folders)
    {
        yield return Path.Combine(Paths.PiAgent, "settings.json");

        foreach (var folder in folders)
        {
            if (!string.IsNullOrWhiteSpace(folder) && Path.IsPathRooted(folder))
            {
                yield return Path.Combine(folder, ".pi", "settings.json");
            }
        }
    }

    /// <summary>
    /// True when a settings file lists a package that names picadence. A file that is
    /// missing, unreadable, or not JSON says nothing rather than guessing.
    /// </summary>
    public static bool Declares(string settingsFile)
    {
        try
        {
            if (!File.Exists(settingsFile))
            {
                return false;
            }

            var json = File.ReadAllText(settingsFile);
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty("packages", out var packages)
                || packages.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (var entry in packages.EnumerateArray())
            {
                if (entry.ValueKind == JsonValueKind.String
                    && entry.GetString() is { } value
                    && value.Contains(Mention, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }
}
