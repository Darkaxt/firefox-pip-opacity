param(
    [string]$BuildDirectory = "$PSScriptRoot\..\src\PipOpacity.Tray\bin\Release\net8.0-windows",
    [string]$LogPath = "$PSScriptRoot\..\artifacts\verification\browser-smoke.log"
)

$ErrorActionPreference = 'Stop'
[void][Reflection.Assembly]::LoadFrom((Join-Path $BuildDirectory 'PipOpacity.Core.dll'))
$tray = [Reflection.Assembly]::LoadFrom((Join-Path $BuildDirectory 'PipOpacity.Tray.dll'))
$loggerType = $tray.GetType('PipOpacity.Tray.FileLogger')
$logger = $loggerType.GetConstructors()[0].Invoke([object[]]@($LogPath))
$enumerator = $tray.GetType('PipOpacity.Tray.WindowEnumerator').GetConstructors()[0].Invoke([object[]]@($logger))
$native = $tray.GetType('PipOpacity.Tray.NativeMethods')
$getStyle = $native.GetMethod('GetWindowExStyle')
$windows = $enumerator.EnumerateTopLevelWindows()
$matched = @($windows | Where-Object { [PipOpacity.Core.PipWindowMatcher]::IsBrowserPictureInPicture($_) })
if ($matched.Count -ne 1) { throw 'Open exactly one disposable browser video PiP before this smoke test.' }
$target = $matched[0]
$original = $getStyle.Invoke($null, [object[]]@($target.Handle))
$untouched = @{}
foreach ($window in $windows) {
    if ($window.IsVisible -and $window.Handle -ne $target.Handle) {
        $untouched[$window.Handle] = $getStyle.Invoke($null, [object[]]@($window.Handle))
    }
}

Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class PipSmokeAlpha {
    [DllImport("user32.dll", SetLastError=true)]
    public static extern bool GetLayeredWindowAttributes(IntPtr h, out uint key, out byte alpha, out uint flags);
}
'@

$controller = $tray.GetType('PipOpacity.Tray.WindowOpacityController').GetConstructors()[0].Invoke(
    [object[]]@($enumerator, $logger, [Action[string]]{ param($message) throw $message }))
try {
    $config = [PipOpacity.Core.PipOpacityConfig]::CreateDefault()
    $config.OpacityPercent = 50
    $config.ClickThrough = $true
    if ($controller.Apply($config) -ne 1) { throw 'Expected exactly one controlled window.' }
    $style = $getStyle.Invoke($null, [object[]]@($target.Handle))
    if (($style -band 0x80020) -ne 0x80020) { throw 'Layered/click-through styles missing.' }
    [uint32]$color = 0; [byte]$alpha = 0; [uint32]$flags = 0
    if (-not [PipSmokeAlpha]::GetLayeredWindowAttributes($target.Handle, [ref]$color, [ref]$alpha, [ref]$flags) -or $alpha -ne 128) {
        throw "Expected alpha 128; got $alpha."
    }
    $config.ClickThrough = [PipOpacity.Core.ClickThroughPolicy]::GetEffectiveClickThrough($true, $true)
    [void]$controller.Apply($config)
    if (($getStyle.Invoke($null, [object[]]@($target.Handle)) -band 0x20) -ne 0) { throw 'Ctrl bypass did not clear click-through.' }
    $config.ClickThrough = [PipOpacity.Core.ClickThroughPolicy]::GetEffectiveClickThrough($true, $false)
    [void]$controller.Apply($config)
    if (($getStyle.Invoke($null, [object[]]@($target.Handle)) -band 0x20) -eq 0) { throw 'Click-through did not return after bypass.' }
    $config.Enabled = $false
    [void]$controller.Apply($config)
    if ($getStyle.Invoke($null, [object[]]@($target.Handle)) -ne $original) { throw 'Pause did not restore the original style.' }
    $config.Enabled = $true
    [void]$controller.Apply($config)
} finally {
    $controller.RestoreAll()
}
if ($getStyle.Invoke($null, [object[]]@($target.Handle)) -ne $original) { throw 'Exit restore changed the original style.' }
foreach ($handle in $untouched.Keys) {
    if ($native.GetMethod('IsWindow').Invoke($null, [object[]]@($handle)) -and
        $getStyle.Invoke($null, [object[]]@($handle)) -ne $untouched[$handle]) {
        throw "Unmatched window $handle changed style."
    }
}
[pscustomobject]@{ Result = 'PASS'; Process = $target.ProcessPath; Title = $target.Title; Class = $target.ClassName;
    Alpha = 128; ClickThrough = 'verified'; Bypass = 'verified'; PauseAndExitRestore = 'verified'; UnmatchedWindowsUnchanged = $untouched.Count }
