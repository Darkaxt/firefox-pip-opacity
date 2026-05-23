namespace PipOpacity.Core;

public sealed record WindowSnapshot(
    nint Handle,
    string ProcessPath,
    string Title,
    string ClassName,
    bool IsVisible,
    int Width,
    int Height,
    nint Owner);
