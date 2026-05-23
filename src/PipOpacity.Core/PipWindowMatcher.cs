namespace PipOpacity.Core;

public static class PipWindowMatcher
{
    public static bool IsFirefoxPictureInPicture(WindowSnapshot window)
    {
        return window.IsVisible
            && window.Width > 0
            && window.Height > 0
            && window.Owner == 0
            && string.Equals(window.Title, "Picture-in-Picture", StringComparison.Ordinal)
            && string.Equals(window.ClassName, "MozillaDialogClass", StringComparison.Ordinal)
            && IsFirefoxProcessPath(window.ProcessPath);
    }

    private static bool IsFirefoxProcessPath(string processPath)
    {
        if (string.IsNullOrWhiteSpace(processPath))
        {
            return false;
        }

        return string.Equals(Path.GetFileName(processPath), "firefox.exe", StringComparison.OrdinalIgnoreCase);
    }
}
