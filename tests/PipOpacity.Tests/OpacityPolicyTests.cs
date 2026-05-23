namespace PipOpacity.Tests;

using PipOpacity.Core;

public sealed class OpacityPolicyTests
{
    [Theory]
    [InlineData(10, 35)]
    [InlineData(35, 35)]
    [InlineData(67, 67)]
    [InlineData(100, 100)]
    [InlineData(150, 100)]
    public void ClampPercentKeepsOpacityInSupportedRange(int input, int expected)
    {
        Assert.Equal(expected, OpacityPolicy.ClampPercent(input));
    }

    [Theory]
    [InlineData(67, 5, 72)]
    [InlineData(99, 5, 100)]
    [InlineData(36, -5, 35)]
    public void AdjustPercentAppliesDeltaAndClamps(int current, int delta, int expected)
    {
        Assert.Equal(expected, OpacityPolicy.AdjustPercent(current, delta));
    }

    [Fact]
    public void PercentToAlphaMapsPercentToWin32ByteAlpha()
    {
        Assert.Equal(171, OpacityPolicy.PercentToAlpha(67));
        Assert.Equal(255, OpacityPolicy.PercentToAlpha(100));
        Assert.Equal(89, OpacityPolicy.PercentToAlpha(35));
    }
}
