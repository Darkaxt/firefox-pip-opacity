namespace PipOpacity.Tray;

using System.Diagnostics;
using System.Text;
using PipOpacity.Core;

internal sealed class WindowEnumerator
{
    private readonly FileLogger logger;

    public WindowEnumerator(FileLogger logger)
    {
        this.logger = logger;
    }

    public IReadOnlyList<WindowSnapshot> EnumerateTopLevelWindows()
    {
        var windows = new List<WindowSnapshot>();

        NativeMethods.EnumWindows((handle, _) =>
        {
            try
            {
                windows.Add(CreateSnapshot(handle));
            }
            catch (Exception exception)
            {
                logger.Error($"Failed to inspect window 0x{handle:X}.", exception);
            }

            return true;
        }, 0);

        return windows;
    }

    private static WindowSnapshot CreateSnapshot(nint handle)
    {
        _ = NativeMethods.GetWindowRect(handle, out var rect);
        _ = NativeMethods.GetWindowThreadProcessId(handle, out var processId);

        var snapshot = new WindowSnapshot(
            Handle: handle,
            ProcessPath: string.Empty,
            Title: GetWindowText(handle),
            ClassName: GetClassName(handle),
            IsVisible: NativeMethods.IsWindowVisible(handle),
            Width: rect.Width,
            Height: rect.Height,
            Owner: NativeMethods.GetWindow(handle, NativeMethods.GwOwner));

        return PipWindowMatcher.HasPictureInPictureShape(snapshot)
            ? snapshot with { ProcessPath = GetProcessPath(processId) }
            : snapshot;
    }

    private static string GetWindowText(nint handle)
    {
        var builder = new StringBuilder(capacity: 512);
        _ = NativeMethods.GetWindowText(handle, builder, builder.Capacity);
        return builder.ToString();
    }

    private static string GetClassName(nint handle)
    {
        var builder = new StringBuilder(capacity: 256);
        _ = NativeMethods.GetClassName(handle, builder, builder.Capacity);
        return builder.ToString();
    }

    private static string GetProcessPath(uint processId)
    {
        try
        {
            using var process = Process.GetProcessById(unchecked((int)processId));
            return process.MainModule?.FileName ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
