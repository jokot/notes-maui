namespace Notes.Maui.ViewModels.Features.Notes;

[QueryProperty(nameof(Note), nameof(Note))]
[QueryProperty(nameof(IsEdit), "IsEdit")]
public partial class NoteViewModel : BaseViewModel
{
    private readonly IMediator _mediator;

    private readonly string _initialNoteId;

    public NoteViewModel(
        IMediator mediator,
        INavigationService navigationService, 
        ILogger<NoteViewModel> logger)
        : base(navigationService, logger)
    {
        Title = "Note";
        _mediator = mediator;
        note = new Note { Title = "Untitled" };
        _initialNoteId = note.Id; // Store the ID of the initial note
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditMode))]
    Note? note;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditMode))]
    bool isEdit;

    public bool IsEditMode 
    {
        get 
        {
            // Show delete button only when:
            // 1. IsEdit is true (we're in edit mode)
            // 2. Note is not null
            // 3. The note is not the initial note we created in constructor (it's a real note from the database)
            return IsEdit && Note != null && Note.Id != _initialNoteId;
        }
    }

    [RelayCommand]
    async Task SaveNote()
    {
        await ExecuteAsync(async () =>
        {
            if (Note != null && !string.IsNullOrWhiteSpace(Note.Text))
            {
                // Ensure note has a title
                if (string.IsNullOrWhiteSpace(Note.Title))
                {
                    // Generate title from first line of text or provide default
                    var firstLine = Note.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                    Note.Title = !string.IsNullOrWhiteSpace(firstLine) && firstLine.Length <= 50 
                        ? firstLine.Trim() 
                        : firstLine?.Substring(0, Math.Min(50, firstLine.Length)).Trim() + "..." ?? "Untitled";
                }

                // Create command with the note object
                var command = new SaveNoteCommand(Note);

                // Use MediatR to send the command
                var savedNote = await _mediator.Send(command);
                
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

                // Use MediatR to send the command
                var success = await _mediator.Send(command);
                
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