namespace Notes.Integration.Tests.Services;

public class EndToEndIntegrationTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly IServiceProvider _serviceProvider;

    public EndToEndIntegrationTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "EndToEndTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);

        var services = new ServiceCollection();
        services.AddCoreServices(_testDataPath);
        _serviceProvider = services.BuildServiceProvider();

        // Initialize database and run migration
        InitializeSystemAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeSystemAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        
        // Initialize database
        var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationService>();
        await databaseInitializer.InitializeDatabaseAsync();
        
        // Run migration
        var migrationService = scope.ServiceProvider.GetRequiredService<IDatabaseMigrationService>();
        await migrationService.MigrateFromFilesToDatabaseAsync();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "EndToEnd")]
    public async Task CompleteNoteLifecycle_WithMediatR_WorksEndToEnd()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert - Complete note lifecycle using MediatR commands and queries

        // 1. Create a new note
        var newNote = new Note { Text = "End-to-end test note" };
        var createCommand = new SaveNoteCommand(newNote);
        var createdNote = await mediator.Send(createCommand);

        Assert.NotNull(createdNote);
        Assert.NotNull(createdNote.Id);
        Assert.NotNull(createdNote.Filename);
        Assert.Equal("End-to-end test note", createdNote.Text);

        // 2. Retrieve all notes
        var getAllQuery = new GetAllNotesQuery();
        var allNotes = await mediator.Send(getAllQuery);

        Assert.Contains(allNotes, n => n.Id == createdNote.Id);

        // 3. Update the note
        createdNote.Text = "Updated end-to-end test note";
        var updateCommand = new SaveNoteCommand(createdNote);
        var updatedNote = await mediator.Send(updateCommand);

        Assert.Equal("Updated end-to-end test note", updatedNote.Text);
        Assert.Equal(createdNote.Id, updatedNote.Id);

        // 4. Verify the update persisted
        var refreshQuery = new RefreshNotesQuery();
        var refreshedNotes = await mediator.Send(refreshQuery);
        var refreshedNote = refreshedNotes.FirstOrDefault(n => n.Id == createdNote.Id);

        Assert.NotNull(refreshedNote);
        Assert.Equal("Updated end-to-end test note", refreshedNote.Text);

        // 5. Delete the note
        var deleteCommand = new DeleteNoteCommand(updatedNote);
        var deleteResult = await mediator.Send(deleteCommand);

        Assert.True(deleteResult);

        // 6. Verify deletion
        var finalNotes = await mediator.Send(getAllQuery);
        Assert.DoesNotContain(finalNotes, n => n.Id == createdNote.Id);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "EndToEnd")]
    public async Task MigrationAndDatabaseOperations_WorkTogether()
    {
        // Arrange - Create some files for migration
        var fileService = _serviceProvider.GetRequiredService<IFileDataService>();
        var migrationService = _serviceProvider.GetRequiredService<IDatabaseMigrationService>();
        var mediator = _serviceProvider.GetRequiredService<IMediator>();

        // Create test files
        var testNote1 = new Note 
        { 
            Id = "migration-e2e-1", 
            Text = "Migration end-to-end test 1", 
            Filename = "migration-e2e-1.notes.txt",
            UpdatedAt = DateTime.Now.AddMinutes(-10)
        };
        var testNote2 = new Note 
        { 
            Id = "migration-e2e-2", 
            Text = "Migration end-to-end test 2", 
            Filename = "migration-e2e-2.notes.txt",
            UpdatedAt = DateTime.Now.AddMinutes(-5)
        };

        await fileService.SaveNoteAsync(testNote1);
        await fileService.SaveNoteAsync(testNote2);

        // Act - Run migration
        var migratedCount = await migrationService.MigrateFromFilesToDatabaseAsync();

        // Assert migration worked
        Assert.Equal(2, migratedCount);

        // Verify notes are accessible through MediatR
        var allNotes = await mediator.Send(new GetAllNotesQuery());
        
        Assert.Equal(2, allNotes.Count());
        Assert.Contains(allNotes, n => n.Id == "migration-e2e-1" && n.Text == "Migration end-to-end test 1");
        Assert.Contains(allNotes, n => n.Id == "migration-e2e-2" && n.Text == "Migration end-to-end test 2");

        // Test that we can add new notes after migration
        var newNote = new Note { Text = "Post-migration note" };
        var savedNewNote = await mediator.Send(new SaveNoteCommand(newNote));

        Assert.NotNull(savedNewNote);
        Assert.NotNull(savedNewNote.Id);

        // Verify all notes are still accessible
        var finalNotes = await mediator.Send(new GetAllNotesQuery());
        Assert.Equal(3, finalNotes.Count());
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "EndToEnd")]
    public async Task SystemRestart_PreservesData()
    {
        // Arrange - Create some notes
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        
        var note1 = new Note { Text = "Persistence test note 1" };
        var note2 = new Note { Text = "Persistence test note 2" };
        
        var saved1 = await mediator.Send(new SaveNoteCommand(note1));
        var saved2 = await mediator.Send(new SaveNoteCommand(note2));

        // Simulate system restart by creating new service provider
        var newServices = new ServiceCollection();
        newServices.AddCoreServices(_testDataPath);
        using var newServiceProvider = newServices.BuildServiceProvider();

        // Initialize the "restarted" system
        using var scope = newServiceProvider.CreateScope();
        var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationService>();
        await databaseInitializer.InitializeDatabaseAsync();

        // Act - Retrieve notes with new service provider
        var newMediator = newServiceProvider.GetRequiredService<IMediator>();
        var retrievedNotes = await newMediator.Send(new GetAllNotesQuery());

        // Assert - Data should be preserved
        Assert.Equal(2, retrievedNotes.Count());
        Assert.Contains(retrievedNotes, n => n.Id == saved1.Id && n.Text == "Persistence test note 1");
        Assert.Contains(retrievedNotes, n => n.Id == saved2.Id && n.Text == "Persistence test note 2");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "EndToEnd")]
    public async Task BulkOperations_PerformCorrectly()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var repository = _serviceProvider.GetRequiredService<IRepository<Note>>();

        // Act - Create multiple notes
        var createTasks = Enumerable.Range(1, 20)
            .Select(i => new Note { Text = $"Bulk test note {i}" })
            .Select(note => mediator.Send(new SaveNoteCommand(note)))
            .ToArray();

        var createdNotes = await Task.WhenAll(createTasks);

        // Assert creation
        Assert.Equal(20, createdNotes.Length);
        Assert.All(createdNotes, note => Assert.NotNull(note.Id));

        // Verify all notes are retrievable
        var allNotes = await mediator.Send(new GetAllNotesQuery());
        Assert.Equal(20, allNotes.Count());

        // Test bulk deletion
        var deleteTasks = createdNotes
            .Select(note => mediator.Send(new DeleteNoteCommand(note)))
            .ToArray();

        var deleteResults = await Task.WhenAll(deleteTasks);
        Assert.All(deleteResults, result => Assert.True(result));

        // Verify all notes are deleted
        var finalNotes = await mediator.Send(new GetAllNotesQuery());
        Assert.Empty(finalNotes);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "EndToEnd")]
    public async Task ErrorHandling_GracefulDegradation()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert - Test various error scenarios

        // 1. Delete non-existent note
        var nonExistentNote = new Note { Id = "non-existent-id", Text = "Does not exist", Filename = "none.notes.txt" };
        var deleteResult = await mediator.Send(new DeleteNoteCommand(nonExistentNote));
        Assert.False(deleteResult);

        // 2. Save and retrieve note with special characters
        var specialNote = new Note { Text = "Special chars: éñüñül 中文 🎉 \"quotes\" 'apostrophes'" };
        var savedSpecial = await mediator.Send(new SaveNoteCommand(specialNote));
        
        Assert.NotNull(savedSpecial);
        Assert.Equal("Special chars: éñüñül 中文 🎉 \"quotes\" 'apostrophes'", savedSpecial.Text);

        var retrievedSpecial = (await mediator.Send(new GetAllNotesQuery())).First();
        Assert.Equal("Special chars: éñüñül 中文 🎉 \"quotes\" 'apostrophes'", retrievedSpecial.Text);

        // 3. Save note with very long text
        var longText = new string('A', 10000);
        var longNote = new Note { Text = longText };
        var savedLong = await mediator.Send(new SaveNoteCommand(longNote));
        
        Assert.NotNull(savedLong);
        Assert.Equal(10000, savedLong.Text.Length);
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
