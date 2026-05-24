namespace PipOpacity.Tests;

using PipOpacity.Core;

public sealed class WindowStateStoreTests
{
    [Fact]
    public void RememberStoresOriginalStateOnlyOnce()
    {
        var store = new WindowStateStore();

        store.RememberOriginal(0x1234, new WindowOriginalState(ExtendedStyle: 0x100));
        store.RememberOriginal(0x1234, new WindowOriginalState(ExtendedStyle: 0x90000));

        Assert.True(store.TryGetOriginal(0x1234, out var originalState));
        Assert.Equal(0x100, originalState.ExtendedStyle);
    }

    [Fact]
    public void SnapshotReturnsStoredOriginalState()
    {
        var store = new WindowStateStore();

        store.RememberOriginal(0x1234, new WindowOriginalState(ExtendedStyle: 0x100));

        var snapshot = store.Snapshot();

        Assert.Single(snapshot);
        Assert.Equal(0x100, snapshot[0x1234].ExtendedStyle);
    }

    [Fact]
    public void ForgetRemovesStoredState()
    {
        var store = new WindowStateStore();

        store.RememberOriginal(0x1234, new WindowOriginalState(ExtendedStyle: 0x100));
        store.Forget(0x1234);

        Assert.False(store.TryGetOriginal(0x1234, out _));
    }
}
