namespace Notes.Services;

public class NoteService
{
    private List<Note> _cacheNotes = [];
    private bool _cacheLoaded = false;
    private DateTime _lastCacheTime = DateTime.MinValue;
    private readonly TimeSpan _cacheTimout = TimeSpan.FromMinutes(5);

    public async Task<List<Note>> GetNotes()
    {
        if (!_cacheLoaded || DateTime.Now - _lastCacheTime > _cacheTimout)
        {
            await RefreshNotesCache();
        }

        return _cacheNotes;
    }

    public async Task<List<Note>> ForceRefreshNotes()
    {
        await RefreshNotesCache();
        return _cacheNotes;
    }

    private async Task RefreshNotesCache()
    {
        _cacheNotes.Clear();

        // Get the folder where the notes are stored.
        string appDataPath = FileSystem.AppDataDirectory;
        // Use Linq extensions to load the *.notes.txt files.
        var files = Directory.EnumerateFiles(appDataPath, "*.notes.txt");

        foreach (var file in files)
        {
            string text = await File.ReadAllTextAsync(file);
            DateTime date = File.GetLastWriteTime(file);

            _cacheNotes.Add(new Note
            {
                Filename = file,
                Text = text,
                Date = date
            });
        }
        _cacheNotes = [.. _cacheNotes.OrderByDescending(note => note.Date)];
        _cacheLoaded = true;
        _lastCacheTime = DateTime.Now;
    }

    private void UpdateCacheForSavedNote(string filename, string text)
    {
        var existingNote = _cacheNotes.FirstOrDefault(n => n.Filename == filename);
        if (existingNote != null)
        {
            existingNote.Text = text;
            existingNote.Date = DateTime.Now;
        }
        else
        {
            _cacheNotes.Add(new Note
            {
                Filename = filename,
                Text = text,
                Date = DateTime.Now
            });
        }

        _cacheNotes = [.. _cacheNotes.OrderByDescending(note => note.Date)];
    }

    public async Task SaveNote(string filename, string text)
    {
        await File.WriteAllTextAsync(filename, text);
        UpdateCacheForSavedNote(filename, text);
    }

    public async Task DeleteNote(string filename)
    {
        if (File.Exists(filename))
        {
            await Task.Run(() => File.Delete(filename));

            _cacheNotes?.RemoveAll(n => n.Filename == filename);
        }
    }
}
