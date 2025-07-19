namespace Notes.Maui.Services.Navigation;

public class NavigationService : INavigationService
{
    private readonly ILogger<NavigationService> _logger;

    public NavigationService(ILogger<NavigationService> logger)
    {
        _logger = logger;
    }

    public async Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        try
        {
            if (parameters != null)
            {
                await Shell.Current.GoToAsync(route, parameters);
            }
            else
            {
                await Shell.Current.GoToAsync(route);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to {Route}", route);
            throw;
        }
    }

    public async Task GoBackAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error going back");
            throw;
        }
    }

    public async Task GoToRootAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("//");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error going to root");
            throw;
        }
    }
} 