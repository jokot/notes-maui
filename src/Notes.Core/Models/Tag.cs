namespace Notes.Core.Models;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#007ACC"; // Default blue color
    
    // Navigation property for many-to-many relationship
    public virtual ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
}