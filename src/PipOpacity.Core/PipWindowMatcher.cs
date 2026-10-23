namespace PipOpacity.Core;

public static class PipWindowMatcher
{
    private static readonly HashSet<string> ChromiumExecutables = new(StringComparer.OrdinalIgnoreCase)
    {
        "chrome.exe", "helium.exe", "msedge.exe", "brave.exe", "opera.exe", "vivaldi.exe", "chromium.exe",
    };

    public static bool IsBrowserPictureInPicture(WindowSnapshot window)
    {
        if (!HasPictureInPictureShape(window) || string.IsNullOrWhiteSpace(window.ProcessPath))
        {
            return false;
        }

        var executable = Path.GetFileName(window.ProcessPath);
        return HasFirefoxSignature(window)
            ? string.Equals(executable, "firefox.exe", StringComparison.OrdinalIgnoreCase)
            : ChromiumExecutables.Contains(executable);
    }

    public static bool IsFirefoxPictureInPicture(WindowSnapshot window)
    {
        return HasPictureInPictureShape(window)
            && HasFirefoxSignature(window)
            && IsFirefoxProcessPath(window.ProcessPath);
    }

    public static bool HasPictureInPictureShape(WindowSnapshot window)
    {
        return window.IsVisible
            && window.Width > 0
            && window.Height > 0
            && window.Owner == 0
            && (HasFirefoxSignature(window)
                || (string.Equals(window.Title, "Picture in picture", StringComparison.Ordinal)
                    && string.Equals(window.ClassName, "Chrome_WidgetWin_1", StringComparison.Ordinal)));
    }

    private static bool HasFirefoxSignature(WindowSnapshot window)
    {
        return string.Equals(window.Title, "Picture-in-Picture", StringComparison.Ordinal)
            && string.Equals(window.ClassName, "MozillaDialogClass", StringComparison.Ordinal);
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
