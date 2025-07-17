namespace Notes.Core.ViewModels;

public partial class AllNotesViewModel : BaseViewModel
{
    [ObservableProperty]
    bool isRefreshing;
    public ObservableCollection<Note> Notes { get; } = [];

    private readonly IRepository<Note> _noteRepository;

    public AllNotesViewModel(IRepository<Note> noteRepository, INavigationService navigationService, ILogger<AllNotesViewModel> logger)
        : base(navigationService, logger)
    {
        Title = "Your Notes";
        _noteRepository = noteRepository;
    }

    public async Task GetNotesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var notes = await _noteRepository.GetAllAsync();
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
            
            var notes = await _noteRepository.GetAllForceAsync();
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
