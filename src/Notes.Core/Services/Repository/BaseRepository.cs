namespace Notes.Core.Services.Repository;

public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly IFileDataService FileDataService;
    protected readonly ILogger<BaseRepository<T>> Logger;

    protected BaseRepository(IFileDataService fileDataService, ILogger<BaseRepository<T>> logger)
    {
        FileDataService = fileDataService;
        Logger = logger;
    }

    public abstract Task<IEnumerable<T>> GetAllAsync();
    public abstract Task<IEnumerable<T>> GetAllForceAsync();
    public abstract Task<T?> GetByIdAsync(string id);
    public abstract Task<T> AddAsync(T entity);
    public abstract Task<T> UpdateAsync(T entity);
    public abstract Task DeleteAsync(string id);
    public abstract Task<bool> ExitsAsync(string id);
}
