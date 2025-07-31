namespace Notes.Core.Tests.Handlers;

public class GetAllNotesHandlerTests
{
    private readonly Mock<IRepository<Note>> _mockRepository;
    private readonly Mock<ILogger<GetAllNotesHandler>> _mockLogger;
    private readonly GetAllNotesHandler _handler;

    public GetAllNotesHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Note>>();
        _mockLogger = new Mock<ILogger<GetAllNotesHandler>>();
        _handler = new GetAllNotesHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Handlers")]
    public async Task Handle_ReturnsNotesFromRepository()
    {
        // Arrange
        var query = new GetAllNotesQuery();
        var expectedNotes = new List<Note>
        {
            new() { Id = "1", Text = "Note 1", Filename = "note1.txt" },
            new() { Id = "2", Text = "Note 2", Filename = "note2.txt" }
        };

        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(expectedNotes);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
        Assert.Equal(expectedNotes.Count, result.Count());
        Assert.Equal(expectedNotes, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Handlers")]
    public async Task Handle_EmptyRepository_ReturnsEmptyCollection()
    {
        // Arrange
        var query = new GetAllNotesQuery();
        var expectedNotes = new List<Note>();

        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(expectedNotes);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
        Assert.Empty(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Handlers")]
    public async Task Handle_RepositoryThrowsException_RethrowsException()
    {
        // Arrange
        var query = new GetAllNotesQuery();
        var exception = new Exception("Repository error");

        _mockRepository.Setup(x => x.GetAllAsync()).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
    }
} 