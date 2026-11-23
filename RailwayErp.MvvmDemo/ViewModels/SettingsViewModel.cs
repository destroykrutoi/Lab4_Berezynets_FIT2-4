using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RailwayErp.MvvmDemo.Services;

namespace RailwayErp.MvvmDemo.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _theme;
    private readonly IAppStateService _state;

    public SettingsViewModel(IThemeService theme, IAppStateService state)
    {
        _theme = theme;
        _state = state;
        UpdateText();
    }

    [ObservableProperty]
    private string _currentThemeText = string.Empty;

    public string SessionInfo =>
        $"Сесію розпочато о {_state.SessionStartedAt:HH:mm:ss}, повернень із фону: {_state.ResumeCount}";

    [RelayCommand]
    private void SetTheme(string name)
    {
        if (Enum.TryParse(name, out AppTheme theme))
        {
            _theme.SetTheme(theme);
            UpdateText();
        }
    }

    public void Refresh()
    {
        UpdateText();
        OnPropertyChanged(nameof(SessionInfo));
    }

    private void UpdateText() =>
        CurrentThemeText = _theme.CurrentTheme switch
        {
            AppTheme.Light => "Поточна тема: світла",
            AppTheme.Dark => "Поточна тема: темна",
            _ => "Поточна тема: як у системі"
        };
}
