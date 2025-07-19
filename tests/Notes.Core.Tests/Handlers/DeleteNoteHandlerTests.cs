namespace Notes.Core.Tests.Handlers;

public class DeleteNoteHandlerTests
{
    private readonly Mock<IRepository<Note>> _mockRepository;
    private readonly Mock<ILogger<DeleteNoteHandler>> _mockLogger;
    private readonly DeleteNoteHandler _handler;

    public DeleteNoteHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Note>>();
        _mockLogger = new Mock<ILogger<DeleteNoteHandler>>();
        _handler = new DeleteNoteHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidFileName_DeletesNoteAndReturnsTrue()
    {
        // Arrange
        var fileName = "test-file.notes.txt";
        var command = new DeleteNoteCommand(fileName);
        var existingNote = new Note { Id = "123", Filename = fileName };
        var notes = new List<Note> { existingNote };

        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(notes);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        _mockRepository.Verify(x => x.DeleteAsync(existingNote.Id), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task HandleAsync_EmptyFileName_ReturnsFalse()
    {
        // Arrange
        var command = new DeleteNoteCommand("");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
        Assert.False(result);
    }

    [Fact]
    public async Task HandleAsync_NullFileName_ReturnsFalse()
    {
        // Arrange
        var command = new DeleteNoteCommand(null!);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
        Assert.False(result);
    }

    [Fact]
    public async Task HandleAsync_NoteNotFound_ReturnsFalse()
    {
        // Arrange
        var fileName = "non-existent-file.notes.txt";
        var command = new DeleteNoteCommand(fileName);
        var notes = new List<Note>(); // Empty list

        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(notes);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
        Assert.False(result);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrowsException_RethrowsException()
    {
        // Arrange
        var fileName = "test-file.notes.txt";
        var command = new DeleteNoteCommand(fileName);
        var exception = new Exception("Repository error");

        _mockRepository.Setup(x => x.GetAllAsync()).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(command));
    }
} 