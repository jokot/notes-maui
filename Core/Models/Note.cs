namespace Notes.Core.Models;

public class Note : BaseEntity
{
    public string Filename { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;

    public Note()
    {
        Filename = GenerateUniqueFilename();
    }

    private static string GenerateUniqueFilename()
    {
        string appDataPath = FileSystem.AppDataDirectory;
        string randomFileName = $"{Path.GetRandomFileName()}.notes.txt";
        return Path.Combine(appDataPath, randomFileName);
    }
}
