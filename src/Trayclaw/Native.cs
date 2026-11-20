// The Win32 surface, kept in one file so the behaviour in Program.cs and
// SessionRef.cs reads like prose. Every declaration here is one the tray needs
// and nothing more: no shell execution, no network, no process launch.
using System.Runtime.InteropServices;

namespace Trayclaw;

internal static class Native
{
    internal const int WM_DESTROY = 0x0002;
    internal const int WM_CLOSE = 0x0010;
    internal const int WM_TIMER = 0x0113;
    internal const int WM_RBUTTONUP = 0x0205;
    internal const int WM_APP = 0x8000;

    internal const int NIM_ADD = 0;
    internal const int NIM_MODIFY = 1;
    internal const int NIM_DELETE = 2;

    internal const int NIF_MESSAGE = 0x01;
    internal const int NIF_ICON = 0x02;
    internal const int NIF_TIP = 0x04;

    internal const int MF_STRING = 0x0000;
    internal const int MF_GRAYED = 0x00000001;
    internal const int MF_SEPARATOR = 0x00000800;
    internal const int TPM_RIGHTBUTTON = 0x0002;
    internal const int TPM_RETURNCMD = 0x0100;

    internal const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

    internal const int WS_POPUP = unchecked((int)0x80000000);

    internal const int CommandQuit = 1;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct NotifyIconData
    {
        internal int cbSize;
        internal nint hWnd;
        internal int uID;
        internal int uFlags;
        internal int uCallbackMessage;
        internal nint hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] internal string szTip;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Msg
    {
        internal nint hwnd;
        internal int message;
        internal nint wParam;
        internal nint lParam;
        internal int time;
        internal int x;
        internal int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Point
    {
        internal int x;
        internal int y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct WindowClassEx
    {
        internal int cbSize;
        internal int style;
        internal nint lpfnWndProc;
        internal int cbClsExtra;
        internal int cbWndExtra;
        internal nint hInstance;
        internal nint hIcon;
        internal nint hCursor;
        internal nint hbrBackground;
        internal string? lpszMenuName;
        internal string lpszClassName;
        internal nint hIconSm;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    internal static extern nint GetModuleHandleW(string? moduleName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern ushort RegisterClassExW(ref WindowClassEx windowClass);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern nint CreateWindowExW(
        int exStyle, string className, string windowName, int style,
        int x, int y, int width, int height,
        nint parent, nint menu, nint instance, nint param);

    [DllImport("user32.dll")]
    internal static extern bool DestroyWindow(nint hwnd);

    [DllImport("user32.dll")]
    internal static extern int GetMessageW(out Msg msg, nint hwnd, int min, int max);

    [DllImport("user32.dll")]
    internal static extern bool TranslateMessage(ref Msg msg);

    [DllImport("user32.dll")]
    internal static extern nint DispatchMessageW(ref Msg msg);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern nint DefWindowProcW(nint hwnd, int msg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    internal static extern void PostQuitMessage(int exitCode);

    [DllImport("user32.dll")]
    internal static extern nint CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern bool AppendMenuW(nint menu, int flags, nint id, string text);

    [DllImport("user32.dll")]
    internal static extern bool DestroyMenu(nint menu);

    [DllImport("user32.dll")]
    internal static extern bool SetForegroundWindow(nint hwnd);

    [DllImport("user32.dll")]
    internal static extern bool GetCursorPos(out Point point);

    [DllImport("user32.dll")]
    internal static extern int TrackPopupMenu(
        nint menu, int flags, int x, int y, int reserved, nint hwnd, nint rect);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    internal static extern bool Shell_NotifyIconW(int message, ref NotifyIconData data);

    [DllImport("user32.dll")]
    internal static extern nint SetTimer(nint hwnd, nint id, uint milliseconds, nint timerProc);

    [DllImport("user32.dll")]
    internal static extern bool KillTimer(nint hwnd, nint id);

    [DllImport("user32.dll")]
    internal static extern nint CreateIcon(
        nint instance, int width, int height, byte planes, byte bitsPerPixel,
        byte[] andBits, byte[] xorBits);

    [DllImport("user32.dll")]
    internal static extern bool DestroyIcon(nint icon);

    [DllImport("kernel32.dll")]
    internal static extern nint OpenProcess(int access, bool inheritHandle, int processId);

    [DllImport("kernel32.dll")]
    internal static extern bool CloseHandle(nint handle);
}
