namespace Notes.Core.Tests.Commands;

public class SaveNoteCommandTests
{
    [Fact]
    public void Constructor_WithParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        var title = "Test Title";
        var text = "Test Content";
        var fileName = "test-file.txt";

        // Act
        var command = new SaveNoteCommand(title, text, fileName);

        // Assert
        Assert.Equal(title, command.Title);
        Assert.Equal(text, command.Text);
        Assert.Equal(fileName, command.FileName);
        Assert.True(command.Date <= DateTime.Now);
        Assert.True(command.Date >= DateTime.Now.AddSeconds(-1));
    }

    [Fact]
    public void Constructor_WithoutFileName_SetsPropertiesCorrectly()
    {
        // Arrange
        var title = "Test Title";
        var text = "Test Content";

        // Act
        var command = new SaveNoteCommand(title, text);

        // Assert
        Assert.Equal(title, command.Title);
        Assert.Equal(text, command.Text);
        Assert.Null(command.FileName);
        Assert.True(command.Date <= DateTime.Now);
        Assert.True(command.Date >= DateTime.Now.AddSeconds(-1));
    }

    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var command = new SaveNoteCommand();

        // Assert
        Assert.Equal(string.Empty, command.Title);
        Assert.Equal(string.Empty, command.Text);
        Assert.Null(command.FileName);
        Assert.True(command.Date <= DateTime.Now);
        Assert.True(command.Date >= DateTime.Now.AddSeconds(-1));
    }
} 