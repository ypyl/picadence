// The tray process: a hidden window that owns the icon, a timer that reads the
// state directory, and a panel that shows what it found.
//
// Every decision worth checking lives in AgentStatus, PanelModel,
// PanelPlacement, PanelController, SessionStore, StateFile, and PanelTheme. This
// file only turns their answers into Win32 calls.
using System.Runtime.InteropServices;

namespace Picadence;

internal static class Program
{
    private const int TrayCallbackMessage = Native.WM_APP + 1;
    private const int PollTimerId = 1;
    private const int ProcessPerMonitorV2 = -4;

    private static nint _window;
    private static Native.NotifyIconData _icon;
    private static readonly Dictionary<AgentState, nint> Icons = [];
    private static readonly SessionStore Store = new(Paths.Agents, IsProcessAlive);
    private static readonly ScheduleStore Schedules = new(Paths.Schedules);

    private static readonly PinStore Pins = new(Paths.Pins);

    private static IReadOnlyList<AgentSession> _sessions = [];
    private static IReadOnlyDictionary<SessionRef, Schedule> _armed = new Dictionary<SessionRef, Schedule>();
    private static IReadOnlyList<KeptSession> _kept = [];

    /// <summary>When each session was last started from the tray, so a window opens one tab.</summary>
    private static readonly Dictionary<string, DateTimeOffset> StartedAt = new(StringComparer.Ordinal);
    private static AgentState _shownState = AgentState.Idle;

    [UnmanagedCallersOnly]
    private static nint WindowProc(nint hwnd, int message, nint wParam, nint lParam)
    {
        switch (message)
        {
            case TrayCallbackMessage when (int)(lParam & 0xFFFF) is Native.WM_LBUTTONUP or Native.WM_RBUTTONUP:
                Panel.Toggle(_sessions, _armed, DateTimeOffset.Now, _window, _kept);
                return 0;
            case TrayCallbackMessage:
                return 0;
            case Native.WM_TIMER when (int)wParam == PollTimerId:
                Refresh();
                return 0;
            case Native.WM_CLOSE:
                Native.DestroyWindow(hwnd);
                return 0;
            case Native.WM_DESTROY:
                Native.KillTimer(hwnd, PollTimerId);
                Native.Shell_NotifyIconW(Native.NIM_DELETE, ref _icon);
                foreach (var icon in Icons.Values)
                {
                    Native.DestroyIcon(icon);
                }

                Native.PostQuitMessage(0);
                return 0;
            default:
                return Native.DefWindowProcW(hwnd, message, wParam, lParam);
        }
    }

    /// <summary>One pass: drop what ended, wake what is armed, aggregate what is left, show it.</summary>
    private static void Refresh()
    {
        var now = DateTimeOffset.Now;

        // The state directory is read first, so a session whose process has ended
        // is already gone by the time a ping could be written for it.
        _sessions = Store.ReadAll(now);

        // The kept list is read before the tick, so a restart that just happened
        // is already visible as a live session on the same pass.
        var running = new HashSet<string>(
            _sessions.Select(session => session.Reference.SessionId), StringComparer.Ordinal);
        _kept =
        [
            .. Pins.ReadAll()
                .Select(pin => new KeptSession(pin, Restart.View(pin, running.Contains(pin.SessionId)))),
        ];

        var tick = SchedulePlan.Plan(
            _sessions,
            Schedules.ReadAll(),
            now,
            _kept,
            sessionId => StartedAt.TryGetValue(sessionId, out var started) ? started : null);

        HashSet<string> undelivered = new(StringComparer.Ordinal);
        foreach (var ping in tick.Pings)
        {
            // The words go to disk before a tab is started, so the session that comes
            // up reads them on its own first poll. A ping that could not be written is
            // not delivered, and its window is retried rather than lost.
            if (!PingFile.Write(Paths.Pings, ping))
            {
                undelivered.Add(ping.Schedule.SessionId);
                continue;
            }

            if (ping is { Wake: Wake.StartAndSend, Launch: { } launch })
            {
                StartedAt[ping.Schedule.SessionId] = now;
                Start(launch);
            }
        }

        foreach (var schedule in tick.Written)
        {
            // A record whose ping never reached the disk keeps its next time in the
            // past, so the window is retried instead of lost.
            if (!undelivered.Contains(schedule.SessionId))
            {
                Schedules.Write(schedule);
            }
        }

        _armed = tick.View;

        var state = AgentStatus.Aggregate(_sessions);
        if (state != _shownState)
        {
            _shownState = state;
            _icon.hIcon = Icons[state];
            _icon.uFlags = Native.NIF_MESSAGE | Native.NIF_ICON;
            Native.Shell_NotifyIconW(Native.NIM_MODIFY, ref _icon);
        }

        Panel.Update(_sessions, _armed, now, _kept);
    }

