namespace Notes.Maui.ViewModels.App;

public partial class AboutViewModel : BaseViewModel
{
    public AboutViewModel(INavigationService navigationService, ILogger<AboutViewModel> logger)
        : base(navigationService, logger)
    {
        Title = $"About {AppInfo.Name}";
    }

    [RelayCommand]
    async Task OpenWebsite()
    {
        try
        {
            await Launcher.OpenAsync("https://dotnet.microsoft.com/apps/maui");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening website");
        }
    }
} 