// The Win32 surface, kept in one file so the behaviour in Program.cs and
// SessionRef.cs reads like prose. Every declaration here is one the tray needs
// and nothing more: no shell execution, no network, no process launch.
using System.Runtime.InteropServices;

namespace Picadence;

internal static class Native
{
    internal const int WM_DESTROY = 0x0002;
    internal const int WM_CLOSE = 0x0010;
    internal const int WM_TIMER = 0x0113;
    internal const int WM_RBUTTONUP = 0x0205;
    internal const int WM_APP = 0x8000;
    internal const int WM_ACTIVATE = 0x0006;
    internal const int WM_KEYDOWN = 0x0100;
    internal const int WM_PAINT = 0x000F;
    internal const int WM_ERASEBKGND = 0x0014;
    internal const int WM_LBUTTONUP = 0x0201;
    internal const int WM_MOUSEMOVE = 0x0200;

    internal const int WM_MOUSEWHEEL = 0x020A;
    internal const int WM_MOUSELEAVE = 0x02A3;
    internal const int WM_SETCURSOR = 0x0020;
    internal const int HTCLIENT = 1;
    internal const uint TME_LEAVE = 0x00000002;

    internal const int WA_INACTIVE = 0;
    internal const int VK_ESCAPE = 0x1B;
    internal const int SW_HIDE = 0;
    internal const int SW_SHOWNORMAL = 1;
    internal const int SW_SHOWNA = 8;

    internal const int WS_EX_TOOLWINDOW = 0x00000080;
    internal const int CS_DROPSHADOW = 0x00020000;
    internal const int DT_LEFT = 0x0000;
    internal const int DT_RIGHT = 0x0002;
    internal const int DT_CENTER = 0x0001;
    internal const int DT_VCENTER = 0x0004;
    internal const int DT_SINGLELINE = 0x0020;
    internal const int DT_END_ELLIPSIS = 0x8000;
    internal const int DT_NOPREFIX = 0x0800;
    internal const int DT_WORDBREAK = 0x0010;
    internal const int TRANSPARENT = 1;
    internal const int FW_NORMAL = 400;
    internal const int FW_SEMIBOLD = 600;
    internal const int DefaultDpi = 96;
    internal const int IDC_ARROW = 32512;
    internal const int IDC_HAND = 32649;
    internal const int NULL_PEN = 8;
    internal const int SRCCOPY = 0x00CC0020;
    internal const int MONITOR_DEFAULTTONEAREST = 2;
    internal const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    internal const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    internal const int DWMWCP_ROUND = 2;
    internal const int HWND_TOPMOST = -1;
    internal const uint SWP_SHOWWINDOW = 0x0040;
    internal const uint SWP_NOACTIVATE = 0x0010;

    internal const int NIM_ADD = 0;
    internal const int NIM_MODIFY = 1;
    internal const int NIM_DELETE = 2;

