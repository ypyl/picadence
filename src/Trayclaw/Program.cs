// The tray process: a hidden window that owns the icon, a timer that reads the
// state directory, and a panel that shows what it found.
//
// Every decision worth checking lives in AgentStatus, PanelModel,
// PanelPlacement, PanelController, SessionStore, StateFile, and PanelTheme. This
// file only turns their answers into Win32 calls.
using System.Runtime.InteropServices;

namespace Trayclaw;

internal static class Program
{
    private const int TrayCallbackMessage = Native.WM_APP + 1;
    private const int PollTimerId = 1;
    private const int ProcessPerMonitorV2 = -4;

    private static nint _window;
    private static Native.NotifyIconData _icon;
    private static readonly Dictionary<AgentState, nint> Icons = [];
    private static readonly string StateDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "trayclaw",
        "agents");
    private static readonly SessionStore Store = new(StateDirectory, IsProcessAlive);

    private static IReadOnlyList<AgentSession> _sessions = [];
    private static AgentState _shownState = AgentState.Idle;
    private static string _shownTooltip = string.Empty;

    [UnmanagedCallersOnly]
    private static nint WindowProc(nint hwnd, int message, nint wParam, nint lParam)
    {
        switch (message)
        {
            case TrayCallbackMessage when (int)(lParam & 0xFFFF) is Native.WM_LBUTTONUP or Native.WM_RBUTTONUP:
                Panel.Toggle(_sessions, DateTimeOffset.UtcNow, _window);
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

    /// <summary>One pass: drop what ended, aggregate what is left, show it.</summary>
    private static void Refresh()
    {
        _sessions = Store.ReadAll(DateTimeOffset.UtcNow);

        var state = AgentStatus.Aggregate(_sessions);
        var tooltip = AgentStatus.Tooltip(_sessions);
        if (state != _shownState || tooltip != _shownTooltip)
        {
            _shownState = state;
            _shownTooltip = tooltip;
            _icon.hIcon = Icons[state];
            _icon.szTip = tooltip;
            _icon.uFlags = Native.NIF_MESSAGE | Native.NIF_ICON | Native.NIF_TIP;
            Native.Shell_NotifyIconW(Native.NIM_MODIFY, ref _icon);
        }

        Panel.Update(_sessions, DateTimeOffset.UtcNow);
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
            lpszClassName = "TrayclawWindow",
        };

        if (Native.RegisterClassExW(ref windowClass) == 0)
        {
            return 1;
        }

        Panel.Register(instance);

        _window = Native.CreateWindowExW(
            0, windowClass.lpszClassName, "trayclaw", Native.WS_POPUP,
            0, 0, 0, 0, 0, 0, instance, 0);
        if (_window == 0)
        {
            return 1;
        }

        _shownTooltip = "trayclaw - no agent sessions";
        _icon = new Native.NotifyIconData
        {
            cbSize = Marshal.SizeOf<Native.NotifyIconData>(),
            hWnd = _window,
            uID = 1,
            uFlags = Native.NIF_MESSAGE | Native.NIF_ICON | Native.NIF_TIP,
            uCallbackMessage = TrayCallbackMessage,
            hIcon = Icons[AgentState.Idle],
            szTip = _shownTooltip,
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
