namespace Notes.Core.Handlers;

public class DeleteNoteHandler : IRequestHandler<DeleteNoteCommand, bool>
{
    private readonly IRepository<Note> _noteRepository;
    private readonly ILogger<DeleteNoteHandler> _logger;

    public DeleteNoteHandler(
        IRepository<Note> noteRepository,
        ILogger<DeleteNoteHandler> logger)
    {
        _noteRepository = noteRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Note == null)
            {
                _logger.LogWarning("Cannot delete note: Note is null");
                return false;
            }

            _logger.LogDebug("Deleting note with ID: {Id}", request.Note.Id);
            var result = await _noteRepository.DeleteAsync(request.Note.Id, cancellationToken);
            
            if (result)
            {
                _logger.LogInformation("Successfully deleted note with ID: {Id}", request.Note.Id);
            }
            else
            {
                _logger.LogWarning("Note with ID: {Id} was not found for deletion", request.Note.Id);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete note with ID: {Id}", request.Note?.Id ?? "null");
            throw;
        }
    }
} 