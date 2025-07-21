namespace Notes.Maui.ViewModels.Features.Notes;

[QueryProperty(nameof(Note), nameof(Note))]
public partial class NoteViewModel : BaseViewModel
{
    private readonly SaveNoteHandler _saveNoteHandler;
    private readonly DeleteNoteHandler _deleteNoteHandler;

    public NoteViewModel(
        SaveNoteHandler saveNoteHandler, 
        DeleteNoteHandler deleteNoteHandler,
        INavigationService navigationService, 
        ILogger<NoteViewModel> logger)
        : base(navigationService, logger)
    {
        Title = "Note";
        _saveNoteHandler = saveNoteHandler;
        _deleteNoteHandler = deleteNoteHandler;
        note = new Note();
    }

    [ObservableProperty]
    Note? note;

    [RelayCommand]
    async Task SaveNote()
    {
        await ExecuteAsync(async () =>
        {
            if (Note != null && !string.IsNullOrWhiteSpace(Note.Text))
            {
                // Create command with the note object
                var command = new SaveNoteCommand(Note);

                // Use handler to process the command
                var savedNote = await _saveNoteHandler.HandleAsync(command);
                
                // Update the current note with saved data
                Note = savedNote;
            }
            await NavigationService.GoBackAsync();
        }, nameof(SaveNote));
    }

    [RelayCommand]
    async Task DeleteNote()
    {
        await ExecuteAsync(async () =>
        {
            if (Note != null)
            {
                // Create command with the full Note object
                var command = new DeleteNoteCommand(Note);

                // Use handler to process the command
                var success = await _deleteNoteHandler.HandleAsync(command);
                
                if (success)
                {
                    Logger.LogInformation("Note deleted successfully: {Filename}", Note.Filename);
                }
                else
                {
                    Logger.LogWarning("Failed to delete note: {Filename}", Note.Filename);
                }
            }
            await NavigationService.GoBackAsync();
        }, nameof(DeleteNote));
    }
} 