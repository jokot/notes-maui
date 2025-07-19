namespace Notes.Core.Handlers;

public class SaveNoteHandler
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

    public async Task<Note> HandleAsync(SaveNoteCommand command)
    {
        try
        {
            var filename = GenerateFilename(command);
            
            var note = new Note
            {
                Filename = filename,
                Text = command.Text,
                Date = command.Date
            };

            // Check if this is an update (filename provided) or new note
            if (!string.IsNullOrEmpty(command.FileName) && await _fileDataService.NoteExistsAsync(command.FileName))
            {
                note.Filename = command.FileName;
                var result = await _noteRepository.UpdateAsync(note);
                _logger.LogInformation("Note updated: {Filename}", note.Filename);
                return result;
            }
            else
            {
                var result = await _noteRepository.AddAsync(note);
                _logger.LogInformation("Note created: {Filename}", note.Filename);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save note: {Title}", command.Title);
            throw;
        }
    }

    private string GenerateFilename(SaveNoteCommand command)
    {
        if (!string.IsNullOrEmpty(command.FileName))
        {
            return command.FileName;
        }

        // Use the file data service for filename generation
        return _fileDataService.GenerateFilename(command.Title);
    }
} 