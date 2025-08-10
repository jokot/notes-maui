namespace Notes.Core.Handlers;

public class SaveNoteHandler : IRequestHandler<SaveNoteCommand, Note>
{
    private readonly IRepository<Note> _noteRepository;
    private readonly ILogger<SaveNoteHandler> _logger;

    public SaveNoteHandler(
        IRepository<Note> noteRepository,
        ILogger<SaveNoteHandler> logger)
    {
        _noteRepository = noteRepository;
        _logger = logger;
    }

    public async Task<Note> Handle(SaveNoteCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var note = command.Note;

            // Check if this is an update (note has an ID and exists in database) or a new note
            if (!string.IsNullOrEmpty(note.Id))
            {
                var existingNote = await _noteRepository.GetByIdAsync(note.Id, cancellationToken);
                if (existingNote != null)
                {
                    // Update existing note
                    var result = await _noteRepository.UpdateAsync(note, cancellationToken);
                    _logger.LogInformation("Updated note with ID: {Id}", note.Id);
                    return result;
                }
            }
            
            // Create new note
            var savedNote = await _noteRepository.AddAsync(note, cancellationToken);
            _logger.LogInformation("Created new note with ID: {Id}", savedNote.Id);
            return savedNote;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save note: {Title}", command.Note.Title ?? command.Note.Text[..Math.Min(50, command.Note.Text.Length)]);
            throw;
        }
    }
}