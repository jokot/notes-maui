namespace Notes.Models;

public class Note
{
    public string Filename { get; set; }
    public string Text { get; set; }
    public DateTime Date { get; set; }

    public Note()
    {
        Filename = GenerateUniqueFilename();
        Text = string.Empty;
        Date = DateTime.Now;
    }

    private static string GenerateUniqueFilename()
    {
        string appDataPath = FileSystem.AppDataDirectory;
        string randomFileName = $"{Path.GetRandomFileName()}.notes.txt";
        return Path.Combine(appDataPath, randomFileName);
    }
}
