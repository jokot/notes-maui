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

    public async Task<IEnumerable<Note>> Handle(GetAllNotesQuery query, CancellationToken cancellationToken)
    {
        try
        {
            return await _noteRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve notes");
            throw;
        }
    }
} 