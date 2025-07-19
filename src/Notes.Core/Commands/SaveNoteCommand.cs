namespace Notes.Core.Commands;

public class SaveNoteCommand
{
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? FileName { get; set; }

    public SaveNoteCommand()
    {
        Date = DateTime.Now;
    }

    public SaveNoteCommand(string title, string text, string? fileName = null)
    {
        Title = title;
        Text = text;
        FileName = fileName;
        Date = DateTime.Now;
    }
} 