namespace Notes.Core.Models;

public class Note : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsPinned { get; set; } = false;
}