    internal const int NIF_MESSAGE = 0x01;
    internal const int NIF_ICON = 0x02;
    internal const int NIF_TIP = 0x04;


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
    internal struct TrackMouseEventData
    {
        internal int cbSize;
        internal uint flags;
        internal nint hwndTrack;
        internal uint hoverTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        internal int left;
        internal int top;
        internal int right;
        internal int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PaintStruct
    {
        internal nint hdc;
        internal bool erase;
        internal Rect paint;
        internal bool restore;
        internal bool incUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] internal byte[] reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MonitorInfo
    {
        internal int cbSize;
        internal Rect monitor;
        internal Rect work;
        internal int flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NotifyIconIdentifier
    {
        internal int cbSize;
        internal nint hWnd;
        internal int uID;
        internal Guid guidItem;
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
    internal static extern bool SetForegroundWindow(nint hwnd);

    [DllImport("user32.dll")]
    internal static extern nint LoadCursorW(nint instance, nint name);

    [DllImport("user32.dll")]
    internal static extern nint SetCursor(nint cursor);

    [DllImport("user32.dll")]
    internal static extern bool TrackMouseEvent(ref TrackMouseEventData tracking);

    [DllImport("user32.dll")]
    internal static extern bool ScreenToClient(nint hwnd, ref Point point);

    [DllImport("user32.dll")]
    internal static extern bool GetCursorPos(out Point point);

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

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
    internal static extern int RegOpenKeyExW(nint key, string subKey, int options, int rights, out nint result);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
    internal static extern int RegQueryValueExW(
        nint key, string valueName, nint reserved, out int type, byte[]? data, ref int size);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
    internal static extern int RegSetValueExW(
        nint key, string valueName, nint reserved, int type, byte[] data, int size);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
    internal static extern int RegDeleteValueW(nint key, string valueName);

    [DllImport("advapi32.dll")]
    internal static extern int RegCloseKey(nint key);

    [DllImport("kernel32.dll")]
    internal static extern nint OpenProcess(int access, bool inheritHandle, int processId);

    [DllImport("kernel32.dll")]
    internal static extern bool CloseHandle(nint handle);

    [DllImport("user32.dll")]
    internal static extern bool PostMessage(nint hwnd, int message, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    internal static extern bool GetClientRect(nint hwnd, out Rect rect);


    [DllImport("user32.dll")]
    internal static extern nint BeginPaint(nint hwnd, out PaintStruct paint);

    [DllImport("user32.dll")]
    internal static extern bool EndPaint(nint hwnd, ref PaintStruct paint);

    [DllImport("user32.dll")]
    internal static extern bool InvalidateRect(nint hwnd, nint rect, bool erase);

    [DllImport("user32.dll")]
    internal static extern bool ShowWindow(nint hwnd, int command);

    [DllImport("user32.dll")]
    internal static extern bool SetWindowPos(
        nint hwnd, nint after, int x, int y, int width, int height, uint flags);

    [DllImport("user32.dll")]
    internal static extern uint GetDpiForWindow(nint hwnd);

    [DllImport("user32.dll")]
    internal static extern bool SetProcessDpiAwarenessContext(nint context);

    [DllImport("user32.dll")]
    internal static extern nint MonitorFromRect(ref Rect rect, int flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern bool GetMonitorInfoW(nint monitor, ref MonitorInfo info);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern int DrawTextW(nint hdc, string text, int length, ref Rect rect, int flags);

    [DllImport("user32.dll")]
    internal static extern int FillRect(nint hdc, ref Rect rect, nint brush);

    [DllImport("user32.dll")]
    internal static extern int FrameRect(nint hdc, ref Rect rect, nint brush);

    [DllImport("gdi32.dll")]
    internal static extern nint CreateCompatibleDC(nint hdc);

    [DllImport("gdi32.dll")]
    internal static extern nint CreateCompatibleBitmap(nint hdc, int width, int height);

    [DllImport("gdi32.dll")]
    internal static extern nint SelectObject(nint hdc, nint objectHandle);

    [DllImport("gdi32.dll")]
    internal static extern bool DeleteObject(nint objectHandle);

    [DllImport("gdi32.dll")]
    internal static extern bool DeleteDC(nint hdc);

    [StructLayout(LayoutKind.Sequential)]
    internal struct SizeExtent
    {
        public int cx;
        public int cy;
    }

    /// <summary>How wide a string is in the font selected into the context.</summary>
    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    internal static extern bool GetTextExtentPoint32W(
        nint hdc, string text, int length, out SizeExtent size);

    [DllImport("gdi32.dll")]
    internal static extern nint CreateSolidBrush(uint colour);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    internal static extern nint CreateFontW(
        int height, int width, int escapement, int orientation, int weight,
        uint italic, uint underline, uint strikeOut, uint charSet, uint outputPrecision,
        uint clipPrecision, uint quality, uint pitchAndFamily, string face);

    [DllImport("gdi32.dll")]
    internal static extern int SetBkMode(nint hdc, int mode);

    [DllImport("gdi32.dll")]
    internal static extern uint SetTextColor(nint hdc, uint colour);

    [DllImport("gdi32.dll")]
    internal static extern bool Ellipse(nint hdc, int left, int top, int right, int bottom);

    [DllImport("gdi32.dll")]
    internal static extern bool RoundRect(nint hdc, int left, int top, int right, int bottom, int width, int height);

    [DllImport("gdi32.dll")]
    internal static extern nint GetStockObject(int index);

    [DllImport("gdi32.dll")]
    internal static extern bool BitBlt(
        nint destination, int x, int y, int width, int height,
        nint source, int sourceX, int sourceY, int rasterOp);

    [DllImport("dwmapi.dll")]
    internal static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int size);

    [DllImport("shell32.dll")]
    internal static extern int Shell_NotifyIconGetRect(ref NotifyIconIdentifier identifier, out Rect icon);

    [StructLayout(LayoutKind.Sequential)]
    internal struct FileTime
    {
        public uint Low;
        public uint High;

        /// <summary>The moment, in UTC. Times from the kernel are 100 ns ticks.</summary>
        public DateTime ToUtc() => DateTime.FromFileTimeUtc(((long)High << 32) | Low);
    }

    /// <summary>When the process behind a handle started, and when it exited.</summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool GetProcessTimes(
        nint process,
        out FileTime creation,
        out FileTime exit,
        out FileTime kernel,
        out FileTime user);

    /// <summary>Starts a process without a window of its own and without a console.</summary>
    internal const uint CREATE_NO_WINDOW = 0x08000000;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StartupInfo
    {
        public int cb;
        public nint lpReserved;
        public nint lpDesktop;
        public nint lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public nint lpReserved2;
        public nint hStdInput;
        public nint hStdOutput;
        public nint hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ProcessInformation
    {
        public nint hProcess;
        public nint hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    /// <summary>
    /// CreateProcess takes one command line and is allowed to write to it, so the
    /// buffer belongs to the caller. Passing a managed string here would hand over
    /// memory the runtime also believes it owns.
    /// </summary>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern unsafe bool CreateProcessW(
        nint applicationName,
        char* commandLine,
        nint processAttributes,
        nint threadAttributes,
        bool inheritHandles,
        uint creationFlags,
        nint environment,
        string? currentDirectory,
        ref StartupInfo startupInfo,
        out ProcessInformation processInformation);
}
