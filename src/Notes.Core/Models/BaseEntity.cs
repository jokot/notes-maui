namespace Notes.Core.Models;

public abstract class BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
