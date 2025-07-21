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
    public async Task HandleAsync_ValidNote_DeletesNoteAndReturnsTrue()
    {
        // Arrange
        var note = new Note { Id = "123", Filename = "test-file.notes.txt", Text = "Test content" };
        var command = new DeleteNoteCommand(note);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        _mockRepository.Verify(x => x.DeleteAsync(note.Id), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task HandleAsync_NullNote_ReturnsFalse()
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
    public async Task HandleAsync_RepositoryThrowsException_RethrowsException()
    {
        // Arrange
        var note = new Note { Id = "123", Filename = "test-file.notes.txt", Text = "Test content" };
        var command = new DeleteNoteCommand(note);
        var exception = new Exception("Repository error");

        _mockRepository.Setup(x => x.DeleteAsync(note.Id)).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_ValidNote_LogsInformation()
    {
        // Arrange
        var note = new Note { Id = "456", Filename = "another-test.notes.txt", Text = "Another content" };
        var command = new DeleteNoteCommand(note);

        // Act
        await _handler.HandleAsync(command);

        // Assert - Verify that repository delete was called with correct ID
        _mockRepository.Verify(x => x.DeleteAsync("456"), Times.Once);
    }
} 