namespace PipOpacity.Core;

public static class ClickThroughPolicy
{
    public static bool GetEffectiveClickThrough(bool configuredClickThrough, bool bypassModifierDown)
    {
        return configuredClickThrough && !bypassModifierDown;
    }
}
