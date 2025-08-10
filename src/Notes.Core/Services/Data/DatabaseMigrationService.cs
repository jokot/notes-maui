namespace Notes.Core.Services.Data;

public interface IDatabaseMigrationService
{
    Task<int> MigrateFromFilesToDatabaseAsync(CancellationToken cancellationToken = default);
    Task<bool> HasMigrationBeenRunAsync(CancellationToken cancellationToken = default);
    Task MarkMigrationAsCompleteAsync(CancellationToken cancellationToken = default);
}

public class DatabaseMigrationService : IDatabaseMigrationService
{
    private readonly IFileDataService _fileDataService;
    private readonly IRepository<Note> _databaseRepository;
    private readonly ILogger<DatabaseMigrationService> _logger;
    private const string MigrationMarkerKey = "migration_completed";

    public DatabaseMigrationService(
        IFileDataService fileDataService,
        IRepository<Note> databaseRepository,
        ILogger<DatabaseMigrationService> logger)
    {
        _fileDataService = fileDataService;
        _databaseRepository = databaseRepository;
        _logger = logger;
    }

    public async Task<int> MigrateFromFilesToDatabaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting migration from JSON files to SQLite database");

            // Check if migration has already been run
            if (await HasMigrationBeenRunAsync(cancellationToken))
            {
                _logger.LogInformation("Migration has already been completed, skipping");
                return 0;
            }

            // Get all notes from file system
            var fileNotes = await _fileDataService.LoadNotesAsync();
            if (!fileNotes.Any())
            {
                _logger.LogInformation("No notes found in file system to migrate");
                await MarkMigrationAsCompleteAsync(cancellationToken);
                return 0;
            }

            _logger.LogInformation("Found {Count} notes in file system to migrate", fileNotes.Count());

            var migratedCount = 0;
            var errors = new List<string>();

            foreach (var note in fileNotes)
            {
                try
                {
                    // Check if note already exists in database (by ID or filename)
                    var existingNote = await _databaseRepository.GetByIdAsync(note.Id, cancellationToken);
                    if (existingNote != null)
                    {
                        _logger.LogDebug("Note with ID {Id} already exists in database, skipping", note.Id);
                        continue;
                    }

                    // Ensure the note has required properties
                    if (string.IsNullOrEmpty(note.Id))
                    {
                        note.Id = Guid.NewGuid().ToString();
                        _logger.LogDebug("Generated new ID for note: {Id}", note.Id);
                    }

                    if (string.IsNullOrEmpty(note.Filename))
                    {
                        note.Filename = $"{note.Id}.notes.txt";
                        _logger.LogDebug("Generated filename for note: {Filename}", note.Filename);
                    }

                    // Migrate note to database
                    await _databaseRepository.AddAsync(note, cancellationToken);
                    migratedCount++;
                    
                    _logger.LogDebug("Successfully migrated note: {Id}", note.Id);
                }
                catch (Exception ex)
                {
                    var error = $"Failed to migrate note {note.Id}: {ex.Message}";
                    errors.Add(error);
                    _logger.LogError(ex, "Failed to migrate note {Id}", note.Id);
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Migration completed with {ErrorCount} errors: {Errors}", 
                    errors.Count, string.Join("; ", errors));
            }

            // Mark migration as complete
            await MarkMigrationAsCompleteAsync(cancellationToken);
            
            _logger.LogInformation("Migration completed successfully. Migrated {MigratedCount} notes", migratedCount);
            return migratedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration failed with error");
            throw;
        }
    }

    public async Task<bool> HasMigrationBeenRunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if we have any notes in the database as a simple migration marker
            var existingNotes = await _databaseRepository.GetAllAsync(cancellationToken);
            var hasNotes = existingNotes.Any();
            
            // Also check for explicit migration marker if we had a way to store it
            // For now, we'll use the presence of notes as the indicator
            return hasNotes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check migration status");
            return false;
        }
    }

    public async Task MarkMigrationAsCompleteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // For this implementation, the presence of notes in the database serves as our migration marker
            // In a more complex scenario, we could create a separate migration tracking table
            _logger.LogDebug("Migration marked as complete");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark migration as complete");
            throw;
        }
    }
}
