namespace Notes.Maui.ViewModels.Features.Notes;

public partial class AllNotesViewModel : BaseViewModel
{
    [ObservableProperty]
    bool isRefreshing;
    public ObservableCollection<Note> Notes { get; } = [];

    private readonly GetAllNotesHandler _getAllNotesHandler;
    private readonly RefreshNotesHandler _refreshNotesHandler;

    public AllNotesViewModel(
        GetAllNotesHandler getAllNotesHandler, 
        RefreshNotesHandler refreshNotesHandler,
        INavigationService navigationService, 
        ILogger<AllNotesViewModel> logger)
        : base(navigationService, logger)
    {
        Title = "Your Notes";
        _getAllNotesHandler = getAllNotesHandler;
        _refreshNotesHandler = refreshNotesHandler;
    }

    public async Task GetNotesAsync()
    {
        await ExecuteAsync(async () =>
        {
            // Create command to get all notes
            var command = new GetAllNotesCommand();
            
            // Use handler to process the command
            var notes = await _getAllNotesHandler.HandleAsync(command);
            
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
            
            // Create command for forced refresh
            var command = new RefreshNotesCommand();
            
            // Use dedicated refresh handler to bypass cache
            var notes = await _refreshNotesHandler.HandleAsync(command);
            
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
            { nameof(Note), note }
        });
    }
} 