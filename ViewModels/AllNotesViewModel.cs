using Notes.Services;
using Notes.Views;

namespace Notes.ViewModels;

public partial class AllNotesViewModel : BaseViewModel
{
    [ObservableProperty]
    bool isRefreshing;
    public ObservableCollection<Note> Notes { get; } = [];

    readonly NoteService noteService;

    public AllNotesViewModel(NoteService noteService)
    {
        Title = "Your Notes";
        this.noteService = noteService;
    }

    public async Task GetNotesAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var notes = await noteService.GetNotes();
            Notes.Clear();
            foreach (var note in notes)
                Notes.Add(note);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get notes: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task RefreshNotes()
    {
        IsRefreshing = true;
        try
        {
            IsBusy = true;
            var notes = await noteService.ForceRefreshNotes();
            Notes.Clear();
            foreach (var note in notes)
                Notes.Add(note);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get notes: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task AddNote()
    {
        await Shell.Current.GoToAsync(nameof(NotePage));
    }

    [RelayCommand]
    async Task GoToNote(Note note)
    {
        if (note == null)
            return;

        // Should navigate to "NotePage?ItemId=path\on\device\XYZ.notes.txt"
        // await Shell.Current.GoToAsync($"{nameof(NotePage)}?{nameof(NotePage.ItemId)}={note.Filename}");

        await Shell.Current.GoToAsync(nameof(NotePage), true, new Dictionary<string, object>
        {
            { nameof(Note), note }
        });
    }
}
