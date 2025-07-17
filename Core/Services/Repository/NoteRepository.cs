namespace Notes.Core.Services.Repository;

public class NoteRepository : BaseRepository<Note>
{
    private readonly IFileDataService _fileDataService;
    private List<Note> _cacheNotes = [];
    private bool _cacheLoaded = false;
    private DateTime _lastCacheTime = DateTime.MinValue;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(AppConstants.Cache.DefaultTimeoutMinutes);

    public NoteRepository(IFileDataService fileDataService, ILogger<NoteRepository> logger)
        : base(fileDataService, logger)
    {
        _fileDataService = fileDataService;
    }

    public override async Task<IEnumerable<Note>> GetAllAsync()
    {
        if (_cacheLoaded || DateTime.Now - _lastCacheTime > _cacheTimeout)
        {
            await RefreshNotesCache();
        }

        return _cacheNotes;
    }

    public override async Task<IEnumerable<Note>> GetAllForceAsync()
    {
        await RefreshNotesCache();

        return _cacheNotes;
    }

    public override async Task<Note?> GetByIdAsync(string id)
    {
        return _cacheNotes.FirstOrDefault(n => n.Id == id);
    }

    public override async Task<Note> AddAsync(Note entity)
    {
        await _fileDataService.SaveNoteAsync(entity.Filename, entity.Text);
        _cacheNotes.Add(entity);
        _cacheNotes = [.. _cacheNotes.OrderByDescending(note => note.Date)];
        return entity;
    }

    public override async Task<Note> UpdateAsync(Note entity)
    {
        await _fileDataService.SaveNoteAsync(entity.Filename, entity.Text);
        var existingNote = _cacheNotes.FirstOrDefault(n => n.Id == entity.Id);
        if (existingNote != null)
        {
            existingNote.Text = entity.Text;
            existingNote.Date = DateTime.Now;
            existingNote.UpdatedAt = DateTime.Now;
        }
        _cacheNotes = [.. _cacheNotes.OrderByDescending(note => note.Date)];
        return entity;
    }

    public override async Task DeleteAsync(string id)
    {
        var note = await GetByIdAsync(id);
        if (note != null)
        {
            await _fileDataService.DeleteNoteAsync(note.Filename);
            _cacheNotes.RemoveAll(n => n.Id == id);
        }
    }

    public override async Task<bool> ExitsAsync(string id)
    {
        var note = await GetByIdAsync(id);
        return note != null;
    }

    private async Task RefreshNotesCache()
    {
        try
        {
            _cacheNotes = await _fileDataService.LoadNotesAsync();
            _cacheLoaded = true;
            _lastCacheTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error refreshing notes cache");
            _cacheNotes.Clear();
        }
    }
}