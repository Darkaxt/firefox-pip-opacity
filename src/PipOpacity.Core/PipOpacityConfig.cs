namespace PipOpacity.Core;

public sealed class PipOpacityConfig
{
    public bool Enabled { get; set; }

    public int OpacityPercent { get; set; }

    public bool ClickThrough { get; set; }

    public bool AlwaysOnTop { get; set; }

    public bool StartWithWindows { get; set; }

    public static PipOpacityConfig CreateDefault()
    {
        return new PipOpacityConfig
        {
            Enabled = true,
            OpacityPercent = 67,
            ClickThrough = false,
            AlwaysOnTop = false,
            StartWithWindows = false,
        };
    }

    public PipOpacityConfig Normalize()
    {
        return new PipOpacityConfig
        {
            Enabled = Enabled,
            OpacityPercent = OpacityPolicy.ClampPercent(OpacityPercent),
            ClickThrough = ClickThrough,
            AlwaysOnTop = AlwaysOnTop,
            StartWithWindows = StartWithWindows,
        };
    }
}
