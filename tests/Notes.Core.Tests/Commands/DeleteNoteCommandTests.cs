namespace Notes.Core.Tests.Commands;

public class DeleteNoteCommandTests
{
    [Fact]
    public void Constructor_WithFileName_SetsFileNameCorrectly()
    {
        // Arrange
        var fileName = "test-file.notes.txt";

        // Act
        var command = new DeleteNoteCommand(fileName);

        // Assert
        Assert.Equal(fileName, command.FileName);
    }

    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var command = new DeleteNoteCommand();

        // Assert
        Assert.Equal(string.Empty, command.FileName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidFileName_SetsFileName(string? fileName)
    {
        // Act
        var command = new DeleteNoteCommand(fileName);

        // Assert
        Assert.Equal(fileName, command.FileName);
    }
} 