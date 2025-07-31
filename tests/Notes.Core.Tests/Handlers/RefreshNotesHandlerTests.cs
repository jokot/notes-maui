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
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Handlers")]
    public async Task Handle_CallsRepositoryForceRefresh()
    {
        // Arrange
        var query = new RefreshNotesQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(x => x.GetAllForceAsync(), Times.Once);
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Never); // Should not call regular GetAll
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Handlers")]
    public async Task Handle_RepositoryThrowsException_RethrowsException()
    {
        // Arrange
        var query = new RefreshNotesQuery();
        var exception = new Exception("Repository error");

        _mockRepository.Setup(x => x.GetAllForceAsync()).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
    }
} 