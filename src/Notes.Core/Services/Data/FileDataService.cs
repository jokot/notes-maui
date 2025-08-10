namespace Notes.Core.Services.Data;

public class FileDataService : IFileDataService
{
    private readonly ILogger<FileDataService> _logger;
    private readonly string _dataPath;

    public FileDataService(ILogger<FileDataService> logger, string? dataPath = null)
    {
        _logger = logger;
        _dataPath = dataPath ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        
        // Ensure the directory exists
        Directory.CreateDirectory(_dataPath);
    }

    public async Task<List<Note>> LoadNotesAsync()
    {
        try
        {
            var notes = new List<Note>();
            var files = Directory.EnumerateFiles(_dataPath, "*.notes.txt");

            foreach (var file in files)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(file);
                    DateTime fileTime = File.GetLastWriteTime(file);
                    
                    Note note;
                    
                    // Try to deserialize as minimal JSON first
                    try
                    {
                        var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                        if (jsonData != null && jsonData.ContainsKey("id") && jsonData.ContainsKey("text"))
                        {
                            // Parse minimal JSON format
                            note = new Note
                            {
                                Id = jsonData["id"].ToString() ?? Guid.NewGuid().ToString(),
                                Filename = file,
                                Text = jsonData["text"].ToString() ?? "",
                                UpdatedAt = jsonData.ContainsKey("updatedAt") && DateTime.TryParse(jsonData["updatedAt"].ToString(), out var parsedDate) 
                                    ? parsedDate 
                                    : fileTime
                            };
                        }
                        else
                        {
                            throw new JsonException("Invalid JSON format");
                        }
                    }
                    catch
                    {
                        // Fall back to plain text format
                        note = new Note
                        {
                            Id = Guid.NewGuid().ToString(), // Always generate an ID
                            Filename = file,
                            Text = content,
                            UpdatedAt = fileTime
                        };
                    }

                    notes.Add(note);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error reading file: {File}", file);
                }
            }

            return [.. notes.OrderByDescending(note => note.UpdatedAt)];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading notes from files");
            return [];
        }
    }

    public async Task SaveNoteAsync(Note note)
    {
        try
        {
            // Ensure we have a full path for the filename
            var fullPath = Path.IsPathRooted(note.Filename) 
                ? note.Filename 
                : Path.Combine(_dataPath, note.Filename);

            // Create minimal JSON with only essential properties
            var minimalData = new
            {
                id = note.Id,
                text = note.Text,
                updatedAt = note.UpdatedAt.ToString("O") // ISO 8601 format
            };
            
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonContent = JsonSerializer.Serialize(minimalData, options);
            await File.WriteAllTextAsync(fullPath, jsonContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving note to file: {Filename}", note.Filename);
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

    public Task<bool> NoteExistsAsync(string filename)
    {
        return Task.FromResult(File.Exists(filename));
    }

    public string GenerateFilename()
    {
        string filename = $"{DateTime.Now:yyyyMMdd-HHmmss}.notes.txt";
        return Path.Combine(_dataPath, filename);
    }
}
