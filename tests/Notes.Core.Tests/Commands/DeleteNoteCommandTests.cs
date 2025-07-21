namespace Notes.Core.Tests.Commands;

public class DeleteNoteCommandTests
{
    [Fact]
    public void Constructor_WithNote_SetsNoteCorrectly()
    {
        // Arrange
        var note = new Note 
        { 
            Id = "123", 
            Filename = "test-file.notes.txt", 
            Text = "Test content",
            UpdatedAt = DateTime.Now
        };

        // Act
        var command = new DeleteNoteCommand(note);

        // Assert
        Assert.Equal(note, command.Note);
        Assert.Equal("test-file.notes.txt", command.Note.Filename);
        Assert.Equal("123", command.Note.Id);
        Assert.Equal("Test content", command.Note.Text);
    }

    [Fact]
    public void Constructor_WithNullNote_SetsNoteToNull()
    {
        // Act
        var command = new DeleteNoteCommand(null!);

        // Assert
        Assert.Null(command.Note);
    }

    [Fact]
    public void Constructor_WithValidNote_AllPropertiesAccessible()
    {
        // Arrange
        var note = new Note 
        { 
            Id = "456", 
            Filename = "another-test.notes.txt", 
            Text = "Another test content",
            UpdatedAt = DateTime.Now
        };

        // Act
        var command = new DeleteNoteCommand(note);

        // Assert
        Assert.NotNull(command.Note);
        Assert.Equal("456", command.Note.Id);
        Assert.Equal("another-test.notes.txt", command.Note.Filename);
        Assert.Equal("Another test content", command.Note.Text);
    }
} 