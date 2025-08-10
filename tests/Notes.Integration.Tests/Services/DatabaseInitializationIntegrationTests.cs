namespace Notes.Integration.Tests.Services;

public class DatabaseInitializationIntegrationTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseInitializationIntegrationTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "DatabaseInitTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);

        var services = new ServiceCollection();
        services.AddCoreServices(_testDataPath);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Database")]
    public async Task InitializeDatabaseAsync_CreatesNewDatabase_Successfully()
    {
        // Arrange
        var databaseInitializer = _serviceProvider.GetRequiredService<IDatabaseInitializationService>();
        var dbPath = Path.Combine(_testDataPath, "notes.db");

        // Verify database doesn't exist initially
        Assert.False(File.Exists(dbPath));

        // Act
        await databaseInitializer.InitializeDatabaseAsync();

        // Assert
        Assert.True(File.Exists(dbPath));

        // Verify database structure is correct by attempting to use it
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Note>>();
        
        var testNote = new Note
        {
            Text = "Database initialization test",
            Filename = "init-test.notes.txt",
            UpdatedAt = DateTime.Now
        };

        var savedNote = await repository.AddAsync(testNote);
        Assert.NotNull(savedNote);
        Assert.NotNull(savedNote.Id);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Database")]
    public async Task InitializeDatabaseAsync_WithExistingDatabase_DoesNotCorrupt()
    {
        // Arrange
        var databaseInitializer = _serviceProvider.GetRequiredService<IDatabaseInitializationService>();
        
        // Initialize database first time
        await databaseInitializer.InitializeDatabaseAsync();
        
        // Add some test data
        using var scope1 = _serviceProvider.CreateScope();
        var repository1 = scope1.ServiceProvider.GetRequiredService<IRepository<Note>>();
        
        var originalNote = new Note
        {
            Text = "Original data before re-initialization",
            Filename = "original.notes.txt",
            UpdatedAt = DateTime.Now
        };
        var savedOriginal = await repository1.AddAsync(originalNote);

        // Act - Initialize again (should be safe)
        await databaseInitializer.InitializeDatabaseAsync();

        // Assert - Original data should still exist
        using var scope2 = _serviceProvider.CreateScope();
        var repository2 = scope2.ServiceProvider.GetRequiredService<IRepository<Note>>();
        
        var retrievedNote = await repository2.GetByIdAsync(savedOriginal.Id);
        Assert.NotNull(retrievedNote);
        Assert.Equal("Original data before re-initialization", retrievedNote.Text);
        Assert.Equal("original.notes.txt", retrievedNote.Filename);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Database")]
    public async Task DatabaseOperations_AfterInitialization_WorkCorrectly()
    {
        // Arrange
        var databaseInitializer = _serviceProvider.GetRequiredService<IDatabaseInitializationService>();
        await databaseInitializer.InitializeDatabaseAsync();

        // Act & Assert - Test all CRUD operations
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Note>>();

        // Create
        var newNote = new Note
        {
            Text = "CRUD test note",
            Filename = "crud-test.notes.txt",
            UpdatedAt = DateTime.Now
        };
        var createdNote = await repository.AddAsync(newNote);
        Assert.NotNull(createdNote);
        Assert.NotNull(createdNote.Id);

        // Read
        var readNote = await repository.GetByIdAsync(createdNote.Id);
        Assert.NotNull(readNote);
        Assert.Equal("CRUD test note", readNote.Text);

        // Update
        readNote.Text = "Updated CRUD test note";
        readNote.UpdatedAt = DateTime.Now;
        var updatedNote = await repository.UpdateAsync(readNote);
        Assert.Equal("Updated CRUD test note", updatedNote.Text);

        // Delete
        var deleteResult = await repository.DeleteAsync(createdNote.Id);
        Assert.True(deleteResult);

        // Verify deletion
        var deletedNote = await repository.GetByIdAsync(createdNote.Id);
        Assert.Null(deletedNote);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Database")]
    public async Task DatabaseInitialization_WithConcurrentRequests_HandlesSafely()
    {
        // Arrange
        var databaseInitializer = _serviceProvider.GetRequiredService<IDatabaseInitializationService>();

        // Act - Initialize database concurrently
        var initTasks = Enumerable.Range(0, 5)
            .Select(_ => databaseInitializer.InitializeDatabaseAsync())
            .ToArray();

        await Task.WhenAll(initTasks);

        // Assert - Database should be properly initialized and functional
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Note>>();

        var testNote = new Note
        {
            Text = "Concurrent initialization test",
            Filename = "concurrent-init.notes.txt",
            UpdatedAt = DateTime.Now
        };

        var savedNote = await repository.AddAsync(testNote);
        Assert.NotNull(savedNote);
        Assert.NotNull(savedNote.Id);

        var retrievedNote = await repository.GetByIdAsync(savedNote.Id);
        Assert.NotNull(retrievedNote);
        Assert.Equal("Concurrent initialization test", retrievedNote.Text);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Database")]
    public async Task DatabaseInitialization_CreatesCorrectSchema()
    {
        // Arrange
        var databaseInitializer = _serviceProvider.GetRequiredService<IDatabaseInitializationService>();
        
        // Act
        await databaseInitializer.InitializeDatabaseAsync();

        // Assert - Verify schema by testing all expected columns
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Note>>();

        var testNote = new Note
        {
            Id = "schema-test-123",
            Text = "Schema validation test with all properties",
            Filename = "schema-test.notes.txt"
            // Note: UpdatedAt will be set automatically by the repository
        };

        var savedNote = await repository.AddAsync(testNote);
        
        // Verify all properties are correctly stored and retrieved
        Assert.Equal("schema-test-123", savedNote.Id);
        Assert.Equal("Schema validation test with all properties", savedNote.Text);
        Assert.Equal("schema-test.notes.txt", savedNote.Filename);
        Assert.NotEqual(default(DateTime), savedNote.UpdatedAt); // Just ensure UpdatedAt is set

        // Retrieve and verify persistence
        var retrievedNote = await repository.GetByIdAsync("schema-test-123");
        Assert.NotNull(retrievedNote);
        Assert.Equal("schema-test-123", retrievedNote.Id);
        Assert.Equal("Schema validation test with all properties", retrievedNote.Text);
        Assert.Equal("schema-test.notes.txt", retrievedNote.Filename);
        Assert.Equal(savedNote.UpdatedAt, retrievedNote.UpdatedAt);
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
