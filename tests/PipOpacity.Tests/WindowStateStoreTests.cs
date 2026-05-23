namespace PipOpacity.Tests;

using PipOpacity.Core;

public sealed class WindowStateStoreTests
{
    [Fact]
    public void RememberStoresOriginalStateOnlyOnce()
    {
        var store = new WindowStateStore();

        store.RememberOriginal(0x1234, new WindowOriginalState(ExtendedStyle: 0x100, WasTopMost: true));
        store.RememberOriginal(0x1234, new WindowOriginalState(ExtendedStyle: 0x90000, WasTopMost: false));

        Assert.True(store.TryGetOriginal(0x1234, out var originalState));
        Assert.Equal(0x100, originalState.ExtendedStyle);
        Assert.True(originalState.WasTopMost);
    }

    [Fact]
    public void SnapshotReturnsStoredOriginalState()
    {
        var store = new WindowStateStore();

        store.RememberOriginal(0x1234, new WindowOriginalState(ExtendedStyle: 0x100, WasTopMost: false));

        var snapshot = store.Snapshot();

        Assert.Single(snapshot);
        Assert.Equal(0x100, snapshot[0x1234].ExtendedStyle);
        Assert.False(snapshot[0x1234].WasTopMost);
    }

    [Fact]
    public void ForgetRemovesStoredState()
    {
        var store = new WindowStateStore();

        store.RememberOriginal(0x1234, new WindowOriginalState(ExtendedStyle: 0x100, WasTopMost: true));
        store.Forget(0x1234);

        Assert.False(store.TryGetOriginal(0x1234, out _));
    }
}
