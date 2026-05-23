namespace PipOpacity.Tray;

using PipOpacity.Core;

internal sealed class WindowOpacityController
{
    private readonly WindowEnumerator windowEnumerator;
    private readonly WindowStyleStateStore stateStore = new();
    private readonly FileLogger logger;
    private readonly Action<string> warnOnce;
    private bool warnedAboutWindowAccess;

    public WindowOpacityController(WindowEnumerator windowEnumerator, FileLogger logger, Action<string> warnOnce)
    {
        this.windowEnumerator = windowEnumerator;
        this.logger = logger;
        this.warnOnce = warnOnce;
    }

    public int Apply(PipOpacityConfig config)
    {
        if (!config.Enabled)
        {
            RestoreAll();
            return 0;
        }

        var matchedHandles = new HashSet<nint>();
        var matchedCount = 0;

        foreach (var window in windowEnumerator.EnumerateTopLevelWindows().Where(PipWindowMatcher.IsFirefoxPictureInPicture))
        {
            matchedHandles.Add(window.Handle);
            matchedCount++;
            ApplyToWindow(window.Handle, config);
        }

        RestoreOrForgetStaleHandles(matchedHandles);
        return matchedCount;
    }

    public void RestoreAll()
    {
        foreach (var entry in stateStore.Snapshot())
        {
            RestoreWindow(entry.Key, entry.Value);
        }

        stateStore.Clear();
    }

    private void ApplyToWindow(nint handle, PipOpacityConfig config)
    {
        if (config.OpacityPercent >= OpacityPolicy.MaximumPercent && !config.ClickThrough)
        {
            RestoreKnownWindow(handle);
            return;
        }

        var originalStyle = NativeMethods.GetWindowExStyle(handle);
        stateStore.RememberOriginal(handle, originalStyle);

        if (!stateStore.TryGetOriginal(handle, out originalStyle))
        {
            originalStyle = NativeMethods.GetWindowExStyle(handle);
        }

        var desiredStyle = originalStyle | NativeMethods.WsExLayered;
        if (config.ClickThrough)
        {
            desiredStyle |= NativeMethods.WsExTransparent;
        }

        if (NativeMethods.GetWindowExStyle(handle) != desiredStyle && !NativeMethods.SetWindowExStyle(handle, desiredStyle))
        {
            HandleWindowAccessFailure(handle, "set extended style");
            return;
        }

        var alpha = OpacityPolicy.PercentToAlpha(config.OpacityPercent);
        if (!NativeMethods.SetLayeredWindowAttributes(handle, crKey: 0, alpha, NativeMethods.LwaAlpha))
        {
            HandleWindowAccessFailure(handle, "set layered opacity");
        }
    }

    private void RestoreOrForgetStaleHandles(HashSet<nint> matchedHandles)
    {
        foreach (var entry in stateStore.Snapshot())
        {
            if (matchedHandles.Contains(entry.Key))
            {
                continue;
            }

            if (NativeMethods.IsWindow(entry.Key))
            {
                RestoreWindow(entry.Key, entry.Value);
            }

            stateStore.Forget(entry.Key);
        }
    }

    private void RestoreKnownWindow(nint handle)
    {
        if (!stateStore.TryGetOriginal(handle, out var originalStyle))
        {
            return;
        }

        RestoreWindow(handle, originalStyle);
        stateStore.Forget(handle);
    }

    private void RestoreWindow(nint handle, int originalStyle)
    {
        if (!NativeMethods.IsWindow(handle))
        {
            return;
        }

        if (!NativeMethods.SetWindowExStyle(handle, originalStyle))
        {
            HandleWindowAccessFailure(handle, "restore extended style");
        }
    }

    private void HandleWindowAccessFailure(nint handle, string operation)
    {
        var message = $"Could not {operation} for window 0x{handle:X}. Firefox may be running elevated.";
        logger.Error(message);

        if (warnedAboutWindowAccess)
        {
            return;
        }

        warnedAboutWindowAccess = true;
        warnOnce("Could not change a Firefox PiP window. If Firefox is running as administrator, run this app the same way or restart Firefox normally.");
    }
}
