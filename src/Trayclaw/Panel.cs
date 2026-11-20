// The panel window: a borderless popup painted with GDI. Everything it draws
// comes from PanelModel in DIPs, scaled once by the monitor's DPI, so the code
// here is arithmetic and Win32 and nothing else.
using System.Runtime.InteropServices;

namespace Trayclaw;

internal static class Panel
{
    private const string ClassName = "TrayclawPanel";
    private const int PanelIconId = 1;

    private static nint _window;
    private static nint _owner;
    private static PanelLayout _layout = PanelModel.Build([], DateTimeOffset.UtcNow);
    /// <summary>The scale the window is sized with, from its monitor's DPI.</summary>
    private static float _dpiScale = 1f;

    /// <summary>The scale it is painted with, derived from the size it actually got.</summary>
    private static float _scale = 1f;
    private static readonly Dictionary<int, nint> Fonts = [];
    private static readonly Dictionary<uint, nint> Brushes = [];

    public static bool IsOpen { get; private set; }

    public static unsafe void Register(nint instance)
    {
        var windowClass = CreateWindowClass(
            instance, (nint)(delegate* unmanaged<nint, int, nint, nint, nint>)&WindowProc);
        Native.RegisterClassExW(ref windowClass);
    }

    /// <summary>
    /// The class the panel is created from. It names the arrow cursor, because a
    /// class that names none keeps whatever pointer Windows had last, which is how
    /// the panel ended up showing the busy indicator.
    /// </summary>
    internal static Native.WindowClassEx CreateWindowClass(nint instance, nint windowProc) => new()
    {
        cbSize = Marshal.SizeOf<Native.WindowClassEx>(),
        style = 0x0002 | 0x0001 | Native.CS_DROPSHADOW,
        lpfnWndProc = windowProc,
        hInstance = instance,
        hCursor = Native.LoadCursorW(0, Native.IDC_ARROW),
        lpszClassName = ClassName,
    };

    /// <summary>Opens the panel above the icon, or closes it when it is open.</summary>
    public static void Toggle(IReadOnlyList<AgentSession> sessions, DateTimeOffset now, nint owner)
    {
        if (PanelController.After(IsOpen, PanelInput.IconClicked))
        {
            Show(sessions, now, owner);
        }
        else
        {
            Hide();
        }
    }

    /// <summary>Repaints while open, so a state change shows without a reopen.</summary>
    public static void Update(IReadOnlyList<AgentSession> sessions, DateTimeOffset now)
    {
        if (!IsOpen)
        {
            return;
        }

        var next = PanelModel.Build(sessions, now);
        var grew = Math.Abs(next.Height - _layout.Height) > 0.5 || Math.Abs(next.Width - _layout.Width) > 0.5;
        _layout = next;

        if (grew)
        {
            Place();
        }

        Native.InvalidateRect(_window, 0, false);
    }

    public static void Hide()
    {
        if (_window != 0)
        {
            Native.ShowWindow(_window, Native.SW_HIDE);
        }

        IsOpen = false;
    }

