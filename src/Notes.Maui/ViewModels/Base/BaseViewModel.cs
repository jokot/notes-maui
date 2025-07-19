namespace Notes.Maui.ViewModels.Base;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool isBusy;

    [ObservableProperty]
    string title = string.Empty;

    public bool IsNotBusy => !IsBusy;

    protected readonly INavigationService NavigationService;
    protected readonly ILogger<BaseViewModel> Logger;

    public BaseViewModel(INavigationService navigationService, ILogger<BaseViewModel> logger)
    {
        NavigationService = navigationService;
        Logger = logger;
    }

    protected virtual async Task ExecuteAsync(Func<Task> operation, string operationName = "")
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await operation();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during {OperationName}", operationName);
            await HandleErrorAsync(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected virtual async Task HandleErrorAsync(Exception ex)
    {
        var message = ex switch
        {
            HttpRequestException => "Network error. Please try again later.",
            UnauthorizedAccessException => "Authentication required",
            _ => "An unexpected error occured."
        };

        // Log the error instead of showing UI (UI handling will be done in MAUI layer)
        Logger.LogError("UI Error: {Message}", message);
    }
}
