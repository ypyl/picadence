// The tray process: one hidden window, one icon, one menu, one message loop.
// It shows no agent state yet. The next change adds the poll and fills the menu;
// this file owns only the plumbing so that change is about behaviour.
using System.Runtime.InteropServices;

namespace Trayclaw;

internal static class Program
{
    private const int TrayCallbackMessage = Native.WM_APP + 1;

    private static nint _window;
    private static Native.NotifyIconData _icon;

    [UnmanagedCallersOnly]
    private static nint WindowProc(nint hwnd, int message, nint wParam, nint lParam)
    {
        switch (message)
        {
            case TrayCallbackMessage:
                HandleTrayInput((int)(lParam & 0xFFFF));
                return 0;
            case Native.WM_CLOSE:
                Native.DestroyWindow(hwnd);
                return 0;
            case Native.WM_DESTROY:
                Native.Shell_NotifyIconW(Native.NIM_DELETE, ref _icon);
                Native.PostQuitMessage(0);
                return 0;
            default:
                return Native.DefWindowProcW(hwnd, message, wParam, lParam);
        }
    }

    private static void HandleTrayInput(int mouseMessage)
    {
        if (mouseMessage == Native.WM_RBUTTONUP)
        {
            ShowMenu();
        }
    }

    private static void ShowMenu()
    {
        var menu = Native.CreatePopupMenu();
        if (menu == 0)
        {
            return;
        }

        try
        {
            Native.AppendMenuW(menu, Native.MF_STRING, Native.CommandQuit, "Quit");
            Native.GetCursorPos(out var cursor);
            // Without this the menu stays on screen when the user clicks away.
            Native.SetForegroundWindow(_window);
            var chosen = Native.TrackPopupMenu(
                menu, Native.TPM_RIGHTBUTTON | Native.TPM_RETURNCMD,
                cursor.x, cursor.y, 0, _window, 0);

            if (chosen == Native.CommandQuit)
            {
                Native.DestroyWindow(_window);
            }
        }
        finally
        {
            Native.DestroyMenu(menu);
        }
    }

    private static unsafe int Main()
    {
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

        _window = Native.CreateWindowExW(
            0, windowClass.lpszClassName, "trayclaw", Native.WS_POPUP,
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
            uFlags = Native.NIF_MESSAGE | Native.NIF_ICON | Native.NIF_TIP,
            uCallbackMessage = TrayCallbackMessage,
            hIcon = Native.LoadIconW(0, (nint)Native.IDI_APPLICATION),
            szTip = "trayclaw: no agent state yet",
        };

        if (!Native.Shell_NotifyIconW(Native.NIM_ADD, ref _icon))
        {
            return 1;
        }

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
