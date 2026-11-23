// The panel window: a borderless popup painted with GDI. Everything it draws
// comes from PanelModel in DIPs, scaled once by the monitor's DPI, so the code
// here is arithmetic and Win32 and nothing else.
using System.Runtime.InteropServices;

namespace Picadence;

internal static class Panel
{
    private const string ClassName = "PicadencePanel";
    private const int PanelIconId = 1;

    private static nint _window;
    private static nint _owner;
    private static PanelLayout _layout = PanelModel.Build([], DateTimeOffset.UtcNow, false);
    /// <summary>The scale the window is sized with, from its monitor's DPI.</summary>
    private static float _dpiScale = 1f;

    /// <summary>The scale it is painted with, derived from the size it actually got.</summary>
    private static float _scale = 1f;
    private static readonly Dictionary<int, nint> Fonts = [];

    /// <summary>Glyphs come from Segoe MDL2 Assets, so they cache apart from the text.</summary>
    private static readonly Dictionary<int, nint> Glyphs = [];

    /// <summary>Measured text widths, so a control is as wide as its words.</summary>
    private static readonly Dictionary<(int Size, string Text), double> Widths = [];
    private static readonly Dictionary<uint, nint> Brushes = [];

    public static bool IsOpen { get; private set; }

    /// <summary>The control under the pointer, or null.</summary>
    private static PanelAction? _hovered;

    /// <summary>The cadence under the pointer while the list is open.</summary>
    private static PanelFlyoutItem? _hoveredItem;

    /// <summary>The row whose pin is under the pointer, so the control reads as one.</summary>
    private static SessionRef? _hoveredPin;

    /// <summary>The row whose start control is under the pointer.</summary>
    private static SessionRef? _hoveredStart;

    /// <summary>Whether the tray is registered to start with Windows.</summary>
    private static bool _autostartOn;

    /// <summary>The session whose cadence list is open, or null.</summary>
    private static SessionRef? _flyoutFor;

    /// <summary>The first row the list is showing, so the wheel has somewhere to go.</summary>
    private static int _scroll;

    /// <summary>The last sessions read, so a toggle can repaint without the store.</summary>
    private static IReadOnlyList<AgentSession> _sessions = [];

    /// <summary>The cadences of those sessions, by session.</summary>
    private static Dictionary<SessionRef, Schedule> _schedules = [];

    private static IReadOnlyList<KeptSession> _kept = [];

    /// <summary>What starts a restart. The tray sets this; a test records it.</summary>
    internal static Func<Launch, bool> Launcher = _ => false;

    /// <summary>The moment the layout was built for, so a choice can time its first ping.</summary>
    private static DateTimeOffset _now = DateTimeOffset.Now;

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
    public static void Toggle(
        IReadOnlyList<AgentSession> sessions,
        IReadOnlyDictionary<SessionRef, Schedule> schedules,
        DateTimeOffset now,
        nint owner,
        IReadOnlyList<KeptSession> kept)
    {
        if (PanelController.After(IsOpen, PanelInput.IconClicked))
        {
            Show(sessions, schedules, now, owner, kept);
        }
        else
        {
            Hide();
        }
    }

    /// <summary>Repaints while open, so a state change shows without a reopen.</summary>
    public static void Update(
        IReadOnlyList<AgentSession> sessions,
        IReadOnlyDictionary<SessionRef, Schedule> schedules,
        DateTimeOffset now,
        IReadOnlyList<KeptSession> kept)
    {
        if (!IsOpen)
        {
            return;
        }

        _sessions = sessions;
        _schedules = new Dictionary<SessionRef, Schedule>(schedules);
        _kept = kept;
        _now = now;
        var next = PanelModel.Build(
            sessions, now, _autostartOn, _schedules, _flyoutFor, _kept, _scroll, Dependencies.Detect(), Measure);
        var grew = Math.Abs(next.Height - _layout.Height) > 0.5 || Math.Abs(next.Width - _layout.Width) > 0.5;
        _scroll = next.FirstRow;
        _layout = next;

        if (next.Flyout is null)
        {
            _flyoutFor = null;
        }

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
        _hovered = null;
        _hoveredItem = null;
        _hoveredPin = null;
        _hoveredStart = null;
        _flyoutFor = null;
    }

