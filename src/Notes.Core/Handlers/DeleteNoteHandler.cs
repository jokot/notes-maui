namespace Notes.Core.Handlers;

public class DeleteNoteHandler
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

    public async Task<bool> HandleAsync(DeleteNoteCommand command)
    {
        try
        {
            if (command.Note == null)
            {
                _logger.LogWarning("Delete command received with null note");
                return false;
            }

            await _noteRepository.DeleteAsync(command.Note.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete note: {FileName}", command.Note?.Filename ?? "unknown");
            throw;
        }
    }
} 