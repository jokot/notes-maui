namespace Notes.Core.Services.Data;

public class FileDataService : IFileDataService
{
    private readonly ILogger<FileDataService> _logger;

    public FileDataService(ILogger<FileDataService> logger)
    {
        _logger = logger;
    }

    public async Task<List<Note>> LoadNotesAsync()
    {
        try
        {
            var notes = new List<Note>();
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var files = Directory.EnumerateFiles(appDataPath, "*.notes.txt");

            foreach (var file in files)
            {
                try
                {
                    string text = await File.ReadAllTextAsync(file);
                    DateTime date = File.GetLastWriteTime(file);

                    notes.Add(new Note
                    {
                        Filename = file,
                        Text = text,
                        Date = date
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error reading file: {File}", file);
                }
            }

            return [.. notes.OrderByDescending(note => note.Date)];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading notes from files");
            return [];
        }
    }

    public async Task SaveNoteAsync(string filename, string text)
    {
        try
        {
            await File.WriteAllTextAsync(filename, text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving note to file: {Filename}", filename);
            throw;
        }
    }

    public async Task DeleteNoteAsync(string filename)
    {
        try
        {
            if (File.Exists(filename))
            {
                await Task.Run(() => File.Delete(filename));
                _logger.LogInformation("Note deleted successfully: {Filename}", filename);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting note file: {Filename}", filename);
            throw;
        }
    }

    public async Task<bool> NoteExistsAsync(string filename)
    {
        return File.Exists(filename);
    }

    public string GenerateUniqueFilename()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        
        // Ensure the directory exists
        Directory.CreateDirectory(appDataPath);
        
        // Get random filename and strip any path separators
        string randomName = Path.GetRandomFileName().Replace(".", "").Replace(Path.DirectorySeparatorChar.ToString(), "");
        string randomFileName = $"{randomName}.notes.txt";
        return Path.Combine(appDataPath, randomFileName);
    }

    public string GenerateFilename(string title)
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        
        // Ensure the directory exists
        Directory.CreateDirectory(appDataPath);
        
        string safeTitle = GetSafeFilename(title);
        string filename = $"{safeTitle}.{DateTime.Now:yyyyMMddHHmmss}.notes.txt";
        return Path.Combine(appDataPath, filename);
    }

    private static string GetSafeFilename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "untitled";
        }

        // Remove invalid characters and limit length
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = new string(title.Where(c => !invalidChars.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(safeName) ? "untitled" : safeName.Substring(0, Math.Min(safeName.Length, 50));
    }
}
