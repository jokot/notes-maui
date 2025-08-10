namespace Notes.Core.Models;

public class NoteTag : BaseEntity
{
    public string NoteId { get; set; } = string.Empty;
    public string TagId { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Note Note { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}