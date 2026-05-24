# Firefox PiP Opacity

A tiny Windows tray utility for controlling Firefox Picture-in-Picture window opacity and optional click-through behavior.

Firefox does not expose a native opacity slider for PiP windows. This app takes the OS-level route: it watches for Firefox PiP windows and applies Win32 layered-window opacity only to those windows.

## Features

- Targets Firefox PiP windows only.
- Adjustable opacity from 0% to 100%.
- Optional click-through mode.
- Hold `Ctrl` while click-through is enabled to temporarily interact with the PiP window using the mouse.
- WPF settings window with a real opacity slider.
- Minimal tray menu for open, reset, startup, and exit.
- Global hotkeys for quick opacity changes.
- Restores modified window styles when paused or closed.
- Runs as a user-session tray app, not a Windows service.

## Install

Download the latest release ZIP from GitHub, extract it somewhere permanent, and run:

```powershell
PipOpacity.Tray.exe
```

Keep the DLL files from the ZIP next to the executable.

The app appears as `Firefox PiP Opacity` in the notification area. Left-click or double-click the tray icon to open settings. Use the tray menu to reset, exit, or enable Start with Windows.

## Hotkeys

- `Ctrl+Alt++` or `Ctrl+Alt+Numpad +`: increase opacity
- `Ctrl+Alt+-` or `Ctrl+Alt+Numpad -`: decrease opacity
- `Ctrl+Alt+0`: reset PiP windows to normal
- `Ctrl+Alt+T`: toggle click-through

Hotkeys do nothing when no Firefox PiP window is detected.

When click-through is enabled, hold either `Ctrl` key before using the mouse on the PiP window. The app temporarily removes click-through while `Ctrl` is down and restores it when released.

## Matching Rules

A window is controlled only when it is a visible top-level Firefox PiP window with:

- process path ending in `firefox.exe`
- title exactly `Picture-in-Picture`
- class exactly `MozillaDialogClass`
- no owner window
- nonzero size

Normal Firefox windows and other apps are ignored.

## Files

- Config: `%APPDATA%\PipOpacity\config.json`
- Log: `%LOCALAPPDATA%\PipOpacity\logs\pip-opacity.log`
- Startup registration: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\PipOpacity`

Disable startup from the tray menu, or remove the registry value above.

## Build

Requirements:

- Windows
- .NET 8 SDK or newer

Build and test:

```powershell
dotnet build PipOpacity.slnx -c Release
dotnet test PipOpacity.slnx -c Release
```

Publish a self-contained Windows build:

```powershell
dotnet publish src\PipOpacity.Tray\PipOpacity.Tray.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=false -o publish
```

The publish output includes the executable plus a few native WindowsDesktop runtime DLLs. Ship the whole output directory together.

## Notes

If Firefox is running elevated, Windows may block this app from changing its PiP window. Run both apps at the same privilege level or restart Firefox normally.

The app icon is derived from the Windowed browser extension icon. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
