namespace PipOpacity.Tray;

internal static class AppPaths
{
    public static string ConfigPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PipOpacity",
            "config.json");

    public static string LogPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PipOpacity",
            "logs",
            "pip-opacity.log");
}
