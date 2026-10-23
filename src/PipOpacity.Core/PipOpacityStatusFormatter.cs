namespace PipOpacity.Core;

public static class PipOpacityStatusFormatter
{
    public static string Format(PipOpacityConfig config)
    {
        if (!config.Enabled)
        {
            return "PiP Opacity: paused";
        }

        var clickThroughText = config.ClickThrough ? ", click-through" : string.Empty;
        return $"PiP Opacity: {config.OpacityPercent}%{clickThroughText}";
    }
}
