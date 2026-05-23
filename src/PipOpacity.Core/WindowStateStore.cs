namespace PipOpacity.Core;

public sealed class WindowStateStore
{
    private readonly Dictionary<nint, WindowOriginalState> originalStates = [];

    public void RememberOriginal(nint handle, WindowOriginalState originalState)
    {
        originalStates.TryAdd(handle, originalState);
    }

    public bool TryGetOriginal(nint handle, out WindowOriginalState originalState)
    {
        return originalStates.TryGetValue(handle, out originalState!);
    }

    public IReadOnlyDictionary<nint, WindowOriginalState> Snapshot()
    {
        return new Dictionary<nint, WindowOriginalState>(originalStates);
    }

    public void Forget(nint handle)
    {
        originalStates.Remove(handle);
    }

    public void Clear()
    {
        originalStates.Clear();
    }
}
