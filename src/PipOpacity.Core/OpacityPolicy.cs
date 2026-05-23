namespace PipOpacity.Core;

public static class OpacityPolicy
{
    public const int MinimumPercent = 35;
    public const int MaximumPercent = 100;
    public const int StepPercent = 5;

    public static int ClampPercent(int percent)
    {
        return Math.Clamp(percent, MinimumPercent, MaximumPercent);
    }

    public static int AdjustPercent(int currentPercent, int deltaPercent)
    {
        return ClampPercent(currentPercent + deltaPercent);
    }

    public static byte PercentToAlpha(int percent)
    {
        var clamped = ClampPercent(percent);
        return (byte)Math.Round(clamped / 100d * byte.MaxValue, MidpointRounding.AwayFromZero);
    }
}
