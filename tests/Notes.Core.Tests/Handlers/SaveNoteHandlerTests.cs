namespace Notes.Core.Tests.Handlers;

public class SaveNoteHandlerTests
{
    private readonly Mock<IRepository<Note>> _mockRepository;
    private readonly Mock<IFileDataService> _mockFileDataService;
    private readonly Mock<ILogger<SaveNoteHandler>> _mockLogger;
    private readonly SaveNoteHandler _handler;

    public SaveNoteHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Note>>();
        _mockFileDataService = new Mock<IFileDataService>();
        _mockLogger = new Mock<ILogger<SaveNoteHandler>>();
        _handler = new SaveNoteHandler(_mockRepository.Object, _mockFileDataService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_NewNote_CallsAddAsync()
    {
        // Arrange
        var note = new Note { Text = "Test Content" }; // No filename = new note
        var command = new SaveNoteCommand(note);
        var expectedNote = new Note
        {
            Id = "123",
            Filename = "test-file.notes.txt",
            Text = note.Text,
            UpdatedAt = note.UpdatedAt
        };

        _mockFileDataService.Setup(x => x.GenerateFilename()).Returns("generated-filename.notes.txt");
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Note>())).ReturnsAsync(expectedNote);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Content", result.Text);
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Note>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Note>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ExistingNote_CallsUpdateAsync()
    {
        // Arrange
        var existingFileName = "existing-file.notes.txt";
        var note = new Note
        {
            Filename = existingFileName,
            Text = "Updated content"
        };
        var command = new SaveNoteCommand(note);

        var expectedNote = new Note
        {
            Id = "456",
            Filename = existingFileName,
            Text = note.Text,
            UpdatedAt = note.UpdatedAt
        };

        _mockFileDataService.Setup(x => x.NoteExistsAsync(existingFileName)).ReturnsAsync(true);
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Note>())).ReturnsAsync(expectedNote);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Note>()), Times.Once);
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Note>()), Times.Never);
        Assert.NotNull(result);
        Assert.Equal(existingFileName, result.Filename);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrowsException_RethrowsException()
    {
        // Arrange
        var note = new Note { Text = "Test Content" };
        var command = new SaveNoteCommand(note);
        var exception = new Exception("Repository error");

        _mockFileDataService.Setup(x => x.GenerateFilename()).Returns("test-filename.notes.txt");
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Note>())).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(command));
    }
} 