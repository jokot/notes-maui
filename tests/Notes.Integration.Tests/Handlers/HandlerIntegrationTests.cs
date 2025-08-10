namespace Notes.Integration.Tests.Handlers;

public class HandlerIntegrationTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly IServiceProvider _serviceProvider;

    public HandlerIntegrationTests()
    {
        // Each test class gets its own temporary directory for complete isolation
        _testDataPath = Path.Combine(Path.GetTempPath(), "NotesHandlerTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
        
        var services = new ServiceCollection();
        
        // Use the Core service registration to ensure consistency
        services.AddCoreServices(_testDataPath);
        
        _serviceProvider = services.BuildServiceProvider();
        
        // Initialize the database for testing
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
    [Trait("Category", "Handlers")]
    public async Task SaveNoteHandler_WithRepository_IntegratesCorrectly()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var note = new Note { Text = "Handler integration test content" };
        var command = new SaveNoteCommand(note);

        // Act
        var result = await mediator.Send(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Handler integration test content", result.Text);
        Assert.NotNull(result.Filename);
        Assert.True(result.UpdatedAt <= DateTime.Now);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Handlers")]
    public async Task DeleteNoteHandler_WithValidNote_IntegratesCorrectly()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();

        // First create a note to delete
        var note = new Note { Text = "This note will be deleted" };
        var saveCommand = new SaveNoteCommand(note);
        var savedNote = await mediator.Send(saveCommand);

        // Act
        var deleteCommand = new DeleteNoteCommand(savedNote);
        var result = await mediator.Send(deleteCommand);

        // Verify deletion by checking repository state
        var notesAfterDelete = await mediator.Send(new GetAllNotesQuery());
        var noteExistsAfterDelete = notesAfterDelete.Any(n => n.Id == savedNote.Id);

        // Assert
        Assert.True(result);
        Assert.False(noteExistsAfterDelete, $"Note with ID {savedNote.Id} should not exist after deletion");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Handlers")]
    public async Task DeleteNoteHandler_IsolatedTest_WorksCorrectly()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();

        // Create a note
        var note = new Note { Text = "Isolated Test" };
        var saveCommand = new SaveNoteCommand(note);
        var savedNote = await mediator.Send(saveCommand);

        // Verify it exists before deletion
        var notesBeforeDelete = await mediator.Send(new GetAllNotesQuery());
        var noteExistsBefore = notesBeforeDelete.Any(n => n.Id == savedNote.Id);

        // Act - Delete the note
        var deleteCommand = new DeleteNoteCommand(savedNote);
        var deleteResult = await mediator.Send(deleteCommand);

        // Verify deletion
        var notesAfterDelete = await mediator.Send(new GetAllNotesQuery());
        var noteExistsAfter = notesAfterDelete.Any(n => n.Id == savedNote.Id);

        // Assert
        Assert.True(noteExistsBefore, "Note should exist before deletion");
        Assert.True(deleteResult, "Delete operation should succeed");
        Assert.False(noteExistsAfter, $"Note with ID {savedNote.Id} should not exist after deletion");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Handlers")]
    public async Task GetAllNotesHandler_WithMultipleNotes_ReturnsAllNotes()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        
        // Create multiple notes
        var note1 = new Note { Text = "Content 1" };
        var note2 = new Note { Text = "Content 2" };
        await mediator.Send(new SaveNoteCommand(note1));
        await mediator.Send(new SaveNoteCommand(note2));
        
        var query = new GetAllNotesQuery();

        // Act
        var result = await mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count() >= 2); // At least the two we created
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Handlers")]
    public async Task CommandHandlerPipeline_FullWorkflow_IntegratesCorrectly()
    {
        // Arrange - Use the same service provider with MediatR
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var noteRepository = _serviceProvider.GetRequiredService<IRepository<Note>>();

        // Act - Execute full CQRS pipeline with unique text
        var uniqueText = $"Testing the full command handler pipeline {Guid.NewGuid()}";
        var note = new Note { Text = uniqueText };
        var saveCommand = new SaveNoteCommand(note);
        var savedNote = await mediator.Send(saveCommand);
        
        // Verify note was created in database
        var noteExistsInDb = await noteRepository.GetByIdAsync(savedNote.Id) != null;
        
        var deleteCommand = new DeleteNoteCommand(savedNote);
        var deleteResult = await mediator.Send(deleteCommand);
        
        // Verify note was deleted from database
        var noteExistsAfterDelete = await noteRepository.GetByIdAsync(savedNote.Id) != null;

        // Assert - Focus on the core save/delete operations
        Assert.NotNull(savedNote);
        Assert.NotNull(savedNote.Filename);
        Assert.True(noteExistsInDb, "Note should be created in database after save");
        Assert.True(deleteResult, "Delete operation should succeed");
        Assert.False(noteExistsAfterDelete, "Note should be deleted from database after delete operation");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Handlers")]
    public async Task DebugSaveAndLoad_TraceProblem()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var noteRepository = _serviceProvider.GetRequiredService<IRepository<Note>>();

        // Create a unique note for this test
        var uniqueText = $"Debug test note {Guid.NewGuid()}";
        var note = new Note { Text = uniqueText };
        var saveCommand = new SaveNoteCommand(note);
        
        // Act - Save note
        var savedNote = await mediator.Send(saveCommand);
        
        // Debug: Check if note was actually created in database
        var noteExistsInDb = await noteRepository.GetByIdAsync(savedNote.Id) != null;
        
        // Debug: Load notes through repository
        var notesFromRepository = await mediator.Send(new GetAllNotesQuery());
        var noteFromRepository = notesFromRepository.FirstOrDefault(n => n.Text == uniqueText);

        // Assert with detailed information
        Assert.NotNull(savedNote);
        Assert.True(noteExistsInDb, $"Note should exist in database with ID: {savedNote.Id}");
        Assert.True(noteFromRepository != null, $"Note should be found by Repository. Found {notesFromRepository.Count()} total notes");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Handlers")]
    public async Task DebugDeleteOperation_TraceProblem()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var noteRepository = _serviceProvider.GetRequiredService<IRepository<Note>>();

        // Create a unique note for this test to avoid conflicts
        var uniqueText = $"Debug test note {Guid.NewGuid()}";
        var note = new Note { Text = uniqueText };
        var saveCommand = new SaveNoteCommand(note);
        
        // Act - Save note
        var savedNote = await mediator.Send(saveCommand);
        
        // Debug 1: Verify it exists in repository
        var notesBeforeDelete = await mediator.Send(new GetAllNotesQuery());
        var noteBeforeDelete = notesBeforeDelete.FirstOrDefault(n => n.Id == savedNote.Id);
        
        // Debug 2: Verify note exists in database
        var noteExistsBeforeDelete = await noteRepository.GetByIdAsync(savedNote.Id, CancellationToken.None);
        
        // Delete the note
        var deleteCommand = new DeleteNoteCommand(savedNote);
        var deleteResult = await mediator.Send(deleteCommand);
        
        // Debug 3: Check if note was actually deleted from database
        var noteExistsAfterDelete = await noteRepository.GetByIdAsync(savedNote.Id, CancellationToken.None);
        
        // Debug 4: Check repository state after delete
        var notesAfterDelete = await mediator.Send(new GetAllNotesQuery());
        var noteAfterDelete = notesAfterDelete.FirstOrDefault(n => n.Id == savedNote.Id);

        // Assert with detailed debug information
        Assert.NotNull(savedNote);
        Assert.NotNull(noteBeforeDelete);
        Assert.NotNull(noteExistsBeforeDelete);
        Assert.True(deleteResult, "Delete operation should return true");
        
        // Check database deletion separately
        if (noteExistsAfterDelete != null)
        {
            Assert.Fail($"Note still exists in database after delete: {savedNote.Id}");
        }
        
        // Check repository state separately
        if (noteAfterDelete != null)
        {
            Assert.Fail($"Note still exists in repository after delete. ID: {noteAfterDelete.Id}, Text: {noteAfterDelete.Text[..Math.Min(50, noteAfterDelete.Text.Length)]}");
        }
    }

    public void Dispose()
    {
        // Clean up the test directory
        if (Directory.Exists(_testDataPath))
        {
            try
            {
                Directory.Delete(_testDataPath, recursive: true);
            }
            catch
            {
                // Best effort cleanup - OS will eventually clean up temp files
            }
        }
    }
} 