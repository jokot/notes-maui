namespace Notes.Core.Services.Data;

public interface IDatabaseInitializationService
{
    Task InitializeDatabaseAsync();
}

public class DatabaseInitializationService : IDatabaseInitializationService
{
    private readonly NotesDbContext _context;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(NotesDbContext context, ILogger<DatabaseInitializationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Initializing Notes database...");
            
            // Ensure the database is created
            await _context.Database.EnsureCreatedAsync();
            
            _logger.LogInformation("Notes database initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Notes database");
            throw;
        }
    }
}
