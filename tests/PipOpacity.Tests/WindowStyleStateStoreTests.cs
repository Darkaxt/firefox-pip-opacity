namespace PipOpacity.Tests;

using PipOpacity.Core;

public sealed class WindowStyleStateStoreTests
{
    [Fact]
    public void RememberStoresOriginalStyleOnlyOnce()
    {
        var store = new WindowStyleStateStore();

        store.RememberOriginal(0x1234, 0x100);
        store.RememberOriginal(0x1234, 0x90000);

        Assert.True(store.TryGetOriginal(0x1234, out var originalStyle));
        Assert.Equal(0x100, originalStyle);
    }

    [Fact]
    public void ForgetRemovesStoredStyle()
    {
        var store = new WindowStyleStateStore();

        store.RememberOriginal(0x1234, 0x100);
        store.Forget(0x1234);

        Assert.False(store.TryGetOriginal(0x1234, out _));
    }
}
