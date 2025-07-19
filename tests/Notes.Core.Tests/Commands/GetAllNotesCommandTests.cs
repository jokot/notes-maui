namespace Notes.Core.Tests.Commands;

public class GetAllNotesCommandTests
{
    [Fact]
    public void Constructor_CreatesInstanceSuccessfully()
    {
        // Act
        var command = new GetAllNotesCommand();

        // Assert
        Assert.NotNull(command);
    }

    [Fact]
    public void Command_IsInstantiable()
    {
        // This test ensures the command class can be created
        // Since it's an empty command with no properties, we just verify instantiation
        
        // Act
        var command = new GetAllNotesCommand();
        
        // Assert
        Assert.NotNull(command);
    }
} 