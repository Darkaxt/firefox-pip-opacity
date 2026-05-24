namespace PipOpacity.Core;

public static class OpacityPolicy
{
    public const int MinimumPercent = 0;
    public const int MaximumPercent = 100;
    public const int StepPercent = 5;
    public static readonly int[] PresetPercents = [0, 25, 50, 67, 75, 100];

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
