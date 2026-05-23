namespace PipOpacity.Tray;

using Microsoft.Win32;

internal sealed class StartupRegistration
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "PipOpacity";

    private readonly FileLogger logger;

    public StartupRegistration(FileLogger logger)
    {
        this.logger = logger;
    }

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        var value = key?.GetValue(ValueName) as string;
        return string.Equals(Unquote(value), Application.ExecutablePath, StringComparison.OrdinalIgnoreCase);
    }

    public bool SetEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
            if (key is null)
            {
                return false;
            }

            if (enabled)
            {
                key.SetValue(ValueName, Quote(Application.ExecutablePath), RegistryValueKind.String);
            }
            else
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
            }

            return true;
        }
        catch (Exception exception)
        {
            logger.Error("Failed to update startup registration.", exception);
            return false;
        }
    }

    private static string Quote(string value)
    {
        return value.Contains(' ') ? $"\"{value}\"" : value;
    }

    private static string? Unquote(string? value)
    {
        return value?.Trim().Trim('"');
    }
}
