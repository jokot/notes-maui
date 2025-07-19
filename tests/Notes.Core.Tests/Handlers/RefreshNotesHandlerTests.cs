namespace Notes.Core.Tests.Handlers;

public class RefreshNotesHandlerTests
{
    private readonly Mock<IRepository<Note>> _mockRepository;
    private readonly Mock<ILogger<RefreshNotesHandler>> _mockLogger;
    private readonly RefreshNotesHandler _handler;

    public RefreshNotesHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Note>>();
        _mockLogger = new Mock<ILogger<RefreshNotesHandler>>();
        _handler = new RefreshNotesHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotesFromRepositoryForceRefresh()
    {
        // Arrange
        var command = new RefreshNotesCommand();
        var expectedNotes = new List<Note>
        {
            new() { Id = "1", Text = "Note 1", Filename = "note1.txt" },
            new() { Id = "2", Text = "Note 2", Filename = "note2.txt" }
        };

        _mockRepository.Setup(x => x.GetAllForceAsync()).ReturnsAsync(expectedNotes);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        _mockRepository.Verify(x => x.GetAllForceAsync(), Times.Once);
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Never); // Should not call regular GetAll
        Assert.Equal(expectedNotes.Count, result.Count());
        Assert.Equal(expectedNotes, result);
    }

    [Fact]
    public async Task HandleAsync_EmptyRepository_ReturnsEmptyCollection()
    {
        // Arrange
        var command = new RefreshNotesCommand();
        var expectedNotes = new List<Note>();

        _mockRepository.Setup(x => x.GetAllForceAsync()).ReturnsAsync(expectedNotes);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        _mockRepository.Verify(x => x.GetAllForceAsync(), Times.Once);
        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrowsException_RethrowsException()
    {
        // Arrange
        var command = new RefreshNotesCommand();
        var exception = new Exception("Repository error");

        _mockRepository.Setup(x => x.GetAllForceAsync()).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(command));
    }
} 