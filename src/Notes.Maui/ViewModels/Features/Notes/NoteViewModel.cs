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