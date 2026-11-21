// Starting with Windows, as four rules and four registry calls.
//
// The registration names the exe that is running, never a path written at
// install time, so a folder move or a rebuild is corrected the next time the app
// starts instead of leaving a dead entry behind.
using System.Text;

namespace Picadence;

public static class Autostart
{
    /// <summary>The per-user Run key. No elevation: it is the current user's hive.</summary>
    public const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public const string ValueName = "picadence";

    private const int KeyQueryValue = 0x0001;
    private const int KeySetValue = 0x0002;
    private static readonly nint CurrentUser = unchecked((nint)0x80000001);
    private const int ErrorFileNotFound = 2;

    /// <summary>
    /// The command Windows runs. Quoted, because a folder can have a space in it
    /// and an unquoted path fails silently at sign-in.
    /// </summary>
    public static string CommandFor(string exePath)
    {
        if (string.IsNullOrWhiteSpace(exePath))
        {
            throw new ArgumentException("An autostart command needs a path.", nameof(exePath));
        }

        return '"' + exePath.Trim() + '"';
    }

    /// <summary>Whether the registered value already names this executable.</summary>
    public static bool IsSetTo(string? registered, string exePath) =>
        !string.IsNullOrEmpty(registered)
        && string.Equals(registered.Trim(), CommandFor(exePath), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// On, but pointing at another path or at nothing this app wrote. This is the
    /// check that runs at startup.
    /// </summary>
    public static bool NeedsRepair(string? registered, string exePath) =>
        !string.IsNullOrEmpty(registered) && !IsSetTo(registered, exePath);

    public static bool Toggle(bool current) => !current;

    /// <summary>The registered command, or null when there is none.</summary>
    public static string? Read()
    {
        var status = Native.RegOpenKeyExW(CurrentUser, RunKey, 0, KeyQueryValue, out var key);
        if (status != 0)
        {
            return null;
        }

        try
        {
            var size = 0;
            if (Native.RegQueryValueExW(key, ValueName, 0, out var type, null, ref size) != 0
                || size <= 0
                || type != 1)
            {
                return null;
            }

            var buffer = new byte[size];
            if (Native.RegQueryValueExW(key, ValueName, 0, out type, buffer, ref size) != 0)
            {
                return null;
            }

            return Encoding.Unicode.GetString(buffer, 0, size).TrimEnd('\0');
        }
        finally
        {
            Native.RegCloseKey(key);
        }
    }

    /// <summary>Points the registration at this exe, replacing whatever was there.</summary>
    public static bool Write(string exePath)
    {
        var status = Native.RegOpenKeyExW(CurrentUser, RunKey, 0, KeySetValue, out var key);
        if (status != 0)
        {
            return false;
        }

        try
        {
            var command = CommandFor(exePath);
            var bytes = Encoding.Unicode.GetBytes(command + "\0");
            return Native.RegSetValueExW(key, ValueName, 0, 1, bytes, bytes.Length) == 0;
        }
        finally
        {
            Native.RegCloseKey(key);
        }
    }

    /// <summary>Removes the registration. False when it could not be removed.</summary>
    public static bool Remove()
    {
        var status = Native.RegOpenKeyExW(CurrentUser, RunKey, 0, KeySetValue, out var key);
        if (status != 0)
        {
            return false;
        }

        try
        {
            var removed = Native.RegDeleteValueW(key, ValueName);
            return removed == 0 || removed == ErrorFileNotFound;
        }
        finally
        {
            Native.RegCloseKey(key);
        }
    }

    /// <summary>Corrects a stale registration at startup. On and current: nothing to do.</summary>
    public static void Follow(string exePath)
    {
        if (NeedsRepair(Read(), exePath))
        {
            Write(exePath);
        }
    }
}
