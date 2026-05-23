namespace PipOpacity.Tray;

using PipOpacity.Core;

internal sealed class WindowOpacityController
{
    private readonly WindowEnumerator windowEnumerator;
    private readonly WindowStateStore stateStore = new();
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
        var currentStyle = NativeMethods.GetWindowExStyle(handle);
        stateStore.RememberOriginal(
            handle,
            new WindowOriginalState(
                ExtendedStyle: currentStyle,
                WasTopMost: (currentStyle & NativeMethods.WsExTopmost) == NativeMethods.WsExTopmost));

        if (!stateStore.TryGetOriginal(handle, out var originalState))
        {
            originalState = new WindowOriginalState(
                ExtendedStyle: currentStyle,
                WasTopMost: (currentStyle & NativeMethods.WsExTopmost) == NativeMethods.WsExTopmost);
        }

        var desiredStyle = BuildDesiredExtendedStyle(originalState.ExtendedStyle, config);
        if (currentStyle != desiredStyle && !NativeMethods.SetWindowExStyle(handle, desiredStyle))
        {
            HandleWindowAccessFailure(handle, "set extended style");
            return;
        }

        if ((desiredStyle & NativeMethods.WsExLayered) == NativeMethods.WsExLayered)
        {
            var alpha = OpacityPolicy.PercentToAlpha(config.OpacityPercent);
            if (!NativeMethods.SetLayeredWindowAttributes(handle, crKey: 0, alpha, NativeMethods.LwaAlpha))
            {
                HandleWindowAccessFailure(handle, "set layered opacity");
            }
        }

        ApplyTopMost(handle, config.AlwaysOnTop);
    }

    private static int BuildDesiredExtendedStyle(int originalStyle, PipOpacityConfig config)
    {
        var desiredStyle = originalStyle;

        if (config.OpacityPercent < OpacityPolicy.MaximumPercent || config.ClickThrough)
        {
            desiredStyle |= NativeMethods.WsExLayered;
        }

        if (config.ClickThrough)
        {
            desiredStyle |= NativeMethods.WsExTransparent;
        }

        desiredStyle = config.AlwaysOnTop
            ? desiredStyle | NativeMethods.WsExTopmost
            : desiredStyle & ~NativeMethods.WsExTopmost;

        return desiredStyle;
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

    private void RestoreWindow(nint handle, WindowOriginalState originalState)
    {
        if (!NativeMethods.IsWindow(handle))
        {
            return;
        }

        if (!NativeMethods.SetWindowExStyle(handle, originalState.ExtendedStyle))
        {
            HandleWindowAccessFailure(handle, "restore extended style");
        }

        ApplyTopMost(handle, originalState.WasTopMost);
    }

    private void ApplyTopMost(nint handle, bool alwaysOnTop)
    {
        var insertAfter = alwaysOnTop ? NativeMethods.HwndTopMost : NativeMethods.HwndNotTopMost;
        var flags = NativeMethods.SwpNomove | NativeMethods.SwpNosize | NativeMethods.SwpNoactivate;

        if (!NativeMethods.SetWindowPos(handle, insertAfter, 0, 0, 0, 0, flags))
        {
            HandleWindowAccessFailure(handle, alwaysOnTop ? "set topmost state" : "clear topmost state");
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
