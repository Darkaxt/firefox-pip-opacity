# Browser PiP Support

## Requirements

- R1: Apply existing opacity and click-through behavior to Firefox and Chromium video PiP windows, including Chrome and Helium. Match visible, positive-size, unowned top-level windows using exact class/title and a browser executable allowlist. Never match by class or title alone.
- R2: Preserve Firefox matching, Ctrl mouse bypass, AltGr fix, configuration paths, startup identity and executable name. Do not alter topmost state. Use the neutral display name PiP Opacity.
- R3: Verify matching against negative fixtures and a real Helium video PiP, including opacity, transparent style, bypass, restoration and unchanged normal browser styles. Chromium support initially requires the English native title Picture in picture; document PiP and unknown browsers fail closed.
- R4: Deliver v0.3.0 through the existing public Darkaxt/firefox-pip-opacity GitHub release channel with a self-contained Windows x64 ZIP. Verify the downloaded asset against the built package, source tag and binary version. Preserve current unrelated local changes.

## Stages And Reconciliation

1. Browser support: COMPLETE. R1-R3 verified: Release build clean; matching/negative fixtures and existing regression suite pass. Live Helium smoke verified alpha 128, click-through, bypass policy applied through the native controller, pause/exit restore, and unchanged styles on 34 unmatched visible windows. Physical Ctrl input on Helium was not synthesized; its existing poll path is unchanged. No scope-related design debt, blockers or deferrals identified.
2. Delivery: ACTIVE. R4; commit/push/tag, package, publish and independently download/verify release. Keep the running installed tray process intact; deliver the upgrade as a versioned release package.

## Detection Evidence

On 2026-10-23, a synthetic canvas video in a task-owned Helium tab produced a visible unowned Chrome_WidgetWin_1 window titled Picture in picture. Helium's executable was LocalAppData/imput/Helium/Application/chrome.exe. The normal browser window was titled PiP Opacity Verification - Helium and must not match. Chromium's video overlay source uses the PictureInPictureWindow widget name and a localized PiP title (chrome/browser/ui/views/overlay/video_overlay_window_views.cc).
