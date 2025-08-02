namespace Notes.Core.Handlers;

public class GetAllNotesHandler : IRequestHandler<GetAllNotesQuery, IEnumerable<Note>>
{
    private readonly IRepository<Note> _noteRepository;
    private readonly ILogger<GetAllNotesHandler> _logger;

    public GetAllNotesHandler(
        IRepository<Note> noteRepository,
        ILogger<GetAllNotesHandler> logger)
    {
        _noteRepository = noteRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Note>> Handle(GetAllNotesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting all notes from repository");
            var notes = await _noteRepository.GetAllAsync(cancellationToken);
            _logger.LogInformation("Retrieved {Count} notes", notes.Count());
            return notes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notes");
            throw;
        }
    }
} 