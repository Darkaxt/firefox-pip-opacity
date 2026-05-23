namespace PipOpacity.Core;

public enum HotkeyCommand
{
    None = 0,
    IncreaseOpacity,
    DecreaseOpacity,
    Reset,
    ToggleClickThrough,
    ToggleAlwaysOnTop,
}

public static class HotkeyIds
{
    public const int IncreaseOpacity = 101;
    public const int DecreaseOpacity = 102;
    public const int Reset = 103;
    public const int ToggleClickThrough = 104;
    public const int IncreaseOpacityNumpad = 105;
    public const int DecreaseOpacityNumpad = 106;
    public const int ToggleAlwaysOnTop = 107;
}

public static class HotkeyCommandMapper
{
    public static HotkeyCommand FromId(int id)
    {
        return id switch
        {
            HotkeyIds.IncreaseOpacity or HotkeyIds.IncreaseOpacityNumpad => HotkeyCommand.IncreaseOpacity,
            HotkeyIds.DecreaseOpacity or HotkeyIds.DecreaseOpacityNumpad => HotkeyCommand.DecreaseOpacity,
            HotkeyIds.Reset => HotkeyCommand.Reset,
            HotkeyIds.ToggleClickThrough => HotkeyCommand.ToggleClickThrough,
            HotkeyIds.ToggleAlwaysOnTop => HotkeyCommand.ToggleAlwaysOnTop,
            _ => HotkeyCommand.None,
        };
    }
}
