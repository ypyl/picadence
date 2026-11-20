# Tasks

## 1. The model

- [x] 1.1 `PanelTheme`: the palette and the contrast maths. Verified: the contrast
      test reads the same constants the painter uses.
- [x] 1.2 `PanelModel`: width, rows, empty state, header summary, the eight-row cap
      with a "more" row, and the Quit rect. Verified: the listing, empty, state
      word, and hit-test tests.
- [x] 1.3 `PanelPlacement`: the anchoring and clamping rule. Verified: the
      work-area test walks all four corners and the too-tall case.
- [x] 1.4 `PanelController`: open and close on click, Escape, and focus loss.
      Verified: the click, Escape, and focus tests.

## 2. The window

- [x] 2.1 `Panel.cs`: register the popup class with a drop shadow, create the
      window once and hide it, and show it on demand. Verified: the runtime smoke
      shows the panel and the screenshot of it.
- [x] 2.2 Paint: memory bitmap, blit once, fonts per DPI, dot per row, header,
      empty state, footer. Verified: the screenshot, and no flicker while the
      timer refreshes.
- [x] 2.3 Input: left click and right click open it, Escape and focus loss close
      it, Quit ends the process, a session row does nothing. Verified: the
      controller tests plus the smoke.
- [x] 2.4 DPI: per-monitor v2 at startup and one scale factor through the paint.
      Verified: the panel is crisp at 100% and at a scaled monitor, checked by eye
      in the smoke.
- [x] 2.5 Delete `AgentStatus.Menu` and `MenuEntry` and their tests, since the
      panel replaces them. Verified: nothing references them, and its two scenario
      tests are gone with the requirements they proved.

## 3. Documents

- [x] 3.1 `README.md`: what a click does, and the panel in one paragraph.
      Verified: no sentence describes a menu.
- [x] 3.2 `ARCHITECTURE.md`: the panel as part of the tray container, not a new
      one. Verified: no invented container.
- [x] 3.3 `CHANGELOG.md` entry, `CONVENTIONS.md` rule for DIPs versus pixels.
      Verified: the changelog heading survives and names no version.

## 4. Gates and finish

- [x] 4.1 `bash scripts/verify.sh`, then run the tray with a live session, open
      the panel, capture it to a PNG, and look at it. Verified: the capture is in
      the archive commit's evidence.
- [x] 4.2 `review-change`, with the security checklist for the new window and the
      paint path. Verified: every finding fixed or recorded.
- [x] 4.3 Archive, commit, merge. Verified: the delta replaces the menu
      requirement in `openspec/specs/agent-status/spec.md`, and the tests that
      proved it are gone with it.
