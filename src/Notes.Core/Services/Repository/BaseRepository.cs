namespace Notes.Core.Services.Repository;

public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ILogger<BaseRepository<T>> Logger;

    protected BaseRepository(ILogger<BaseRepository<T>> logger)
    {
        Logger = logger;
    }

    // Common logging helper methods
    protected void LogDebug(string message, params object[] args) => Logger.LogDebug(message, args);
    protected void LogInformation(string message, params object[] args) => Logger.LogInformation(message, args);
    protected void LogWarning(string message, params object[] args) => Logger.LogWarning(message, args);
    protected void LogError(Exception ex, string message, params object[] args) => Logger.LogError(ex, message, args);

    // Abstract methods that concrete implementations must provide
    public abstract Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    public abstract Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    public abstract Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    public abstract Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    public abstract Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    // Common utility methods that can be shared across implementations
    protected virtual string GenerateUniqueId() => Guid.NewGuid().ToString();
    
    protected virtual void EnsureEntityId(T entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = GenerateUniqueId();
        }
    }

    protected virtual void UpdateTimestamp(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