    private static void Show(IReadOnlyList<AgentSession> sessions, DateTimeOffset now, nint owner)
    {
        _owner = owner;
        _layout = PanelModel.Build(sessions, now);

        if (_window == 0)
        {
            _window = Native.CreateWindowExW(
                Native.WS_EX_TOOLWINDOW, ClassName, "trayclaw status", Native.WS_POPUP,
                0, 0, 1, 1, 0, 0, Native.GetModuleHandleW(null), 0);
            if (_window == 0)
            {
                return;
            }

            // shortcut: one scale for the panel, taken from the monitor it is
            // created on, size per monitor when a machine spans two DPI scales
            _dpiScale = ScaleOf(Native.GetDpiForWindow(_window));

            var dark = 1;
            Native.DwmSetWindowAttribute(_window, Native.DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
            var corner = Native.DWMWCP_ROUND;
            Native.DwmSetWindowAttribute(_window, Native.DWMWA_WINDOW_CORNER_PREFERENCE, ref corner, sizeof(int));
        }

        if (!Place())
        {
            return;
        }

        Native.ShowWindow(_window, Native.SW_SHOWNORMAL);
        Native.SetForegroundWindow(_window);
        Native.InvalidateRect(_window, 0, false);
        IsOpen = true;
    }

    /// <summary>
    /// Moves and sizes the window for the current layout and monitor. The scale
    /// comes from the caller, because the window's own DPI is only trustworthy
    /// once it has been on that monitor for a message or two: Paint() asks again
    /// and calls back here when the answer changed.
    /// </summary>
    private static bool Place()
    {
        var icon = IconRect();
        if (icon is null)
        {
            return false;
        }

        var iconRect = icon.Value;
        var monitor = Native.MonitorFromRect(ref iconRect, Native.MONITOR_DEFAULTTONEAREST);
        var info = new Native.MonitorInfo { cbSize = Marshal.SizeOf<Native.MonitorInfo>() };
        if (monitor == 0 || !Native.GetMonitorInfoW(monitor, ref info))
        {
            return false;
        }

        var work = new DipRect(
            info.work.left, info.work.top,
            info.work.right - info.work.left, info.work.bottom - info.work.top);
        var iconBox = new DipRect(
            iconRect.left, iconRect.top,
            iconRect.right - iconRect.left, iconRect.bottom - iconRect.top);

        var width = (int)Math.Round(_layout.Width * _dpiScale);
        var height = (int)Math.Round(_layout.Height * _dpiScale);
        var gap = (int)Math.Round(PanelModel.Gap * _dpiScale);
        var at = PanelPlacement.Place(width, height, gap, iconBox, work);

        Native.SetWindowPos(
            _window, Native.HWND_TOPMOST, (int)at.X, (int)at.Y, width, height, Native.SWP_NOACTIVATE);
        return true;
    }

    private static float ScaleOf(uint dpi) => dpi == 0 ? 1f : dpi / (float)Native.DefaultDpi;

    private static Native.Rect? IconRect()
    {
        var identifier = new Native.NotifyIconIdentifier
        {
            cbSize = Marshal.SizeOf<Native.NotifyIconIdentifier>(),
            hWnd = _owner,
            uID = PanelIconId,
        };

        if (Native.Shell_NotifyIconGetRect(ref identifier, out var rect) == 0)
        {
            return rect;
        }

        return Native.GetCursorPos(out var point)
            ? new Native.Rect { left = point.x, top = point.y, right = point.x + 16, bottom = point.y + 16 }
            : null;
    }

    [UnmanagedCallersOnly]
    private static nint WindowProc(nint hwnd, int message, nint wParam, nint lParam)
    {
        switch (message)
        {
            case Native.WM_PAINT:
                Paint(hwnd);
                return 0;
            case Native.WM_ERASEBKGND:
                return 1;
            case Native.WM_ACTIVATE when (int)(wParam & 0xFFFF) == Native.WA_INACTIVE:
                Hide();
                return 0;
            case Native.WM_KEYDOWN when (int)wParam == Native.VK_ESCAPE:
                Hide();
                return 0;
            case Native.WM_LBUTTONUP:
                Click((short)(lParam & 0xFFFF), (short)((lParam >> 16) & 0xFFFF));
                return 0;
            default:
                return Native.DefWindowProcW(hwnd, message, wParam, lParam);
        }
    }

    private static void Click(int x, int y)
    {
        // A row and empty space both do nothing: the panel reports, it does not act.
        if (_layout.HitTest(x / _scale, y / _scale) == PanelHit.Quit)
        {
            Hide();
            Native.PostMessage(_owner, Native.WM_CLOSE, 0, 0);
        }
    }

    private static void Paint(nint hwnd)
    {
        Native.BeginPaint(hwnd, out var paint);
        try
        {
            Native.GetClientRect(hwnd, out var client);
            var width = client.right;
            var height = client.bottom;
            if (width <= 0 || height <= 0)
            {
                return;
            }

            // Paint at the scale the window actually got, not at a scale someone
            // hoped for. The window is sized from its DPI when it is created, so
            // the two agree; if they ever stop agreeing the content still fits.
            var scale = width / (float)_layout.Width;
            if (Math.Abs(scale - _scale) > 0.001f)
            {
                _scale = scale;
                ResetFonts();
            }

            var memory = Native.CreateCompatibleDC(paint.hdc);
            var bitmap = Native.CreateCompatibleBitmap(paint.hdc, width, height);
            var previous = Native.SelectObject(memory, bitmap);
            try
            {
                var whole = client;
                Native.FillRect(memory, ref whole, Brush(PanelTheme.Surface));
                Native.FrameRect(memory, ref whole, Brush(PanelTheme.Outline));
                Native.SetBkMode(memory, Native.TRANSPARENT);
                DrawContent(memory);
                Native.BitBlt(paint.hdc, 0, 0, width, height, memory, 0, 0, Native.SRCCOPY);
            }
            finally
            {
                Native.SelectObject(memory, previous);
                Native.DeleteObject(bitmap);
                Native.DeleteDC(memory);
            }
        }
        finally
        {
            Native.EndPaint(hwnd, ref paint);
        }
    }

    private static void DrawContent(nint dc)
    {
        var header = _layout.HeaderBounds;
        Text(dc, _layout.Title, header, 12, Native.FW_SEMIBOLD, PanelTheme.PrimaryText,
            Native.DT_LEFT | Native.DT_VCENTER | Native.DT_SINGLELINE);
        Text(dc, _layout.Summary, header, 12, Native.FW_NORMAL, PanelTheme.SecondaryText,
            Native.DT_RIGHT | Native.DT_VCENTER | Native.DT_SINGLELINE);

        foreach (var row in _layout.Rows)
        {
            var background = Scaled(row.Bounds);
            Native.FillRect(dc, ref background, Brush(PanelTheme.Row));

            var dot = new DipRect(row.Bounds.X + 8, row.Bounds.Y + 10, 8, 8);
            var shape = Scaled(dot);
            // Ellipse paints with the brush selected into the device context, not
            // with one passed in, so the state colour has to be selected first.
            var previousBrush = Native.SelectObject(dc, Brush(PanelTheme.State(row.State)));
            Native.Ellipse(dc, shape.left, shape.top, shape.right, shape.bottom);
            Native.SelectObject(dc, previousBrush);

            var textLeft = row.Bounds.X + 26;
            var textWidth = row.Bounds.Width - 26 - 64;

            Text(dc, row.Folder, new DipRect(textLeft, row.Bounds.Y + 5, textWidth, 20),
                14, Native.FW_NORMAL, PanelTheme.PrimaryText,
                Native.DT_LEFT | Native.DT_SINGLELINE | Native.DT_END_ELLIPSIS);

            var state = string.IsNullOrEmpty(row.Detail) ? row.StateWord : $"{row.StateWord} ({row.Detail})";
            Text(dc, state, new DipRect(textLeft, row.Bounds.Y + 25, textWidth, 16),
                12, Native.FW_NORMAL, PanelTheme.SecondaryText,
                Native.DT_LEFT | Native.DT_SINGLELINE | Native.DT_END_ELLIPSIS);

            Text(dc, row.Age, new DipRect(row.Bounds.X + row.Bounds.Width - 60, row.Bounds.Y + 15, 60, 16),
                12, Native.FW_NORMAL, PanelTheme.SecondaryText,
                Native.DT_RIGHT | Native.DT_SINGLELINE);
        }

        if (_layout.EmptyText is not null)
        {
            var empty = _layout.EmptyBounds;
            Text(dc, _layout.EmptyText, new DipRect(empty.X, empty.Y + 4, empty.Width, 20),
                14, Native.FW_NORMAL, PanelTheme.PrimaryText,
                Native.DT_LEFT | Native.DT_SINGLELINE);
            Text(dc, _layout.EmptyHint ?? string.Empty, new DipRect(empty.X, empty.Y + 24, empty.Width, 16),
                12, Native.FW_NORMAL, PanelTheme.SecondaryText,
                Native.DT_LEFT | Native.DT_SINGLELINE);
        }

        if (_layout.MoreText is not null)
        {
            Text(dc, _layout.MoreText, _layout.MoreBounds, 12, Native.FW_NORMAL,
                PanelTheme.SecondaryText, Native.DT_LEFT | Native.DT_SINGLELINE);
        }

        Text(dc, "Quit", _layout.QuitBounds, 12, Native.FW_NORMAL, PanelTheme.PrimaryText,
            Native.DT_RIGHT | Native.DT_VCENTER | Native.DT_SINGLELINE);

    }

    private static void Text(nint dc, string text, DipRect bounds, int size, int weight, Rgb colour, int flags)
    {
        var rect = Scaled(bounds);
        var font = Font(size, weight);
        Native.SetTextColor(dc, colour.ToColorRef());
        var previous = Native.SelectObject(dc, font);
        Native.DrawTextW(dc, text, text.Length, ref rect, flags | Native.DT_NOPREFIX);
        Native.SelectObject(dc, previous);
    }

    private static Native.Rect Scaled(DipRect bounds) => new()
    {
        left = (int)Math.Round(bounds.X * _scale),
        top = (int)Math.Round(bounds.Y * _scale),
        right = (int)Math.Round((bounds.X + bounds.Width) * _scale),
        bottom = (int)Math.Round((bounds.Y + bounds.Height) * _scale),
    };

    private static nint Font(int size, int weight)
    {
        var key = (size * 1000) + weight;
        if (Fonts.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var height = -(int)Math.Round(size * _scale);
        var font = Native.CreateFontW(
            height, 0, 0, 0, weight, 0, 0, 0, 1, 0, 0, 5, 0, "Segoe UI Variable Text");
        Fonts[key] = font;
        return font;
    }

    private static nint Brush(Rgb colour)
    {
        if (Brushes.TryGetValue(colour.ToColorRef(), out var cached))
        {
            return cached;
        }

        var brush = Native.CreateSolidBrush(colour.ToColorRef());
        Brushes[colour.ToColorRef()] = brush;
        return brush;
    }

    /// <summary>Font sizes depend on the scale, so a monitor change drops them.</summary>
    private static void ResetFonts()
    {
        foreach (var font in Fonts.Values)
        {
            Native.DeleteObject(font);
        }

        Fonts.Clear();
    }
}
