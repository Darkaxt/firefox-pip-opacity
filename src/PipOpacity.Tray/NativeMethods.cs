namespace PipOpacity.Tray;

using System.Runtime.InteropServices;
using System.Text;

internal static partial class NativeMethods
{
    public const int GwlExStyle = -20;
    public const int GwOwner = 4;
    public const int WsExLayered = 0x00080000;
    public const int WsExTransparent = 0x00000020;
    public const int LwaAlpha = 0x00000002;
    public const int WmHotkey = 0x0312;
    public const uint ModAlt = 0x0001;
    public const uint ModControl = 0x0002;

    public delegate bool EnumWindowsProc(nint hWnd, nint lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    [DllImport("user32.dll")]
    public static extern bool IsWindow(nint hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(nint hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int GetClassName(nint hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(nint hWnd, out Rect rect);

    [DllImport("user32.dll")]
    public static extern nint GetWindow(nint hWnd, uint uCmd);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern nint GetWindowLongPtr64(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
    private static extern nint GetWindowLongPtr32(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern nint SetWindowLongPtr64(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern nint SetWindowLongPtr32(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetLayeredWindowAttributes(nint hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnregisterHotKey(nint hWnd, int id);

    [DllImport("kernel32.dll")]
    private static extern void SetLastError(uint dwErrCode);

    public static int GetWindowExStyle(nint hWnd)
    {
        var result = nint.Size == 8
            ? GetWindowLongPtr64(hWnd, GwlExStyle)
            : GetWindowLongPtr32(hWnd, GwlExStyle);

        return unchecked((int)result);
    }

    public static bool SetWindowExStyle(nint hWnd, int exStyle)
    {
        SetLastError(0);

        var previous = nint.Size == 8
            ? SetWindowLongPtr64(hWnd, GwlExStyle, exStyle)
            : SetWindowLongPtr32(hWnd, GwlExStyle, exStyle);

        return previous != 0 || Marshal.GetLastWin32Error() == 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public readonly int Width => Right - Left;

        public readonly int Height => Bottom - Top;
    }
}
