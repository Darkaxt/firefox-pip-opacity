namespace PipOpacity.Core;

public static class PipOpacityStatusFormatter
{
    public static string Format(PipOpacityConfig config)
    {
        if (!config.Enabled)
        {
            return "Firefox PiP Opacity: paused";
        }

        var clickThroughText = config.ClickThrough ? ", click-through" : string.Empty;
        return $"Firefox PiP Opacity: {config.OpacityPercent}%{clickThroughText}";
    }
}
