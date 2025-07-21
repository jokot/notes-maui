namespace Notes.Integration.Tests.Services;

public class NoteServiceIntegrationTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly IServiceProvider _serviceProvider;

    public NoteServiceIntegrationTests()
    {
        // Each test class gets its own temporary directory for complete isolation
        _testDataPath = Path.Combine(Path.GetTempPath(), "NotesIntegrationTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
        
        var services = new ServiceCollection();
        
        // Register Core services as Singletons for consistent cache state within tests
        services.AddLogging();
        services.AddSingleton<IFileDataService>(sp => 
            new FileDataService(sp.GetRequiredService<ILogger<FileDataService>>(), _testDataPath));
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
        var note = new Note { Text = "This is a test note for integration testing" };
        var saveCommand = new SaveNoteCommand(note);

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

        // Create a note
        var note = new Note { Text = "Original content" };
        var saveCommand = new SaveNoteCommand(note);
        var createdNote = await saveHandler.HandleAsync(saveCommand);

        // Update the note
        createdNote.Text = $"Updated content {Guid.NewGuid()}";
        var updateCommand = new SaveNoteCommand(createdNote);
        var savedUpdatedNote = await saveHandler.HandleAsync(updateCommand);

        // Delete the note
        var deleteCommand = new DeleteNoteCommand(savedUpdatedNote);
        await deleteHandler.HandleAsync(deleteCommand);

        // Verify final state
        var finalNotes = await getAllHandler.HandleAsync(new GetAllNotesCommand());

        // Assert
        Assert.NotNull(createdNote);
        Assert.NotNull(savedUpdatedNote);
        Assert.Equal(createdNote.Id, savedUpdatedNote.Id); // Should be same note
        Assert.Contains("Updated content", savedUpdatedNote.Text);
        Assert.DoesNotContain(finalNotes, n => n.Id == savedUpdatedNote.Id);
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