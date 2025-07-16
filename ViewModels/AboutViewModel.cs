namespace Notes.ViewModels;

public partial class AboutViewModel : BaseViewModel
{
    public string Version => AppInfo.VersionString;
    public string MoreInfoUrl => "https://aka.ms/maui";
    public string Message => "This app is written in XAML and C# with .NET MAUI.";
    public AboutViewModel()
    {
        Title = "About";
    }

    [RelayCommand]
    async Task ShowMoreInfo()
    {
        await Launcher.OpenAsync(MoreInfoUrl);
    }
}