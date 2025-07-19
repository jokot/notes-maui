namespace Notes.Integration.Tests.Services;

public class NoteServiceIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;

    public NoteServiceIntegrationTests()
    {
        var services = new ServiceCollection();
        
        // Register Core services
        services.AddLogging();
        services.AddSingleton<IFileDataService, FileDataService>();
        services.AddSingleton<IRepository<Note>, NoteRepository>();
        services.AddSingleton<SaveNoteHandler>();
        services.AddSingleton<GetAllNotesHandler>();
        services.AddSingleton<RefreshNotesHandler>();
        services.AddSingleton<DeleteNoteHandler>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task SaveAndRetrieveNote_Integration_WorksCorrectly()
    {
        // Arrange
        var saveHandler = _serviceProvider.GetRequiredService<SaveNoteHandler>();
        var getAllHandler = _serviceProvider.GetRequiredService<GetAllNotesHandler>();
        var saveCommand = new SaveNoteCommand("Integration Test", "This is a test note for integration testing");

        // Act
        var savedNote = await saveHandler.HandleAsync(saveCommand);
        var allNotes = await getAllHandler.HandleAsync(new GetAllNotesCommand());

        // Assert
        Assert.NotNull(savedNote);
        Assert.Equal("This is a test note for integration testing", savedNote.Text);
        Assert.Contains(allNotes, n => n.Text == "This is a test note for integration testing");
    }

    [Fact]
    public async Task SaveUpdateDeleteNote_FullWorkflow_WorksCorrectly()
    {
        // Arrange
        var saveHandler = _serviceProvider.GetRequiredService<SaveNoteHandler>();
        var getAllHandler = _serviceProvider.GetRequiredService<GetAllNotesHandler>();
        var deleteHandler = _serviceProvider.GetRequiredService<DeleteNoteHandler>();

        // Act - Create
        var createCommand = new SaveNoteCommand("Workflow Test", "Original content");
        var createdNote = await saveHandler.HandleAsync(createCommand);
        
        // Act - Update
        var updateCommand = new SaveNoteCommand("Workflow Test", "Updated content", createdNote.Filename);
        var updatedNote = await saveHandler.HandleAsync(updateCommand);
        
        // Act - Delete
        var deleteCommand = new DeleteNoteCommand(updatedNote.Filename);
        var deleteResult = await deleteHandler.HandleAsync(deleteCommand);
        
        // Act - Verify deletion
        var finalNotes = await getAllHandler.HandleAsync(new GetAllNotesCommand());

        // Assert
        Assert.NotNull(createdNote);
        Assert.NotNull(updatedNote);
        Assert.True(deleteResult);
        Assert.DoesNotContain(finalNotes, n => n.Id == createdNote.Id);
    }
} 