namespace Notes.Core.Tests.Services.Data;

public class DatabaseMigrationServiceTests
{
    private readonly Mock<IFileDataService> _mockFileDataService;
    private readonly Mock<IRepository<Note>> _mockDatabaseRepository;
    private readonly Mock<ILogger<DatabaseMigrationService>> _mockLogger;
    private readonly DatabaseMigrationService _migrationService;

    public DatabaseMigrationServiceTests()
    {
        _mockFileDataService = new Mock<IFileDataService>();
        _mockDatabaseRepository = new Mock<IRepository<Note>>();
        _mockLogger = new Mock<ILogger<DatabaseMigrationService>>();
        _migrationService = new DatabaseMigrationService(
            _mockFileDataService.Object,
            _mockDatabaseRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Migration")]
    public async Task MigrateFromFilesToDatabaseAsync_WithNoFiles_ReturnsZero()
    {
        // Arrange
        _mockDatabaseRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Note>());
        _mockFileDataService.Setup(x => x.LoadNotesAsync()).ReturnsAsync(new List<Note>());

        // Act
        var result = await _migrationService.MigrateFromFilesToDatabaseAsync();

        // Assert
        Assert.Equal(0, result);
        _mockDatabaseRepository.Verify(x => x.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Migration")]
    public async Task MigrateFromFilesToDatabaseAsync_WithFiles_MigratesSuccessfully()
    {
        // Arrange
        var fileNotes = new List<Note>
        {
            new() { Id = "1", Text = "Note 1", Filename = "note1.txt" },
            new() { Id = "2", Text = "Note 2", Filename = "note2.txt" }
        };

        _mockDatabaseRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Note>());
        _mockFileDataService.Setup(x => x.LoadNotesAsync()).ReturnsAsync(fileNotes);
        _mockDatabaseRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Note?)null);
        _mockDatabaseRepository.Setup(x => x.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>())).ReturnsAsync((Note n, CancellationToken ct) => n);

        // Act
        var result = await _migrationService.MigrateFromFilesToDatabaseAsync();

        // Assert
        Assert.Equal(2, result);
        _mockDatabaseRepository.Verify(x => x.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Migration")]
    public async Task MigrateFromFilesToDatabaseAsync_WithExistingNotes_SkipsDuplicates()
    {
        // Arrange
        var fileNotes = new List<Note>
        {
            new() { Id = "1", Text = "Note 1", Filename = "note1.txt" },
            new() { Id = "2", Text = "Note 2", Filename = "note2.txt" }
        };

        var existingNote = new Note { Id = "1", Text = "Existing Note 1", Filename = "existing1.txt" };

        _mockDatabaseRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Note>());
        _mockFileDataService.Setup(x => x.LoadNotesAsync()).ReturnsAsync(fileNotes);
        _mockDatabaseRepository.Setup(x => x.GetByIdAsync("1", It.IsAny<CancellationToken>())).ReturnsAsync(existingNote);
        _mockDatabaseRepository.Setup(x => x.GetByIdAsync("2", It.IsAny<CancellationToken>())).ReturnsAsync((Note?)null);
        _mockDatabaseRepository.Setup(x => x.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>())).ReturnsAsync((Note n, CancellationToken ct) => n);

        // Act
        var result = await _migrationService.MigrateFromFilesToDatabaseAsync();

        // Assert
        Assert.Equal(1, result); // Only one note should be migrated
        _mockDatabaseRepository.Verify(x => x.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Migration")]
    public async Task MigrateFromFilesToDatabaseAsync_GeneratesIdAndFilename_WhenMissing()
    {
        // Arrange
        var fileNotes = new List<Note>
        {
            new() { Text = "Note without ID or filename" }
        };

        _mockDatabaseRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Note>());
        _mockFileDataService.Setup(x => x.LoadNotesAsync()).ReturnsAsync(fileNotes);
        _mockDatabaseRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Note?)null);
        _mockDatabaseRepository.Setup(x => x.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>())).ReturnsAsync((Note n, CancellationToken ct) => n);

        // Act
        var result = await _migrationService.MigrateFromFilesToDatabaseAsync();

        // Assert
        Assert.Equal(1, result);
        _mockDatabaseRepository.Verify(x => x.AddAsync(It.Is<Note>(n => 
            !string.IsNullOrEmpty(n.Id) && !string.IsNullOrEmpty(n.Filename)), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Migration")]
    public async Task HasMigrationBeenRunAsync_WithExistingNotes_ReturnsTrue()
    {
        // Arrange
        var existingNotes = new List<Note> { new() { Id = "1", Text = "Existing note" } };
        _mockDatabaseRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(existingNotes);

        // Act
        var result = await _migrationService.HasMigrationBeenRunAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Migration")]
    public async Task HasMigrationBeenRunAsync_WithNoNotes_ReturnsFalse()
    {
        // Arrange
        _mockDatabaseRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Note>());

        // Act
        var result = await _migrationService.HasMigrationBeenRunAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Migration")]
    public async Task MigrateFromFilesToDatabaseAsync_AlreadyMigrated_ReturnsZero()
    {
        // Arrange
        var existingNotes = new List<Note> { new() { Id = "1", Text = "Existing note" } };
        _mockDatabaseRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(existingNotes);

        // Act
        var result = await _migrationService.MigrateFromFilesToDatabaseAsync();

        // Assert
        Assert.Equal(0, result);
        _mockFileDataService.Verify(x => x.LoadNotesAsync(), Times.Never);
        _mockDatabaseRepository.Verify(x => x.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
