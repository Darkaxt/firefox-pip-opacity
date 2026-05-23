# Changelog

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
