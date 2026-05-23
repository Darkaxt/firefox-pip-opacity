namespace PipOpacity.Tray;

static class Program
{
    [STAThread]
    static void Main()
    {
        using var mutex = new Mutex(initiallyOwned: true, "PipOpacity.Tray.SingleInstance", out var ownsMutex);
        if (!ownsMutex)
        {
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new PipOpacityApplicationContext());
    }
}
