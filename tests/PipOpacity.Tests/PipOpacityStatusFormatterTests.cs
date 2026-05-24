namespace PipOpacity.Tests;

using PipOpacity.Core;

public sealed class PipOpacityStatusFormatterTests
{
    [Fact]
    public void FormatShowsPausedWhenOpacityControlIsDisabled()
    {
        var config = PipOpacityConfig.CreateDefault();
        config.Enabled = false;

        Assert.Equal("Firefox PiP Opacity: paused", PipOpacityStatusFormatter.Format(config));
    }

    [Fact]
    public void FormatIncludesOpacityAndClickThroughWhenEnabled()
    {
        var config = PipOpacityConfig.CreateDefault();
        config.OpacityPercent = 25;
        config.ClickThrough = true;

        Assert.Equal("Firefox PiP Opacity: 25%, click-through", PipOpacityStatusFormatter.Format(config));
    }
}
