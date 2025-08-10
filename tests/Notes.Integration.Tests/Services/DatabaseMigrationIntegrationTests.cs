namespace Notes.Integration.Tests.Services;

public class DatabaseMigrationIntegrationTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseMigrationIntegrationTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "MigrationTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);

        var services = new ServiceCollection();
        services.AddCoreServices(_testDataPath);
        _serviceProvider = services.BuildServiceProvider();

        // Initialize database
        InitializeDatabaseAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeDatabaseAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationService>();
        await databaseInitializer.InitializeDatabaseAsync();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Migration")]
    public async Task MigrateFromFilesToDatabaseAsync_WithRealFiles_MigratesSuccessfully()
    {
        // Arrange
        var fileService = _serviceProvider.GetRequiredService<IFileDataService>();
        var migrationService = _serviceProvider.GetRequiredService<IDatabaseMigrationService>();
        var databaseRepository = _serviceProvider.GetRequiredService<IRepository<Note>>();

        // Create some test notes in the file system first
        var testNotes = new List<Note>
        {
            new() { Id = "test-1", Text = "Migration Test Note 1", Filename = "migration-test-1.notes.txt" },
            new() { Id = "test-2", Text = "Migration Test Note 2", Filename = "migration-test-2.notes.txt" }
        };

        foreach (var note in testNotes)
        {
            await fileService.SaveNoteAsync(note);
        }

        // Verify files were created
        var filesBeforeMigration = await fileService.LoadNotesAsync();
        Assert.Equal(2, filesBeforeMigration.Count());

        // Verify database is empty
        var dbNotesBeforeMigration = await databaseRepository.GetAllAsync();
        Assert.Empty(dbNotesBeforeMigration);

        // Act - Run migration
        var migratedCount = await migrationService.MigrateFromFilesToDatabaseAsync();

        // Assert
        Assert.Equal(2, migratedCount);

        // Verify notes are now in database
        var dbNotesAfterMigration = await databaseRepository.GetAllAsync();
        Assert.Equal(2, dbNotesAfterMigration.Count());

        // Verify note content is preserved
        var migratedNote1 = dbNotesAfterMigration.FirstOrDefault(n => n.Id == "test-1");
        var migratedNote2 = dbNotesAfterMigration.FirstOrDefault(n => n.Id == "test-2");

        Assert.NotNull(migratedNote1);
        Assert.NotNull(migratedNote2);
        Assert.Equal("Migration Test Note 1", migratedNote1.Text);
        Assert.Equal("Migration Test Note 2", migratedNote2.Text);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Migration")]
    public async Task MigrateFromFilesToDatabaseAsync_RunTwice_DoesNotDuplicateNotes()
    {
        // Arrange
        var fileService = _serviceProvider.GetRequiredService<IFileDataService>();
        var migrationService = _serviceProvider.GetRequiredService<IDatabaseMigrationService>();
        var databaseRepository = _serviceProvider.GetRequiredService<IRepository<Note>>();

        // Create a test note in the file system
        var testNote = new Note { Id = "duplicate-test", Text = "Test for duplicate prevention", Filename = "duplicate-test.notes.txt" };
        await fileService.SaveNoteAsync(testNote);

        // Act - Run migration twice
        var firstMigrationCount = await migrationService.MigrateFromFilesToDatabaseAsync();
        var secondMigrationCount = await migrationService.MigrateFromFilesToDatabaseAsync();

        // Assert
        Assert.Equal(1, firstMigrationCount);
        Assert.Equal(0, secondMigrationCount); // Second run should find database is already populated

        // Verify only one note exists in database
        var dbNotes = await databaseRepository.GetAllAsync();
        Assert.Single(dbNotes);
        Assert.Equal("duplicate-test", dbNotes.First().Id);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Migration")]
    public async Task HasMigrationBeenRunAsync_AfterMigration_ReturnsTrue()
    {
        // Arrange
        var fileService = _serviceProvider.GetRequiredService<IFileDataService>();
        var migrationService = _serviceProvider.GetRequiredService<IDatabaseMigrationService>();

        // Create a test note and migrate it
        var testNote = new Note { Id = "migration-check", Text = "Test migration check", Filename = "migration-check.notes.txt" };
        await fileService.SaveNoteAsync(testNote);

        // Verify migration hasn't been run initially
        var initialCheck = await migrationService.HasMigrationBeenRunAsync();
        Assert.False(initialCheck);

        // Act - Run migration
        await migrationService.MigrateFromFilesToDatabaseAsync();

        // Assert - Migration should now be marked as complete
        var finalCheck = await migrationService.HasMigrationBeenRunAsync();
        Assert.True(finalCheck);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataPath))
        {
            try
            {
                Directory.Delete(_testDataPath, recursive: true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }
}
