namespace Notes.Core.Services.Navigation;

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
                await Shell.Current.GoToAsync(route, true, parameters);
            else
                await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Navigation error to route: {Route}", route);
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
            _logger.LogError(ex, "Navigation eror going back");
            throw;
        }
    }

    public Task GoToRootAsync()
    {
        try
        {
            return Shell.Current.GoToAsync("///");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Navigation error going to root");
            throw;
        }
    }
}
