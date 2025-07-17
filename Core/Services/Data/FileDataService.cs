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
            string appDataPath = FileSystem.AppDataDirectory;
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
}
