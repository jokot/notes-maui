namespace Notes.Core.Tests.Queries;

public class GetAllNotesQueryTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Queries")]
    public void Constructor_CreatesQuerySuccessfully()
    {
        // Act
        var query = new GetAllNotesQuery();

        // Assert
        Assert.NotNull(query);
        Assert.IsAssignableFrom<IRequest<IEnumerable<Note>>>(query);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Queries")]
    public void Query_ImplementsCorrectInterface()
    {
        // Arrange & Act
        var query = new GetAllNotesQuery();

        // Assert
        Assert.True(query is IRequest<IEnumerable<Note>>);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Queries")]
    public void Query_IsRecord()
    {
        // Arrange & Act
        var query1 = new GetAllNotesQuery();
        var query2 = new GetAllNotesQuery();

        // Assert - Records should have value equality
        Assert.Equal(query1, query2);
        Assert.True(query1.Equals(query2));
        Assert.Equal(query1.GetHashCode(), query2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Category", "Fast")]
    [Trait("Category", "Queries")]
    public void Query_ToString_ReturnsCorrectFormat()
    {
        // Arrange & Act
        var query = new GetAllNotesQuery();
        var stringRepresentation = query.ToString();

        // Assert
        Assert.Contains("GetAllNotesQuery", stringRepresentation);
    }
}
