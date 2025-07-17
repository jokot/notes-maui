namespace Notes.Core.Services.Data;

public class LocalDataService : IDataService
{
    private readonly ILogger<LocalDataService> _logger;

    public LocalDataService(ILogger<LocalDataService> logger)
    {
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var json = await SecureStorage.Default.GetAsync(key);
            return json != null ? JsonSerializer.Deserialize<T>(json) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data fro key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await SecureStorage.Default.SetAsync(key, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting data for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            SecureStorage.Default.Remove(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing data for key: {Key}", key);
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            SecureStorage.Default.RemoveAll();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all data");
        }
    }
}
