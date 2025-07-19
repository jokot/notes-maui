namespace Notes.Core.Handlers;

public class RefreshNotesHandler
{
    private readonly IRepository<Note> _noteRepository;
    private readonly ILogger<RefreshNotesHandler> _logger;

    public RefreshNotesHandler(
        IRepository<Note> noteRepository,
        ILogger<RefreshNotesHandler> logger)
    {
        _noteRepository = noteRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Note>> HandleAsync(RefreshNotesCommand command)
    {
        try
        {
            return await _noteRepository.GetAllForceAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh notes from storage");
            throw;
        }
    }
} 