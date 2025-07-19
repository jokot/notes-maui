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
                // Create command with note data
                var command = new SaveNoteCommand(
                    title: ExtractTitle(Note.Text), 
                    text: Note.Text, 
                    fileName: Note.Filename);

                // Use handler to process the command
                var savedNote = await _saveNoteHandler.HandleAsync(command);
                
                // Update the current note with saved data
                Note = savedNote;
            }
            await NavigationService.GoBackAsync();
        }, nameof(SaveNote));
    }

    private static string ExtractTitle(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "Untitled";
            
        // Take first line or first 50 characters as title
        var firstLine = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
        return firstLine.Length > 50 ? firstLine[..50] + "..." : firstLine;
    }

    [RelayCommand]
    async Task DeleteNote()
    {
        await ExecuteAsync(async () =>
        {
            if (Note != null && !string.IsNullOrEmpty(Note.Filename))
            {
                // Create command with filename to delete
                var command = new DeleteNoteCommand(Note.Filename);

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