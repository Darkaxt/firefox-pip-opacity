namespace PipOpacity.Tray;

using PipOpacity.Core;

internal sealed class PipOpacityApplicationContext : ApplicationContext
{
    private readonly FileLogger logger = new(AppPaths.LogPath);
    private readonly ConfigStore configStore;
    private readonly StartupRegistration startupRegistration;
    private readonly WindowOpacityController controller;
    private readonly HotkeyMessageWindow hotkeyWindow;
    private readonly NotifyIcon notifyIcon;
    private readonly System.Windows.Forms.Timer scanTimer;
    private readonly ToolStripMenuItem enabledItem;
    private readonly ToolStripMenuItem opacityLabelItem;
    private readonly ToolStripMenuItem clickThroughItem;
    private readonly ToolStripMenuItem alwaysOnTopItem;
    private readonly ToolStripMenuItem startupItem;
    private readonly TrackBar opacityTrackBar;
    private readonly Icon appIcon;

    private PipOpacityConfig config;

    public PipOpacityApplicationContext()
    {
        configStore = new ConfigStore(AppPaths.ConfigPath, logger);
        startupRegistration = new StartupRegistration(logger);
        config = configStore.Load();
        config.StartWithWindows = startupRegistration.IsEnabled();

        controller = new WindowOpacityController(
            new WindowEnumerator(logger),
            logger,
            ShowOneTimeWarning);

        enabledItem = new ToolStripMenuItem("Enable opacity control")
        {
            CheckOnClick = true,
        };
        enabledItem.Click += (_, _) =>
        {
            config.Enabled = enabledItem.Checked;
            if (!config.Enabled)
            {
                controller.RestoreAll();
            }

            SaveAndApply();
        };

        opacityLabelItem = new ToolStripMenuItem();
        opacityTrackBar = new TrackBar
        {
            Minimum = OpacityPolicy.MinimumPercent,
            Maximum = OpacityPolicy.MaximumPercent,
            TickFrequency = OpacityPolicy.StepPercent,
            SmallChange = OpacityPolicy.StepPercent,
            LargeChange = OpacityPolicy.StepPercent,
            AutoSize = false,
            Width = 180,
            Height = 42,
        };
        opacityTrackBar.Scroll += (_, _) => SetOpacity(RoundToStep(opacityTrackBar.Value));
        opacityTrackBar.ValueChanged += (_, _) => SetOpacity(RoundToStep(opacityTrackBar.Value));

        clickThroughItem = new ToolStripMenuItem("Click-through")
        {
            CheckOnClick = true,
        };
        clickThroughItem.Click += (_, _) =>
        {
            config.ClickThrough = clickThroughItem.Checked;
            SaveAndApply();
        };

        alwaysOnTopItem = new ToolStripMenuItem("Always on top")
        {
            CheckOnClick = true,
        };
        alwaysOnTopItem.Click += (_, _) =>
        {
            config.AlwaysOnTop = alwaysOnTopItem.Checked;
            SaveAndApply();
        };

        var resetItem = new ToolStripMenuItem("Reset PiP windows to normal", null, (_, _) => ResetToNormal());

        startupItem = new ToolStripMenuItem("Start with Windows")
        {
            CheckOnClick = true,
        };
        startupItem.Click += (_, _) => ToggleStartup();

        var exitItem = new ToolStripMenuItem("Exit", null, (_, _) => ExitThread());

        var menu = new ContextMenuStrip();
        menu.Items.Add(enabledItem);
        menu.Items.Add(opacityLabelItem);
        menu.Items.Add(new ToolStripControlHost(opacityTrackBar)
        {
            AutoSize = false,
            Width = 210,
            Height = 48,
        });
        menu.Items.Add(clickThroughItem);
        menu.Items.Add(alwaysOnTopItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(resetItem);
        menu.Items.Add(startupItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        appIcon = LoadAppIcon();
        notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = menu,
            Icon = appIcon,
            Text = "Firefox PiP Opacity",
            Visible = true,
        };
        notifyIcon.DoubleClick += (_, _) => ResetToNormal();

        hotkeyWindow = new HotkeyMessageWindow(logger);
        hotkeyWindow.HotkeyPressed += (_, command) => HandleHotkey(command);

        scanTimer = new System.Windows.Forms.Timer
        {
            Interval = 1000,
            Enabled = true,
        };
        scanTimer.Tick += (_, _) => ApplyCurrentConfig();

        UpdateMenuState();
        SaveAndApply();
        logger.Info("PipOpacity started.");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            scanTimer.Stop();
            hotkeyWindow.Dispose();
            controller.RestoreAll();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            appIcon.Dispose();
            scanTimer.Dispose();
            logger.Info("PipOpacity exited.");
        }

        base.Dispose(disposing);
    }

