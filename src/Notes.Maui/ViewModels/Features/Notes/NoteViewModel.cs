namespace Notes.Maui.ViewModels.Features.Notes;

[QueryProperty(nameof(Note), nameof(Note))]
public partial class NoteViewModel : BaseViewModel
{
    private readonly IMediator _mediator;

    public NoteViewModel(
        IMediator mediator,
        INavigationService navigationService, 
        ILogger<NoteViewModel> logger)
        : base(navigationService, logger)
    {
        Title = "Note";
        _mediator = mediator;
        note = new Note { Title = "Untitled" };
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