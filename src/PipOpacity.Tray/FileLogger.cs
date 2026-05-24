namespace PipOpacity.Tray;

using System.IO;

internal sealed class FileLogger
{
    private readonly object gate = new();
    private readonly string logPath;

    public FileLogger(string logPath)
    {
        this.logPath = logPath;
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
    }

    public void Info(string message)
    {
        Write("INFO", message);
    }

    public void Error(string message, Exception? exception = null)
    {
        Write("ERROR", exception is null ? message : $"{message} {exception}");
    }

    private void Write(string level, string message)
    {
        var line = $"{DateTimeOffset.Now:O} [{level}] {message}{Environment.NewLine}";

        lock (gate)
        {
            File.AppendAllText(logPath, line);
        }
    }
}
