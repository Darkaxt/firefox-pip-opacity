namespace PipOpacity.Tray;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PipOpacity.Core;

internal sealed class SettingsViewModel : INotifyPropertyChanged
{
    private bool enabled;
    private double opacityPercent;
    private bool clickThrough;
    private bool startWithWindows;
    private string statusText = string.Empty;
    private bool loading;

    public SettingsViewModel(Action reset)
    {
        ResetCommand = new RelayCommand(reset);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? UserChanged;

    public bool Enabled
    {
        get => enabled;
        set => SetField(ref enabled, value);
    }

    public double OpacityPercent
    {
        get => opacityPercent;
        set => SetField(ref opacityPercent, OpacityPolicy.ClampPercent((int)Math.Round(value)));
    }

    public bool ClickThrough
    {
        get => clickThrough;
        set => SetField(ref clickThrough, value);
    }

    public bool StartWithWindows
    {
        get => startWithWindows;
        set => SetField(ref startWithWindows, value);
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value, notifyUser: false);
    }

    public ICommand ResetCommand { get; }

    public void LoadFromConfig(PipOpacityConfig config)
    {
        loading = true;
        Enabled = config.Enabled;
        OpacityPercent = config.OpacityPercent;
        ClickThrough = config.ClickThrough;
        StartWithWindows = config.StartWithWindows;
        StatusText = PipOpacityStatusFormatter.Format(config);
        loading = false;
    }

    private void SetField<T>(ref T field, T value, bool notifyUser = true, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        if (notifyUser && !loading)
        {
            UserChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
