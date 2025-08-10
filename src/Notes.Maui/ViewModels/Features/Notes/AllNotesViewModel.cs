namespace Notes.Maui.ViewModels.Features.Notes;

public partial class AllNotesViewModel : BaseViewModel
{
    [ObservableProperty]
    bool isRefreshing;
    
    [ObservableProperty]
    string searchText = string.Empty;
    
    public ObservableCollection<Note> Notes { get; } = [];
    public ObservableCollection<Note> FilteredNotes { get; } = [];

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
    
    partial void OnSearchTextChanged(string value)
    {
        FilterNotes();
    }
    
    private void FilterNotes()
    {
        FilteredNotes.Clear();
        
        IEnumerable<Note> notesToFilter;
        
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            notesToFilter = Notes;
        }
        else
        {
            var searchTerm = SearchText.ToLowerInvariant();
            notesToFilter = Notes.Where(note => 
                note.Title.ToLowerInvariant().Contains(searchTerm) ||
                note.Text.ToLowerInvariant().Contains(searchTerm));
        }
        
        // Sort notes: pinned notes first, then by UpdatedAt descending
        var sortedNotes = notesToFilter
            .OrderByDescending(note => note.IsPinned)
            .ThenByDescending(note => note.UpdatedAt);
            
        foreach (var note in sortedNotes)
            FilteredNotes.Add(note);
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
                
            FilterNotes();
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
                
            FilterNotes();

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
    
    [RelayCommand]
    async Task DeleteNote(Note note)
    {
        if (note == null) return;
        
        await ExecuteAsync(async () =>
        {
            var command = new DeleteNoteCommand(note);
            var success = await _mediator.Send(command);
            
            if (success)
            {
                Notes.Remove(note);
                FilteredNotes.Remove(note);
            }
        }, nameof(DeleteNote));
    }
    
    [RelayCommand]
    async Task TogglePin(Note note)
    {
        if (note == null) return;
        
        await ExecuteAsync(async () =>
        {
            note.IsPinned = !note.IsPinned;
            note.UpdatedAt = DateTime.Now;
            
            var command = new SaveNoteCommand(note);
            var updatedNote = await _mediator.Send(command);
            
            // Update the note in the Notes collection
            var existingNote = Notes.FirstOrDefault(n => n.Id == updatedNote.Id);
            if (existingNote != null)
            {
                var index = Notes.IndexOf(existingNote);
                Notes[index] = updatedNote;
            }
            
            // Re-filter to update the UI with new sorting
            FilterNotes();
        }, nameof(TogglePin));
    }
}