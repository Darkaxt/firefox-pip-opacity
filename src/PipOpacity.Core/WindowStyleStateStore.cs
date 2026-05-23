namespace PipOpacity.Core;

public sealed class WindowStyleStateStore
{
    private readonly Dictionary<nint, int> originalStyles = [];

    public void RememberOriginal(nint handle, int originalStyle)
    {
        originalStyles.TryAdd(handle, originalStyle);
    }

    public bool TryGetOriginal(nint handle, out int originalStyle)
    {
        return originalStyles.TryGetValue(handle, out originalStyle);
    }

    public IReadOnlyDictionary<nint, int> Snapshot()
    {
        return new Dictionary<nint, int>(originalStyles);
    }

    public void Forget(nint handle)
    {
        originalStyles.Remove(handle);
    }

    public void Clear()
    {
        originalStyles.Clear();
    }
}
