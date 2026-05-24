namespace PipOpacity.Tray;

using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using PipOpacity.Core;
using WinForms = System.Windows.Forms;

internal sealed class PipOpacityWpfApplication : Application
{
    private readonly FileLogger logger = new(AppPaths.LogPath);
    private readonly ConfigStore configStore;
    private readonly StartupRegistration startupRegistration;
    private readonly WindowOpacityController controller;
    private readonly SettingsViewModel viewModel;
    private readonly DispatcherTimer scanTimer;
    private readonly DispatcherTimer clickThroughBypassTimer;
    private readonly Icon appIcon;
    private readonly WinForms.NotifyIcon notifyIcon;
    private readonly WinForms.ToolStripMenuItem startupItem;
    private readonly object applyGate = new();
    private readonly object controllerGate = new();

    private HotkeyMessageWindow? hotkeyWindow;
    private SettingsWindow? settingsWindow;
    private PipOpacityConfig config;
    private PipOpacityConfig? latestConfigToApply;
    private bool clickThroughBypassActive;
    private bool applyRunning;
    private bool applyPending;
    private int lastMatchedWindowCount;
    private bool shuttingDown;

    public PipOpacityWpfApplication()
    {
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        configStore = new ConfigStore(AppPaths.ConfigPath, logger);
        startupRegistration = new StartupRegistration(logger);
        config = configStore.Load();
        config.StartWithWindows = startupRegistration.IsEnabled();

        controller = new WindowOpacityController(
            new WindowEnumerator(logger),
            logger,
            ShowWarning);

        viewModel = new SettingsViewModel(ResetToNormal);
        viewModel.UserChanged += (_, _) => ApplyViewModelSettings();

        startupItem = new WinForms.ToolStripMenuItem("Start with Windows")
        {
            CheckOnClick = true,
        };
        startupItem.Click += (_, _) => ToggleStartupFromMenu();

        appIcon = LoadAppIcon();
        notifyIcon = new WinForms.NotifyIcon
        {
            ContextMenuStrip = BuildTrayMenu(),
            Icon = appIcon,
            Text = "Firefox PiP Opacity",
            Visible = true,
        };
        notifyIcon.MouseClick += (_, args) =>
        {
            if (args.Button == WinForms.MouseButtons.Left)
            {
                ShowSettingsWindow();
            }
        };
        notifyIcon.DoubleClick += (_, _) => ShowSettingsWindow();

        scanTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        scanTimer.Tick += (_, _) => QueueApplyCurrentConfig();

        clickThroughBypassTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50),
        };
        clickThroughBypassTimer.Tick += (_, _) => UpdateClickThroughBypassState();

        UpdateUiState();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        hotkeyWindow = new HotkeyMessageWindow(logger);
        hotkeyWindow.HotkeyPressed += (_, command) => HandleHotkey(command);

        scanTimer.Start();
        clickThroughBypassTimer.Start();
        SaveAndApply();
        logger.Info("PipOpacity started.");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        shuttingDown = true;
        scanTimer.Stop();
        clickThroughBypassTimer.Stop();
        hotkeyWindow?.Dispose();
        lock (controllerGate)
        {
            controller.RestoreAll();
        }

        if (settingsWindow is not null)
        {
            settingsWindow.Closing -= HideSettingsWindowOnClose;
            settingsWindow.Close();
        }

        notifyIcon.Visible = false;
        notifyIcon.Dispose();
        appIcon.Dispose();
        logger.Info("PipOpacity exited.");

        base.OnExit(e);
    }

    private WinForms.ContextMenuStrip BuildTrayMenu()
    {
        var menu = new WinForms.ContextMenuStrip();
        menu.Items.Add(new WinForms.ToolStripMenuItem("Open", null, (_, _) => ShowSettingsWindow()));
        menu.Items.Add(new WinForms.ToolStripMenuItem("Reset PiP windows to normal", null, (_, _) => ResetToNormal()));
        menu.Items.Add(startupItem);
        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add(new WinForms.ToolStripMenuItem("Exit", null, (_, _) => Shutdown()));
        return menu;
    }

    private void ShowSettingsWindow()
    {
        if (settingsWindow is null)
        {
            settingsWindow = new SettingsWindow
            {
                DataContext = viewModel,
            };
            settingsWindow.Closing += HideSettingsWindowOnClose;
        }

        if (!settingsWindow.IsVisible)
        {
            settingsWindow.Show();
        }

        if (settingsWindow.WindowState == WindowState.Minimized)
        {
            settingsWindow.WindowState = WindowState.Normal;
        }

        settingsWindow.Activate();
    }

    private void HideSettingsWindowOnClose(object? sender, CancelEventArgs args)
    {
        if (shuttingDown)
        {
            return;
        }

        args.Cancel = true;
        settingsWindow?.Hide();
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
        }
    }

    private void ApplyViewModelSettings()
    {
        var wasEnabled = config.Enabled;
        var requestedStartWithWindows = viewModel.StartWithWindows;

        config.Enabled = viewModel.Enabled;
        config.OpacityPercent = (int)Math.Round(viewModel.OpacityPercent);
        config.ClickThrough = viewModel.ClickThrough;

        if (!config.Enabled && wasEnabled)
        {
            lastMatchedWindowCount = 0;
        }

        if (requestedStartWithWindows != config.StartWithWindows)
        {
            if (!startupRegistration.SetEnabled(requestedStartWithWindows))
            {
                ShowWarning("Could not update Start with Windows. See the log for details.");
                UpdateUiState();
                return;
            }

            config.StartWithWindows = requestedStartWithWindows;
        }

        SaveAndApply();
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
        SaveAndApply();
    }

    private void ToggleStartupFromMenu()
    {
        var requested = startupItem.Checked;
        if (!startupRegistration.SetEnabled(requested))
        {
            startupItem.Checked = !requested;
            ShowWarning("Could not update Start with Windows. See the log for details.");
            return;
        }

        config.StartWithWindows = requested;
        SaveAndApply();
    }

    private void SaveAndApply()
    {
        config = config.Normalize();
        configStore.Save(config);
        UpdateUiState();
        QueueApplyCurrentConfig();
    }

    private void QueueApplyCurrentConfig()
    {
        lock (applyGate)
        {
            latestConfigToApply = BuildEffectiveConfig();
            if (applyRunning)
            {
                applyPending = true;
                return;
            }

            applyRunning = true;
        }

        _ = Task.Run(ProcessApplyQueue);
    }

    private void ProcessApplyQueue()
    {
        while (true)
        {
            PipOpacityConfig configToApply;
            lock (applyGate)
            {
                configToApply = latestConfigToApply ?? BuildEffectiveConfig();
                applyPending = false;
            }

            try
            {
                int matchedCount;
                lock (controllerGate)
                {
                    matchedCount = controller.Apply(configToApply);
                }

                Volatile.Write(ref lastMatchedWindowCount, matchedCount);
            }
            catch (Exception exception)
            {
                logger.Error("Failed while applying current config.", exception);
            }

            lock (applyGate)
            {
                if (!applyPending)
                {
                    applyRunning = false;
                    return;
                }
            }
        }
    }

    private bool HasCurrentPictureInPictureWindow()
    {
        return Volatile.Read(ref lastMatchedWindowCount) > 0;
    }

    private void UpdateClickThroughBypassState()
    {
        var bypassActive = config.ClickThrough && IsControlKeyDown();
        if (clickThroughBypassActive == bypassActive)
        {
            return;
        }

        clickThroughBypassActive = bypassActive;
        QueueApplyCurrentConfig();
    }

    private PipOpacityConfig BuildEffectiveConfig()
    {
        var normalized = config.Normalize();
        return new PipOpacityConfig
        {
            Enabled = normalized.Enabled,
            OpacityPercent = normalized.OpacityPercent,
            ClickThrough = ClickThroughPolicy.GetEffectiveClickThrough(normalized.ClickThrough, clickThroughBypassActive),
            StartWithWindows = normalized.StartWithWindows,
        };
    }

    private void UpdateUiState()
    {
        startupItem.Checked = config.StartWithWindows;
        notifyIcon.Text = PipOpacityStatusFormatter.Format(config);
        viewModel.LoadFromConfig(config);
    }

    private void ShowWarning(string message)
    {
        if (!Dispatcher.CheckAccess())
        {
            _ = Dispatcher.BeginInvoke(() => ShowWarning(message));
            return;
        }

        notifyIcon.BalloonTipTitle = "Firefox PiP Opacity";
        notifyIcon.BalloonTipText = message;
        notifyIcon.ShowBalloonTip(5000);
    }

    private static bool IsControlKeyDown()
    {
        return IsKeyDown(NativeMethods.VkControl)
            || IsKeyDown(NativeMethods.VkLControl)
            || IsKeyDown(NativeMethods.VkRControl);
    }

    private static bool IsKeyDown(int virtualKey)
    {
        return (NativeMethods.GetAsyncKeyState(virtualKey) & 0x8000) != 0;
    }

    private static Icon LoadAppIcon()
    {
        using var extracted = Icon.ExtractAssociatedIcon(WinForms.Application.ExecutablePath);
        return extracted is null
            ? (Icon)SystemIcons.Application.Clone()
            : (Icon)extracted.Clone();
    }
}