    private void HandleHotkey(HotkeyCommand command)
    {
        if (!HasCurrentPictureInPictureWindow())
        {
            return;
        }

        switch (command)
        {
            case HotkeyCommand.IncreaseOpacity:
                SetOpacity(OpacityPolicy.AdjustPercent(config.OpacityPercent, OpacityPolicy.StepPercent));
                break;
            case HotkeyCommand.DecreaseOpacity:
                SetOpacity(OpacityPolicy.AdjustPercent(config.OpacityPercent, -OpacityPolicy.StepPercent));
                break;
            case HotkeyCommand.Reset:
                ResetToNormal();
                break;
            case HotkeyCommand.ToggleClickThrough:
                config.ClickThrough = !config.ClickThrough;
                SaveAndApply();
                break;
            case HotkeyCommand.ToggleAlwaysOnTop:
                config.AlwaysOnTop = !config.AlwaysOnTop;
                SaveAndApply();
                break;
        }
    }

    private void SetOpacity(int opacityPercent)
    {
        var clamped = OpacityPolicy.ClampPercent(opacityPercent);
        if (config.OpacityPercent == clamped)
        {
            return;
        }

        config.OpacityPercent = clamped;
        SaveAndApply();
    }

    private void ResetToNormal()
    {
        config.Enabled = true;
        config.OpacityPercent = OpacityPolicy.MaximumPercent;
        config.ClickThrough = false;
        config.AlwaysOnTop = false;
        controller.RestoreAll();
        SaveAndApply();
    }

    private void ToggleStartup()
    {
        var requested = startupItem.Checked;
        if (!startupRegistration.SetEnabled(requested))
        {
            startupItem.Checked = !requested;
            ShowOneTimeWarning("Could not update Start with Windows. See the log for details.");
            return;
        }

        config.StartWithWindows = requested;
        SaveAndApply();
    }

    private void SaveAndApply()
    {
        config = config.Normalize();
        configStore.Save(config);
        UpdateMenuState();
        ApplyCurrentConfig();
    }

    private void ApplyCurrentConfig()
    {
        try
        {
            _ = controller.Apply(config);
        }
        catch (Exception exception)
        {
            logger.Error("Failed while applying current config.", exception);
        }
    }

    private bool HasCurrentPictureInPictureWindow()
    {
        try
        {
            return controller.Apply(config) > 0;
        }
        catch (Exception exception)
        {
            logger.Error("Failed while checking for current PiP windows.", exception);
            return false;
        }
    }

    private void UpdateMenuState()
    {
        enabledItem.Checked = config.Enabled;
        clickThroughItem.Checked = config.ClickThrough;
        alwaysOnTopItem.Checked = config.AlwaysOnTop;
        startupItem.Checked = config.StartWithWindows;
        opacityLabelItem.Text = $"Opacity: {config.OpacityPercent}%";

        if (opacityTrackBar.Value != config.OpacityPercent)
        {
            opacityTrackBar.Value = config.OpacityPercent;
        }

        notifyIcon.Text = BuildNotifyText();
    }

    private string BuildNotifyText()
    {
        var clickThroughText = config.ClickThrough ? ", click-through" : string.Empty;
        var topMostText = config.AlwaysOnTop ? ", always on top" : ", not topmost";
        return config.Enabled
            ? $"Firefox PiP Opacity: {config.OpacityPercent}%{clickThroughText}{topMostText}"
            : "Firefox PiP Opacity: paused";
    }

    private static int RoundToStep(int value)
    {
        var rounded = (int)Math.Round(value / (double)OpacityPolicy.StepPercent) * OpacityPolicy.StepPercent;
        return OpacityPolicy.ClampPercent(rounded);
    }

    private void ShowOneTimeWarning(string message)
    {
        notifyIcon.BalloonTipTitle = "Firefox PiP Opacity";
        notifyIcon.BalloonTipText = message;
        notifyIcon.ShowBalloonTip(5000);
    }

    private static Icon LoadAppIcon()
    {
        using var extracted = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        return extracted is null
            ? (Icon)SystemIcons.Application.Clone()
            : (Icon)extracted.Clone();
    }
}
