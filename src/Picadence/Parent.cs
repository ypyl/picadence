// The process that started this one. A command shell is the only launcher here
// that waits for what it started, so telling it apart from a shortcut or a build is
// what decides whether the tray has to hand itself to a copy.
namespace Picadence;

internal static class Parent
{
    /// <summary>
    /// Whether a command shell started this process and is waiting for it. The tool
    /// installs its command as a batch file, and cmd waits for what a batch file
    /// starts, so this is the one shape that holds a terminal.
    /// </summary>
    internal static bool IsWaitingShell() =>
        string.Equals(NameOf(Environment.ProcessId), "cmd.exe", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// The base name of a process's parent image, or null when it cannot be read. One
    /// snapshot answers both questions, which needs no handle on the parent and no
    /// access right it might already have lost.
    /// </summary>
    internal static string? NameOf(int pid)
    {
        var snapshot = Native.CreateToolhelp32Snapshot(Native.TH32CS_SNAPPROCESS, 0);
        if (snapshot is 0 or -1)
        {
            return null;
        }

        try
        {
            var entry = New();
            var parent = ParentOf(snapshot, ref entry, pid);
            return parent <= 0 ? null : ImageOf(snapshot, ref entry, parent);
        }
        finally
        {
            Native.CloseHandle(snapshot);
        }
    }

    private static int ParentOf(nint snapshot, ref Native.ProcessEntry entry, int pid)
    {
        unsafe
        {
            fixed (Native.ProcessEntry* row = &entry)
            {
                if (!Native.Process32FirstW(snapshot, row))
                {
                    return 0;
                }

                do
                {
                    if (entry.th32ProcessID == pid)
                    {
                        return entry.th32ParentProcessID;
                    }
                }
                while (Native.Process32NextW(snapshot, row));
            }
        }

        return 0;
    }

    private static string? ImageOf(nint snapshot, ref Native.ProcessEntry entry, int pid)
    {
        unsafe
        {
            fixed (Native.ProcessEntry* row = &entry)
            {
                // Asking for the first entry again starts the walk over, so the parent's
                // own row is one more pass across the same snapshot.
                if (!Native.Process32FirstW(snapshot, row))
                {
                    return null;
                }

                do
                {
                    if (entry.th32ProcessID == pid)
                    {
                        return Native.ImageName(row);
                    }
                }
                while (Native.Process32NextW(snapshot, row));
            }
        }

        return null;
    }

    private static Native.ProcessEntry New() =>
        new() { dwSize = Size() };

    private static unsafe int Size() => sizeof(Native.ProcessEntry);
}
