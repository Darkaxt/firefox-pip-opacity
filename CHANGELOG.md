# Changelog

## v0.2.1 - 2026-05-23

- Rolled back topmost control because it interfered with Firefox PiP media interaction.
- Removed the topmost tray item and `Ctrl+Alt+A` hotkey.
- Returned window state tracking to opacity and click-through styles only.
- Expanded opacity control to the full 0% to 100% range.
- Replaced the WinForms tray settings UI with a WPF settings window to avoid corrupted context-menu rendering.
- Kept the tray right-click menu minimal: open, reset, startup, and exit.
- Added a temporary click-through bypass while either `Ctrl` key is held.
- Fixed a settings-window freeze by moving periodic PiP scan/apply work off the WPF UI thread and avoiding process-path inspection for unrelated windows.

## v0.2.0 - 2026-05-23

- Added an `Always on top` tray toggle for Firefox PiP windows.
- Added `Ctrl+Alt+A` to toggle always-on-top from the keyboard.
- Unchecked topmost mode now actively makes detected PiP windows not-topmost.
- Pause and exit restore each managed PiP window's original topmost state.
- Reset now returns opacity to 100%, disables click-through, and sets always-on-top off.

## v0.1.0 - 2026-05-23

Initial public release.

- Firefox Picture-in-Picture window detection.
- Opacity control from the tray menu and global hotkeys.
- Optional click-through mode.
- Reset and pause behavior that restores original window styles.
- Per-user config, file logging, and Start with Windows support.
- White Windowed-derived app icon.
