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
            if (string.IsNullOrEmpty(command.FileName))
            {
                _logger.LogWarning("Delete command received with empty filename");
                return false;
            }

            // Find the note by filename to get its ID
            var allNotes = await _noteRepository.GetAllAsync();
            var noteToDelete = allNotes.FirstOrDefault(n => n.Filename == command.FileName);

            if (noteToDelete == null)
            {
                _logger.LogWarning("Note not found for deletion: {FileName}", command.FileName);
                return false;
            }

            await _noteRepository.DeleteAsync(noteToDelete.Id);
            _logger.LogInformation("Note deleted: {FileName}", command.FileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete note: {FileName}", command.FileName);
            throw;
        }
    }
} 