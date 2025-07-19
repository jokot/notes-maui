namespace Notes.Core.Handlers;

public class GetAllNotesHandler
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

    public async Task<IEnumerable<Note>> HandleAsync(GetAllNotesCommand command)
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