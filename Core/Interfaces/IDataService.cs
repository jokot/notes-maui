using System;

namespace Notes.Core.Interfaces;

public interface IDataService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value) where T : class;
    Task RemoveAsync(string key);
    Task ClearAsync();
}
