namespace Notes.Core.Tests.Handlers;

public class SaveNoteHandlerTests
{
    private readonly Mock<IRepository<Note>> _mockRepository;
    private readonly Mock<ILogger<SaveNoteHandler>> _mockLogger;
    private readonly SaveNoteHandler _handler;

    public SaveNoteHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Note>>();
        _mockLogger = new Mock<ILogger<SaveNoteHandler>>();
        _handler = new SaveNoteHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Handlers")]
    public async Task Handle_NewNote_CallsAddAsync()
    {
        // Arrange
        var note = new Note { Text = "Test Content" }; // No ID = new note
        var command = new SaveNoteCommand(note);
        var expectedNote = new Note
        {
            Id = "123",
            Filename = "test-file.notes.txt",
            Text = note.Text,
            UpdatedAt = note.UpdatedAt
        };

        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Note?)null);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedNote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Content", result.Text);
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Handlers")]
    public async Task Handle_ExistingNote_CallsUpdateAsync()
    {
        // Arrange
        var existingId = "existing-id";
        var note = new Note
        {
            Id = existingId,
            Text = "Updated content"
        };
        var command = new SaveNoteCommand(note);

        var existingNote = new Note
        {
            Id = existingId,
            Filename = "existing-file.notes.txt",
            Text = "Old content"
        };

        var expectedNote = new Note
        {
            Id = existingId,
            Filename = existingNote.Filename,
            Text = note.Text,
            UpdatedAt = note.UpdatedAt
        };

        _mockRepository.Setup(x => x.GetByIdAsync(existingId, It.IsAny<CancellationToken>())).ReturnsAsync(existingNote);
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedNote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.NotNull(result);
        Assert.Equal(existingId, result.Id);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Handlers")]
    public async Task Handle_RepositoryThrowsException_RethrowsException()
    {
        // Arrange
        var note = new Note { Text = "Test Content" };
        var command = new SaveNoteCommand(note);
        var exception = new Exception("Repository error");

        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Note?)null);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    }
} 