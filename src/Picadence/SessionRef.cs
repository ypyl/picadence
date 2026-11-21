// The one piece of the state contract a machine can check today: the file name
// the extension writes and the tray reads. A session id arrives from pi, so it
// is treated as untrusted input: one that is not already a plain file stem is
// rejected rather than repaired, because repairing two different ids into one
// name would let them overwrite each other.
using System.Globalization;

namespace Picadence;

/// <summary>One agent session, identified by the pi process and its session id.</summary>
public readonly record struct SessionRef(int Pid, string SessionId)
{
    private const string Extension = ".json";

    /// <summary>
    /// The file name for a session: <c>&lt;pid&gt;-&lt;session&gt;.json</c>. Fails when the
    /// session id holds anything outside letters, digits, and dashes.
    /// </summary>
    public static bool TryFileName(int pid, string? sessionId, out string fileName)
    {
        fileName = string.Empty;
        if (pid <= 0 || !IsPlainStem(sessionId))
        {
            return false;
        }

        fileName = pid.ToString(CultureInfo.InvariantCulture) + "-" + sessionId + Extension;
        return true;
    }

    /// <summary>
    /// True when the value is non-empty and holds only letters, digits, and
    /// dashes, so it cannot become a path segment.
    /// </summary>
    public static bool IsPlainStem(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        foreach (var c in value)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c != '-')
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Reads a file name back. Returns false for anything <see cref="TryFileName"/>
    /// would not have produced, including a name that tries to walk out of the
    /// directory.
    /// </summary>
    public static bool TryParse(string? fileName, out SessionRef value)
    {
        value = default;
        if (string.IsNullOrEmpty(fileName) || !fileName.EndsWith(Extension, StringComparison.Ordinal))
        {
            return false;
        }

        var stem = fileName[..^Extension.Length];
        var dash = stem.IndexOf('-', StringComparison.Ordinal);
        if (dash <= 0 || dash == stem.Length - 1)
        {
            return false;
        }

        var digits = stem[..dash];
        if (!int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var pid) || pid <= 0)
        {
            return false;
        }

        var session = stem[(dash + 1)..];
        if (!IsPlainStem(session))
        {
            return false;
        }

        value = new SessionRef(pid, session);
        return true;
    }
}
