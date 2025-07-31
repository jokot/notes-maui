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
        
        // Use the Core service registration to ensure consistency with MediatR
        services.AddCoreServices(_testDataPath);
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Services")]
    public async Task SaveAndRetrieveNote_Integration_WorksCorrectly()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var note = new Note { Text = "This is a test note for integration testing" };
        var saveCommand = new SaveNoteCommand(note);

        // Act
        var savedNote = await mediator.Send(saveCommand);
        var allNotes = await mediator.Send(new GetAllNotesQuery());

        // Assert
        Assert.NotNull(savedNote);
        Assert.Equal("This is a test note for integration testing", savedNote.Text);
        Assert.Contains(allNotes, n => n.Text == "This is a test note for integration testing");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Slow")]
    [Trait("Category", "Services")]
    public async Task SaveUpdateDeleteNote_FullWorkflow_WorksCorrectly()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();

        // Create a note
        var note = new Note { Text = "Original content" };
        var saveCommand = new SaveNoteCommand(note);
        var createdNote = await mediator.Send(saveCommand);

        // Update the note
        createdNote.Text = $"Updated content {Guid.NewGuid()}";
        var updateCommand = new SaveNoteCommand(createdNote);
        var savedUpdatedNote = await mediator.Send(updateCommand);

        // Delete the note
        var deleteCommand = new DeleteNoteCommand(savedUpdatedNote);
        await mediator.Send(deleteCommand);

        // Verify final state
        var finalNotes = await mediator.Send(new GetAllNotesQuery());

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