    /// <summary>A process id is alive when it can still be opened for a query.</summary>
    private static bool IsProcessAlive(int pid)
    {
        if (pid <= 0)
        {
            return false;
        }

        var handle = Native.OpenProcess(Native.PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
        if (handle == 0)
        {
            return false;
        }

        Native.CloseHandle(handle);
        return true;
    }

    /// <summary>
    /// Starts the one program the tray may start: a Windows Terminal tab that runs
    /// pi on a kept session's conversation. It does not wait, and it holds nothing
    /// of the process afterwards, because the tab outlives this call.
    /// </summary>
    internal static bool Start(Launch launch)
    {
        if (launch.CommandLine.Length == 0 || !File.Exists(launch.File))
        {
            return false;
        }

        var commandLine = launch.CommandLine;
        var buffer = new char[commandLine.Length + 1];
        commandLine.CopyTo(0, buffer, 0, commandLine.Length);

        var startup = new Native.StartupInfo { cb = Marshal.SizeOf<Native.StartupInfo>() };
        bool started;
        unsafe
        {
            fixed (char* line = buffer)
            {
                started = Native.CreateProcessW(
                    0,
                    line,
                    0,
                    0,
                    false,
                    Native.CREATE_NO_WINDOW,
                    0,
                    launch.WorkingDirectory,
                    ref startup,
                    out var process);
                if (started)
                {
                    Native.CloseHandle(process.hThread);
                    Native.CloseHandle(process.hProcess);
                }
            }
        }

        return started;
    }

    private static nint CreateIconFor(AgentState state) =>
        Native.CreateIcon(
            0, IconPixels.Size, IconPixels.Size, 1, 32,
            IconPixels.AndMask(), IconPixels.Bgra(state));

    private static unsafe int Main()
    {
        // Without this every size in the panel is a guess on a scaled monitor.
        Native.SetProcessDpiAwarenessContext(ProcessPerMonitorV2);

        foreach (var state in Enum.GetValues<AgentState>())
        {
            Icons[state] = CreateIconFor(state);
        }

        var instance = Native.GetModuleHandleW(null);
        var windowClass = new Native.WindowClassEx
        {
            cbSize = Marshal.SizeOf<Native.WindowClassEx>(),
            lpfnWndProc = (nint)(delegate* unmanaged<nint, int, nint, nint, nint>)&WindowProc,
            hInstance = instance,
            lpszClassName = "PicadenceWindow",
        };

        if (Native.RegisterClassExW(ref windowClass) == 0)
        {
            return 1;
        }

        Panel.Register(instance);
        Panel.Launcher = Start;

        _window = Native.CreateWindowExW(
            0, windowClass.lpszClassName, "picadence", Native.WS_POPUP,
            0, 0, 0, 0, 0, 0, instance, 0);
        if (_window == 0)
        {
            return 1;
        }

        _icon = new Native.NotifyIconData
        {
            cbSize = Marshal.SizeOf<Native.NotifyIconData>(),
            hWnd = _window,
            uID = 1,
            // No NIF_TIP: the panel header carries the counts, and a hover text
            // that appears unbidden only covers what is behind the icon.
            uFlags = Native.NIF_MESSAGE | Native.NIF_ICON,
            uCallbackMessage = TrayCallbackMessage,
            hIcon = Icons[AgentState.Idle],
            szTip = string.Empty,
        };

        if (!Native.Shell_NotifyIconW(Native.NIM_ADD, ref _icon))
        {
            return 1;
        }

        // The first pass runs before the loop, so a session already on disk shows
        // up without waiting for a timer tick.
        Refresh();
        Native.SetTimer(_window, PollTimerId, TraySettings.PollMilliseconds, 0);

        while (true)
        {
            var result = Native.GetMessageW(out var message, 0, 0, 0);
            if (result == 0)
            {
                return 0;
            }

            if (result < 0)
            {
                // The message loop itself failed; reporting success would hide it.
                return 1;
            }

            Native.TranslateMessage(ref message);
            Native.DispatchMessageW(ref message);
        }
    }
}
