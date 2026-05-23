namespace PipOpacity.Tests;

using PipOpacity.Core;

public sealed class AppConfigTests
{
    [Fact]
    public void DefaultsMatchPlannedTrayBehavior()
    {
        var config = PipOpacityConfig.CreateDefault();

        Assert.True(config.Enabled);
        Assert.Equal(67, config.OpacityPercent);
        Assert.False(config.ClickThrough);
        Assert.False(config.AlwaysOnTop);
        Assert.False(config.StartWithWindows);
    }

    [Fact]
    public void NormalizedConfigClampsOpacity()
    {
        var config = new PipOpacityConfig
        {
            Enabled = true,
            OpacityPercent = 5,
            ClickThrough = true,
            AlwaysOnTop = true,
            StartWithWindows = true,
        };

        var normalized = config.Normalize();

        Assert.Equal(35, normalized.OpacityPercent);
        Assert.True(normalized.ClickThrough);
        Assert.True(normalized.AlwaysOnTop);
        Assert.True(normalized.StartWithWindows);
    }
}
