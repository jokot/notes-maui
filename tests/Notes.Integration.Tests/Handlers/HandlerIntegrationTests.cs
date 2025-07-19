namespace Notes.Integration.Tests.Handlers;

public class HandlerIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;

    public HandlerIntegrationTests()
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
    public async Task SaveNoteHandler_WithRepository_IntegratesCorrectly()
    {
        // Arrange
        var handler = _serviceProvider.GetRequiredService<SaveNoteHandler>();
        var command = new SaveNoteCommand("Integration Test", "Handler integration test content");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Handler integration test content", result.Text);
        Assert.NotNull(result.Filename);
        Assert.True(result.Date <= DateTime.Now);
    }

    [Fact]
    public async Task DeleteNoteHandler_WithValidNote_IntegratesCorrectly()
    {
        // Arrange
        var saveHandler = _serviceProvider.GetRequiredService<SaveNoteHandler>();
        var deleteHandler = _serviceProvider.GetRequiredService<DeleteNoteHandler>();
        
        // First create a note
        var saveCommand = new SaveNoteCommand("Delete Test", "This note will be deleted");
        var savedNote = await saveHandler.HandleAsync(saveCommand);
        
        var deleteCommand = new DeleteNoteCommand(savedNote.Filename);

        // Act
        var result = await deleteHandler.HandleAsync(deleteCommand);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetAllNotesHandler_WithMultipleNotes_ReturnsAllNotes()
    {
        // Arrange
        var saveHandler = _serviceProvider.GetRequiredService<SaveNoteHandler>();
        var getAllHandler = _serviceProvider.GetRequiredService<GetAllNotesHandler>();
        
        // Create multiple notes
        await saveHandler.HandleAsync(new SaveNoteCommand("Note 1", "Content 1"));
        await saveHandler.HandleAsync(new SaveNoteCommand("Note 2", "Content 2"));
        
        var command = new GetAllNotesCommand();

        // Act
        var result = await getAllHandler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count() >= 2); // At least the two we created
    }

    [Fact]
    public async Task CommandHandlerPipeline_FullWorkflow_IntegratesCorrectly()
    {
        // Arrange
        var saveHandler = _serviceProvider.GetRequiredService<SaveNoteHandler>();
        var getAllHandler = _serviceProvider.GetRequiredService<GetAllNotesHandler>();
        var deleteHandler = _serviceProvider.GetRequiredService<DeleteNoteHandler>();

        // Act - Execute full CQRS pipeline
        var saveCommand = new SaveNoteCommand("Pipeline Test", "Testing the full command handler pipeline");
        var savedNote = await saveHandler.HandleAsync(saveCommand);
        
        var getAllCommand = new GetAllNotesCommand();
        var allNotesBeforeDelete = await getAllHandler.HandleAsync(getAllCommand);
        
        var deleteCommand = new DeleteNoteCommand(savedNote.Filename);
        var deleteResult = await deleteHandler.HandleAsync(deleteCommand);
        
        var allNotesAfterDelete = await getAllHandler.HandleAsync(getAllCommand);

        // Assert
        Assert.NotNull(savedNote);
        Assert.Contains(allNotesBeforeDelete, n => n.Text == "Testing the full command handler pipeline");
        Assert.True(deleteResult);
        Assert.DoesNotContain(allNotesAfterDelete, n => n.Text == "Testing the full command handler pipeline");
    }
} 