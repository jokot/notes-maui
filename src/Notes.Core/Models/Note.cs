namespace Notes.Core.Models;

public class Note : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsPinned { get; set; } = false;
    public string BackgroundColor { get; set; } = "#FFFFFF"; // Default to white
    
    // Navigation property for many-to-many relationship
    public virtual ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
}
