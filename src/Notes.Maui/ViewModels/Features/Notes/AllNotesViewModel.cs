namespace Notes.Maui.ViewModels.Features.Notes;

public partial class AllNotesViewModel : BaseViewModel
{
    [ObservableProperty]
    bool isRefreshing;
    public ObservableCollection<Note> Notes { get; } = [];

    private readonly IMediator _mediator;

    public AllNotesViewModel(
        IMediator mediator,
        INavigationService navigationService, 
        ILogger<AllNotesViewModel> logger)
        : base(navigationService, logger)
    {
        Title = "Your Notes";
        _mediator = mediator;
    }

    public async Task GetNotesAsync()
    {
        await ExecuteAsync(async () =>
        {
            // Use MediatR to send query
            var notes = await _mediator.Send(new GetAllNotesQuery());
            
            Notes.Clear();
            foreach (var note in notes)
                Notes.Add(note);
        }, nameof(GetNotesAsync));
    }

    [RelayCommand]
    async Task RefreshNotes()
    {
        await ExecuteAsync(async () =>
        {
            IsRefreshing = true;
            
            // Use MediatR to send refresh query
            await _mediator.Send(new RefreshNotesQuery());
            
            // Then get all notes to update the UI
            var notes = await _mediator.Send(new GetAllNotesQuery());
            
            Notes.Clear();
            foreach (var note in notes)
                Notes.Add(note);

            IsRefreshing = false;
        }, nameof(RefreshNotes));
    }

    [RelayCommand]
    async Task AddNote()
    {
        await NavigationService.NavigateToAsync(AppConstants.Navigation.NotePage);
    }

    [RelayCommand]
    async Task GoToNote(Note note)
    {
        if (note == null) return;

        await NavigationService.NavigateToAsync(AppConstants.Navigation.NotePage, new Dictionary<string, object>
        {
            { nameof(Note), note },
            { "IsEdit", true }
        });
    }
}