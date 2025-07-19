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
        var command = new SaveNoteCommand("Test Title", "Test Content");
        var expectedNote = new Note
        {
            Filename = "expected-filename.notes.txt",
            Text = command.Text,
            Date = command.Date
        };

        _mockFileDataService.Setup(x => x.NoteExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Note>())).ReturnsAsync(expectedNote);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Note>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Note>()), Times.Never);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task HandleAsync_ExistingNote_CallsUpdateAsync()
    {
        // Arrange
        var existingFileName = "existing-file.notes.txt";
        var command = new SaveNoteCommand("Test Title", "Test Content", existingFileName);
        var expectedNote = new Note
        {
            Filename = existingFileName,
            Text = command.Text,
            Date = command.Date
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
        var command = new SaveNoteCommand("Test Title", "Test Content");
        var exception = new Exception("Repository error");

        _mockFileDataService.Setup(x => x.NoteExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Note>())).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(command));
    }
} 