using Microsoft.EntityFrameworkCore;
using Notes.Core.Data;

namespace Notes.Core.Tests.Services.Repository;

public class SqliteNoteRepositoryTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly NotesDbContext _context;
    private readonly SqliteNoteRepository _repository;
    private readonly ILogger<SqliteNoteRepository> _logger;

    public SqliteNoteRepositoryTests()
    {
        // Create a unique temporary database for each test
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test-notes-{Guid.NewGuid()}.db");
        
        var options = new DbContextOptionsBuilder<NotesDbContext>()
            .UseSqlite($"Data Source={_testDatabasePath}")
            .Options;

        _context = new NotesDbContext(options);
        _context.Database.EnsureCreated();

        // Create logger mock
        var loggerMock = new Mock<ILogger<SqliteNoteRepository>>();
        _logger = loggerMock.Object;

        _repository = new SqliteNoteRepository(_context, _logger);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Repository")]
    public async Task AddAsync_WithValidNote_ReturnsNoteWithId()
    {
        // Arrange
        var note = new Note
        {
            Text = "Test note content",
            Filename = "test.notes.txt",
            UpdatedAt = DateTime.Now
        };

        // Act
        var result = await _repository.AddAsync(note);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Equal("Test note content", result.Text);
        Assert.Equal("test.notes.txt", result.Filename);
        
        // Verify it was actually saved to database
        var savedNote = await _context.Notes.FindAsync(result.Id);
        Assert.NotNull(savedNote);
        Assert.Equal("Test note content", savedNote.Text);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Repository")]
    public async Task GetByIdAsync_WithExistingId_ReturnsNote()
    {
        // Arrange
        var note = new Note
        {
            Id = "test-id-123",
            Text = "Find me test",
            Filename = "findme.notes.txt",
            UpdatedAt = DateTime.Now
        };
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync("test-id-123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-id-123", result.Id);
        Assert.Equal("Find me test", result.Text);
        Assert.Equal("findme.notes.txt", result.Filename);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Repository")]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync("non-existent-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Repository")]
    public async Task GetAllAsync_WithMultipleNotes_ReturnsAllNotes()
    {
        // Arrange
        var notes = new List<Note>
        {
            new() { Id = "note-1", Text = "First note", Filename = "1.notes.txt", UpdatedAt = DateTime.Now.AddMinutes(-10) },
            new() { Id = "note-2", Text = "Second note", Filename = "2.notes.txt", UpdatedAt = DateTime.Now.AddMinutes(-5) },
            new() { Id = "note-3", Text = "Third note", Filename = "3.notes.txt", UpdatedAt = DateTime.Now }
        };

        _context.Notes.AddRange(notes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count());
        
        // Should be ordered by UpdatedAt descending (most recent first)
        var orderedResult = result.ToList();
        Assert.Equal("note-3", orderedResult[0].Id); // Most recent
        Assert.Equal("note-2", orderedResult[1].Id);
        Assert.Equal("note-1", orderedResult[2].Id); // Oldest
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Repository")]
    public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmptyCollection()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Repository")]
    public async Task UpdateAsync_WithExistingNote_UpdatesSuccessfully()
    {
        // Arrange
        var originalNote = new Note
        {
            Id = "update-test",
            Text = "Original text",
            Filename = "original.notes.txt",
            UpdatedAt = DateTime.Now.AddHours(-1)
        };
        _context.Notes.Add(originalNote);
        await _context.SaveChangesAsync();

        // Modify the note
        originalNote.Text = "Updated text";
        originalNote.Filename = "updated.notes.txt";
        originalNote.UpdatedAt = DateTime.Now;

        // Act
        var result = await _repository.UpdateAsync(originalNote);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("update-test", result.Id);
        Assert.Equal("Updated text", result.Text);
        Assert.Equal("updated.notes.txt", result.Filename);

        // Verify changes were persisted
        var dbNote = await _context.Notes.FindAsync("update-test");
        Assert.NotNull(dbNote);
        Assert.Equal("Updated text", dbNote.Text);
        Assert.Equal("updated.notes.txt", dbNote.Filename);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Repository")]
    public async Task UpdateAsync_WithNonExistentNote_ThrowsException()
    {
        // Arrange
        var nonExistentNote = new Note
        {
            Id = "does-not-exist",
            Text = "This note doesn't exist",
            Filename = "nonexistent.notes.txt",
            UpdatedAt = DateTime.Now
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.UpdateAsync(nonExistentNote));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Repository")]
    public async Task DeleteAsync_WithExistingId_DeletesSuccessfully()
    {
        // Arrange
        var note = new Note
        {
            Id = "delete-me",
            Text = "This will be deleted",
            Filename = "delete.notes.txt",
            UpdatedAt = DateTime.Now
        };
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync("delete-me");

        // Assert
        Assert.True(result);
        
        // Verify it was actually deleted
        var deletedNote = await _context.Notes.FindAsync("delete-me");
        Assert.Null(deletedNote);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Repository")]
    public async Task DeleteAsync_WithNonExistentId_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteAsync("does-not-exist");

        // Assert
        Assert.False(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Repository")]
    public async Task ExistsAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        var note = new Note
        {
            Id = "exists-test",
            Text = "I exist",
            Filename = "exists.notes.txt",
            UpdatedAt = DateTime.Now
        };
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync("exists-test");

        // Assert
        Assert.True(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Repository")]
    public async Task ExistsAsync_WithNonExistentId_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync("does-not-exist");

        // Assert
        Assert.False(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Repository")]
    public async Task AddAsync_WithCancellationToken_CanBeCancelled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        var note = new Note
        {
            Text = "This operation should be cancelled",
            Filename = "cancelled.notes.txt",
            UpdatedAt = DateTime.Now
        };

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _repository.AddAsync(note, cts.Token));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Repository")]
    public async Task Repository_WithConcurrentOperations_HandlesCorrectly()
    {
        // Arrange
        var tasks = new List<Task<Note>>();
        
        // Act - Add multiple notes concurrently
        for (int i = 0; i < 10; i++)
        {
            var note = new Note
            {
                Text = $"Concurrent note {i}",
                Filename = $"concurrent-{i}.notes.txt",
                UpdatedAt = DateTime.Now
            };
            tasks.Add(_repository.AddAsync(note));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, results.Length);
        Assert.All(results, note => Assert.NotNull(note.Id));
        
        // Verify all notes were saved
        var allNotes = await _repository.GetAllAsync();
        Assert.Equal(10, allNotes.Count());
    }

    public void Dispose()
    {
        _context?.Dispose();
        
        // Clean up test database file
        if (File.Exists(_testDatabasePath))
        {
            try
            {
                File.Delete(_testDatabasePath);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }
}