    private static void Show(
        IReadOnlyList<AgentSession> sessions,
        IReadOnlyDictionary<SessionRef, Schedule> schedules,
        DateTimeOffset now,
        nint owner,
        IReadOnlyList<KeptSession> kept)
    {
        _owner = owner;
        _sessions = sessions;
        _schedules = new Dictionary<SessionRef, Schedule>(schedules);
        _kept = kept;
        _now = now;
        _autostartOn = Autostart.Read() is not null;
        _flyoutFor = null;
        _scroll = 0;
        _layout = PanelModel.Build(
            sessions, now, _autostartOn, _schedules, kept: _kept, scroll: _scroll,
            dependencies: Dependencies.Detect(), measure: Measure);

        if (_window == 0)
        {
            _window = Native.CreateWindowExW(
                Native.WS_EX_TOOLWINDOW, ClassName, "picadence status", Native.WS_POPUP,
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
                // The list is the nearer thing, so Escape closes it first and the
                // panel second, the way a menu behaves.
                if (_flyoutFor is null)
                {
                    Hide();
                }
                else
                {
                    _flyoutFor = null;
                    Rebuild();
                }

                return 0;
            case Native.WM_MOUSEMOVE:
                Move((short)(lParam & 0xFFFF), (short)((lParam >> 16) & 0xFFFF));
                return 0;
            case Native.WM_MOUSEWHEEL when _layout.TotalRows > PanelModel.VisibleRows:
                // One notch is three lines, and each line is a row: three rows felt
                // like jumping, so one notch moves one row.
                var notches = (short)((wParam >> 16) & 0xFFFF) / 120;
                _scroll = Math.Max(0, _scroll - notches);
                _hoveredPin = null;
                Rebuild();
                return 0;
            case Native.WM_MOUSELEAVE:
                if (_hovered is not null || _hoveredItem is not null || _hoveredPin is not null || _hoveredStart is not null)
                {
                    _hovered = null;
                    _hoveredItem = null;
                    _hoveredPin = null;
                    _hoveredStart = null;
                    Native.InvalidateRect(hwnd, 0, false);
                }

                return 0;
            case Native.WM_SETCURSOR when (int)(lParam & 0xFFFF) == Native.HTCLIENT:
                Native.SetCursor(Cursor());
                return 1;
            case Native.WM_LBUTTONUP:
                Click((short)(lParam & 0xFFFF), (short)((lParam >> 16) & 0xFFFF));
                return 0;
            case Native.WM_RBUTTONUP:
                RightClick((short)(lParam & 0xFFFF), (short)((lParam >> 16) & 0xFFFF));
                return 0;
            default:
                return Native.DefWindowProcW(hwnd, message, wParam, lParam);
        }
    }

    /// <summary>Hover tracking, so a control looks clickable before it is clicked.</summary>
    private static void Move(int x, int y)
    {
        var pointX = x / _scale;
        var pointY = y / _scale;
        var hit = _layout.HitTest(pointX, pointY);
        PanelAction? over = hit switch
        {
            PanelHit.Autostart => PanelAction.Autostart,
            PanelHit.Quit => PanelAction.Quit,
            _ => null,
        };
        var overItem = hit == PanelHit.Flyout && _layout.Flyout is { } flyout
            ? ItemAt(flyout, pointX, pointY)
            : null;
        var overPin = hit == PanelHit.Pin ? RowAt(pointX, pointY)?.Reference : null;
        var overStart = hit == PanelHit.Start ? RowAt(pointX, pointY)?.Reference : null;

        if (over == _hovered && overItem == _hoveredItem && overPin == _hoveredPin && overStart == _hoveredStart)
        {
            return;
        }

        _hovered = over;
        _hoveredItem = overItem;
        _hoveredPin = overPin;
        _hoveredStart = overStart;
        Native.SetCursor(Cursor());

        if (over is not null || overItem is not null || overPin is not null || overStart is not null)
        {
            var tracking = new Native.TrackMouseEventData
            {
                cbSize = Marshal.SizeOf<Native.TrackMouseEventData>(),
                flags = Native.TME_LEAVE,
                hwndTrack = _window,
            };
            Native.TrackMouseEvent(ref tracking);
        }

        Native.InvalidateRect(_window, 0, false);
    }

    /// <summary>The row under a point, whether the point landed on its body or its pin.</summary>
    private static PanelRow? RowAt(double x, double y)
    {
        foreach (var row in _layout.Rows)
        {
            if (row.Bounds.Contains(x, y))
            {
                return row;
            }
        }

        return null;
    }

    private static PanelFlyoutItem? ItemAt(PanelFlyout flyout, double x, double y)
    {
        foreach (var item in flyout.Items)
        {
            if (item.Bounds.Contains(x, y))
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>
    /// A right click on a row opens its cadence list. Anywhere else closes the list
    /// and does nothing else, so a stray click cannot arm something.
    /// </summary>
    private static void RightClick(int x, int y)
    {
        var pointX = x / _scale;
        var pointY = y / _scale;
        for (var index = 0; index < _layout.Rows.Count; index++)
        {
            if (_layout.Rows[index].Bounds.Contains(pointX, pointY))
            {
                _flyoutFor = _layout.Rows[index].Reference;
                _hoveredItem = null;
                Rebuild();
                return;
            }
        }

        if (_flyoutFor is not null)
        {
            _flyoutFor = null;
            Rebuild();
        }
    }

    /// <summary>
    /// Carries out the item the user picked. A cadence writes or removes the record
    /// for that session; keep and stop keeping write or remove the pin; restart asks
    /// the tray to open one tab and does nothing else.
    /// </summary>
    private static void Choose(PanelFlyoutItem item)
    {
        if (_layout.Flyout is not { } flyout)
        {
            return;
        }

        var row = _layout.Rows[flyout.RowIndex];
        switch (item.Action)
        {
            case PanelFlyoutAction.Cadence:
                SetCadence(row, item.Cadence);
                break;
        }

        _flyoutFor = null;
        _hoveredItem = null;
        Rebuild();
    }

    /// <summary>
    /// Writes the chosen cadence for the row the list belongs to. Off removes the
    /// record, which is the only way a cadence is ever deleted.
    /// </summary>
    private static void SetCadence(PanelRow row, Cadence cadence)
    {
        var store = new ScheduleStore(Paths.Schedules);
        if (cadence == Cadence.Off)
        {
            store.Remove(row.Reference.SessionId);
            _schedules.Remove(row.Reference);
        }
        else
        {
            var schedule = new Schedule(
                row.Reference.SessionId,
                Name: null,
                row.Folder,
                cadence,
                NextAt: cadence.Advance(_now),
                LastIssuedAt: null,
                Missed: 0);
            store.Write(schedule);
            _schedules[row.Reference] = schedule;
        }
    }

    /// <summary>
    /// Keeps a session, or stops keeping it, which is what the pin on its row does.
    /// The conversation is recorded at the moment it is kept, while the session is
    /// still findable, because that is the only moment the search is cheap.
    /// </summary>
    private static void TogglePin(PanelRow row)
    {
        if (row.Keeping)
        {
            new PinStore(Paths.Pins).Remove(row.Reference.SessionId);
            _kept = KeptNow();
        }
        else
        {
            Keep(row);
        }
    }

    /// <summary>
    /// Keeps a session. The conversation is recorded now, while the session is still
    /// findable, because that is the only moment the search is cheap.
    /// </summary>
    private static void Keep(PanelRow row)
    {
        var session = _sessions.FirstOrDefault(
            candidate => string.Equals(
                candidate.Reference.SessionId, row.Reference.SessionId, StringComparison.Ordinal));
        // The row shows a folder name; a restart needs a place. pi reports both, and
        // the absolute one is what a pin keeps.
        var folder = session?.Cwd is { Length: > 0 } cwd ? cwd : row.Folder;
        var pin = new Pin(
            row.Reference.SessionId,
            session?.Name,
            folder,
            Transcripts.Find(Paths.PiSessions, row.Reference.SessionId),
            _now);
        new PinStore(Paths.Pins).Write(pin);
        _kept = KeptNow();
    }

    /// <summary>
    /// Starts one tab for a kept session that is not running, and only when the plan
    /// says it will work. A failure to start is the tray's to report, not the panel's
    /// to crash on.
    /// </summary>
    private static void RestartRow(PanelRow row)
    {
        var pin = new PinStore(Paths.Pins).ReadAll().FirstOrDefault(
            candidate => string.Equals(
                candidate.SessionId, row.Reference.SessionId, StringComparison.Ordinal));
        if (pin is null)
        {
            return;
        }

        var plan = Restart.View(pin, running: false);
        if (plan.Launch is { } launch)
        {
            Launcher(launch);
        }
    }

    /// <summary>Every pin, with the answer to whether it could be restarted now.</summary>
    private static IReadOnlyList<KeptSession> KeptNow()
    {
        var running = new HashSet<string>(
            _sessions.Select(session => session.Reference.SessionId), StringComparer.Ordinal);
        return
        [
            .. new PinStore(Paths.Pins)
                .ReadAll()
                .Select(pin => new KeptSession(pin, Restart.View(pin, running.Contains(pin.SessionId)))),
        ];
    }

    private static void Rebuild()
    {
        var next = PanelModel.Build(
            _sessions, _now, _autostartOn, _schedules, _flyoutFor, _kept, _scroll, Dependencies.Detect(), Measure);
        _scroll = next.FirstRow;
        _layout = next;

        if (next.Flyout is null)
        {
            _flyoutFor = null;
        }

        Native.InvalidateRect(_window, 0, false);
    }

    /// <summary>The pointer the point under the cursor asks for.</summary>
    private static nint Cursor()
    {
        var hit = PanelHit.None;
        if (Native.GetCursorPos(out var point) && Native.ScreenToClient(_window, ref point))
        {
            hit = _layout.HitTest(point.x / _scale, point.y / _scale);
        }

        return PanelCursor.For(hit) == PanelPointer.Hand
            ? Native.LoadCursorW(0, Native.IDC_HAND)
            : Native.LoadCursorW(0, Native.IDC_ARROW);
    }

    private static void Click(int x, int y)
    {
        var pointX = x / _scale;
        var pointY = y / _scale;

        // With the list open, it takes the click: a choice inside it, and anything
        // else only dismisses it.
        if (_layout.Flyout is { } flyout)
        {
            if (ItemAt(flyout, pointX, pointY) is { } item)
            {
                Choose(item);
            }
            else
            {
                _flyoutFor = null;
                _hoveredItem = null;
                Rebuild();
            }

            return;
        }

        // A session row and empty space both do nothing: the panel reports, it
        // does not act on a session.
        switch (_layout.HitTest(pointX, pointY))
        {
            case PanelHit.Start when RowAt(pointX, pointY) is { } started:
                RestartRow(started);
                _hoveredStart = null;
                break;
            case PanelHit.Pin when RowAt(pointX, pointY) is { } pinned:
                TogglePin(pinned);
                _hoveredPin = null;
                Rebuild();
                break;
            case PanelHit.Quit:
                Hide();
                Native.PostMessage(_owner, Native.WM_CLOSE, 0, 0);
                break;
            case PanelHit.Autostart:
                _autostartOn = Autostart.Toggle(_autostartOn);
                if (_autostartOn)
                {
                    Autostart.Write(Environment.ProcessPath ?? string.Empty);
                }
                else
                {
                    Autostart.Remove();
                }

                _layout = PanelModel.Build(
                    _sessions, _now, _autostartOn, _schedules, _flyoutFor, _kept, _scroll,
                    Dependencies.Detect(), Measure);
                Native.InvalidateRect(_window, 0, false);
                break;
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
            var controlsLeft = row.StartBounds.Width > 0 ? row.StartBounds.X : row.PinBounds.X;
            var textWidth = controlsLeft - 8 - textLeft;

            Text(dc, row.Folder, new DipRect(textLeft, row.Bounds.Y + 5, textWidth, 20),
                14, Native.FW_NORMAL, PanelTheme.PrimaryText,
                Native.DT_LEFT | Native.DT_SINGLELINE | Native.DT_END_ELLIPSIS);

            var state = string.IsNullOrEmpty(row.Detail) ? row.StateWord : $"{row.StateWord} ({row.Detail})";
            if (row.Schedule is { Length: > 0 } schedule)
            {
                state = $"{state} · {schedule}";
            }

            Text(dc, state, new DipRect(textLeft, row.Bounds.Y + 25, textWidth, 16),
                12, Native.FW_NORMAL, PanelTheme.SecondaryText,
                Native.DT_LEFT | Native.DT_SINGLELINE | Native.DT_END_ELLIPSIS);

            Text(dc, row.Age, row.AgeBounds, 12, Native.FW_NORMAL, PanelTheme.SecondaryText,
                Native.DT_RIGHT | Native.DT_SINGLELINE);

            if (row.StartBounds.Width > 0)
            {
                if (_hoveredStart == row.Reference)
                {
                    Control(dc, row.StartBounds);
                }

                Text(
                    dc,
                    PanelModel.StartGlyph,
                    row.StartBounds,
                    12,
                    Native.FW_NORMAL,
                    PanelTheme.PrimaryText,
                    Native.DT_CENTER | Native.DT_VCENTER | Native.DT_SINGLELINE,
                    GlyphFont(12));
            }

            // The pin says which state it is in by its shape, and the colour repeats
            // it. A hover fill is what makes it read as a control rather than a mark.
            if (_hoveredPin == row.Reference)
            {
                Control(dc, row.PinBounds);
            }

            Text(
                dc,
                row.Keeping ? PanelModel.KeptGlyph : PanelModel.KeepGlyph,
                row.PinBounds,
                12,
                Native.FW_NORMAL,
                row.Keeping ? PanelTheme.PrimaryText : PanelTheme.SecondaryText,
                Native.DT_CENTER | Native.DT_VCENTER | Native.DT_SINGLELINE,
                GlyphFont(12));
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

        if (_layout.Notice is not null)
        {
            Text(dc, _layout.Notice, _layout.NoticeBounds, 12, Native.FW_NORMAL,
                PanelTheme.SecondaryText, Native.DT_LEFT | Native.DT_SINGLELINE | Native.DT_END_ELLIPSIS);
        }

        if (_layout.ScrollText is not null)
        {
            Text(dc, _layout.ScrollText, _layout.ScrollBounds, 12, Native.FW_NORMAL,
                PanelTheme.SecondaryText, Native.DT_LEFT | Native.DT_SINGLELINE);
        }

        foreach (var action in _layout.Actions)
        {
            if (_hovered == action.Action)
            {
                var box = Scaled(inset(action.Bounds, PanelModel.ActionInset));
                var previousPen = Native.SelectObject(dc, Native.GetStockObject(Native.NULL_PEN));
                var previousBrush = Native.SelectObject(dc, Brush(PanelTheme.Hover));
                var radius = (int)Math.Round(4 * _scale);
                Native.RoundRect(dc, box.left, box.top, box.right, box.bottom, radius, radius);
                Native.SelectObject(dc, previousBrush);
                Native.SelectObject(dc, previousPen);
            }

            if (action.BoxBounds.Width > 0)
            {
                // The box says checked or not by its mark, which is a shape rather
                // than a colour, and the outline makes an empty box read as a box.
                var box = Scaled(action.BoxBounds);
                Native.FrameRect(dc, ref box, Brush(PanelTheme.SecondaryText));

                if (action.Checked)
                {
                    Text(
                        dc,
                        PanelModel.CheckGlyph,
                        action.BoxBounds,
                        12,
                        Native.FW_NORMAL,
                        PanelTheme.PrimaryText,
                        Native.DT_CENTER | Native.DT_VCENTER | Native.DT_SINGLELINE,
                        GlyphFont(12));
                }
            }

            if (action.Glyph is { Length: > 0 } glyph)
            {
                Text(dc, glyph, action.LabelBounds, 12, Native.FW_NORMAL, PanelTheme.PrimaryText,
                    Native.DT_CENTER | Native.DT_VCENTER | Native.DT_SINGLELINE, GlyphFont(12));
            }
            else
            {
                Text(dc, action.Label, action.LabelBounds, 12, Native.FW_NORMAL, PanelTheme.PrimaryText,
                    Native.DT_LEFT | Native.DT_VCENTER | Native.DT_SINGLELINE);
            }
        }

        if (_layout.Flyout is { } flyout)
        {
            var box = Scaled(flyout.Bounds);
            Native.FillRect(dc, ref box, Brush(PanelTheme.Surface));
            Native.FrameRect(dc, ref box, Brush(PanelTheme.Outline));

            Text(dc, flyout.Header, flyout.HeaderBounds, 12, Native.FW_SEMIBOLD, PanelTheme.PrimaryText,
                Native.DT_LEFT | Native.DT_VCENTER | Native.DT_SINGLELINE);

            var rule = Scaled(new DipRect(
                flyout.HeaderBounds.X,
                flyout.HeaderBounds.Y + flyout.HeaderBounds.Height,
                flyout.HeaderBounds.Width,
                PanelModel.FlyoutRule));
            Native.FillRect(dc, ref rule, Brush(PanelTheme.Outline));

            foreach (var item in flyout.Items)
            {
                if (_hoveredItem == item)
                {
                    var hover = Scaled(item.Bounds);
                    Native.FillRect(dc, ref hover, Brush(PanelTheme.Hover));
                }

                // The chosen cadence is the one in full white; the others are
                // secondary, which is the same pair the panel already proves.
                var label = new DipRect(
                    item.Bounds.X + 8, item.Bounds.Y, item.Bounds.Width - 16, item.Bounds.Height);
                Text(dc, item.Label, label, 12,
                    item.Selected ? Native.FW_SEMIBOLD : Native.FW_NORMAL,
                    item.Selected ? PanelTheme.PrimaryText : PanelTheme.SecondaryText,
                    Native.DT_LEFT | Native.DT_VCENTER | Native.DT_SINGLELINE);
            }
        }
    }

    /// <summary>The rounded fill behind a control the pointer is over.</summary>
    private static void Control(nint dc, DipRect bounds)
    {
        var hover = Scaled(bounds);
        var previousPen = Native.SelectObject(dc, Native.GetStockObject(Native.NULL_PEN));
        var previousBrush = Native.SelectObject(dc, Brush(PanelTheme.Hover));
        var radius = (int)Math.Round(4 * _scale);
        Native.RoundRect(dc, hover.left, hover.top, hover.right, hover.bottom, radius, radius);
        Native.SelectObject(dc, previousBrush);
        Native.SelectObject(dc, previousPen);
    }

    private static void Text(
        nint dc,
        string text,
        DipRect bounds,
        int size,
        int weight,
        Rgb colour,
        int flags,
        nint? font = null)
    {
        var rect = Scaled(bounds);
        font ??= Font(size, weight);
        Native.SetTextColor(dc, colour.ToColorRef());
        var previous = Native.SelectObject(dc, font.Value);
        Native.DrawTextW(dc, text, text.Length, ref rect, flags | Native.DT_NOPREFIX);
        Native.SelectObject(dc, previous);
    }

    /// <summary>
    /// How wide a string is in the panel's own text font, in device-independent
    /// pixels. Measured once per string and scale: the panel builds several times a
    /// second, and the answer cannot change between two ticks at one scale.
    /// </summary>
    private static double Measure(string text)
    {
        if (Widths.TryGetValue((12, text), out var cached))
        {
            return cached;
        }

        var scale = _scale <= 0 ? 1 : _scale;
        var dc = Native.CreateCompatibleDC(0);
        double width;
        if (dc == 0)
        {
            width = text.Length * PanelModel.EstimatePerCharacter;
        }
        else
        {
            var previous = Native.SelectObject(dc, Font(12, Native.FW_NORMAL));
            Native.GetTextExtentPoint32W(dc, text, text.Length, out var extent);
            Native.SelectObject(dc, previous);
            Native.DeleteDC(dc);
            width = extent.cx / scale;
        }

        Widths[(12, text)] = width;
        return width;
    }

    /// <summary>The icon font every Windows install carries, for the one glyph the panel draws.</summary>
    private static nint GlyphFont(int size)
    {
        if (Glyphs.TryGetValue(size, out var cached))
        {
            return cached;
        }

        var font = Native.CreateFontW(
            -(int)Math.Round(size * _scale), 0, 0, 0, Native.FW_NORMAL, 0, 0, 0, 1, 0, 0, 5, 0, "Segoe MDL2 Assets");
        Glyphs[size] = font;
        return font;
    }

    private static DipRect inset(DipRect bounds, double by) =>
        new(bounds.X + by, bounds.Y + by, bounds.Width - (2 * by), bounds.Height - (2 * by));

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
        Glyphs.Clear();
        Widths.Clear();
    }
}
