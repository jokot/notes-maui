namespace Notes.Core.Handlers;

public class RefreshNotesHandler : IRequestHandler<RefreshNotesQuery, IEnumerable<Note>>
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

    public async Task<IEnumerable<Note>> Handle(RefreshNotesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Refreshing notes from repository");
            var notes = await _noteRepository.GetAllAsync(cancellationToken);
            _logger.LogInformation("Refreshed {Count} notes", notes.Count());
            return notes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh notes");
            throw;
        }
    }
} 