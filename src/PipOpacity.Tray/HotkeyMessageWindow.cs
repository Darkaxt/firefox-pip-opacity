namespace PipOpacity.Tray;

using PipOpacity.Core;

internal sealed class HotkeyMessageWindow : NativeWindow, IDisposable
{
    private readonly FileLogger logger;
    private bool disposed;

    public HotkeyMessageWindow(FileLogger logger)
    {
        this.logger = logger;
        CreateHandle(new CreateParams());
        RegisterHotkeys();
    }

    public event EventHandler<HotkeyCommand>? HotkeyPressed;

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == NativeMethods.WmHotkey)
        {
            var command = HotkeyCommandMapper.FromId(message.WParam.ToInt32());
            if (command != HotkeyCommand.None)
            {
                HotkeyPressed?.Invoke(this, command);
            }
        }

        base.WndProc(ref message);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        UnregisterHotkeys();
        DestroyHandle();
        GC.SuppressFinalize(this);
    }

    private void RegisterHotkeys()
    {
        Register(HotkeyIds.IncreaseOpacity, Keys.Oemplus);
        Register(HotkeyIds.IncreaseOpacityNumpad, Keys.Add);
        Register(HotkeyIds.DecreaseOpacity, Keys.OemMinus);
        Register(HotkeyIds.DecreaseOpacityNumpad, Keys.Subtract);
        Register(HotkeyIds.Reset, Keys.D0);
        Register(HotkeyIds.ToggleClickThrough, Keys.T);
        Register(HotkeyIds.ToggleAlwaysOnTop, Keys.A);
    }

    private void Register(int id, Keys key)
    {
        if (!NativeMethods.RegisterHotKey(Handle, id, NativeMethods.ModControl | NativeMethods.ModAlt, (uint)key))
        {
            logger.Error($"Failed to register Ctrl+Alt+{key} hotkey. Error={System.Runtime.InteropServices.Marshal.GetLastWin32Error()}");
        }
    }

    private void UnregisterHotkeys()
    {
        foreach (var id in new[]
        {
            HotkeyIds.IncreaseOpacity,
            HotkeyIds.IncreaseOpacityNumpad,
            HotkeyIds.DecreaseOpacity,
            HotkeyIds.DecreaseOpacityNumpad,
            HotkeyIds.Reset,
            HotkeyIds.ToggleClickThrough,
            HotkeyIds.ToggleAlwaysOnTop,
        })
        {
            _ = NativeMethods.UnregisterHotKey(Handle, id);
        }
    }
}
