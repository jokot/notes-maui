namespace Notes.Core.Handlers;

public class SaveNoteHandler : IRequestHandler<SaveNoteCommand, Note>
{
    private readonly IRepository<Note> _noteRepository;
    private readonly IFileDataService _fileDataService;
    private readonly ILogger<SaveNoteHandler> _logger;

    public SaveNoteHandler(
        IRepository<Note> noteRepository,
        IFileDataService fileDataService,
        ILogger<SaveNoteHandler> logger)
    {
        _noteRepository = noteRepository;
        _fileDataService = fileDataService;
        _logger = logger;
    }

    public async Task<Note> Handle(SaveNoteCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var note = command.Note;

            if (!string.IsNullOrEmpty(note.Filename) && await _fileDataService.NoteExistsAsync(note.Filename))
            {
                var result = await _noteRepository.UpdateAsync(note);
                return result;
            }
            else
            {
                if (string.IsNullOrEmpty(note.Filename))
                {
                    note.Filename = _fileDataService.GenerateFilename();
                }
                
                var result = await _noteRepository.AddAsync(note);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save note: {Text}", command.Note.Text[..Math.Min(50, command.Note.Text.Length)]);
            throw;
        }
    }
} 