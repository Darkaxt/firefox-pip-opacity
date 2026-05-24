namespace PipOpacity.Tests;

using PipOpacity.Core;

public sealed class ClickThroughPolicyTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void EffectiveClickThroughIsDisabledWhileBypassModifierIsPressed(
        bool configuredClickThrough,
        bool bypassModifierDown,
        bool expectedClickThrough)
    {
        Assert.Equal(
            expectedClickThrough,
            ClickThroughPolicy.GetEffectiveClickThrough(configuredClickThrough, bypassModifierDown));
    }
}
