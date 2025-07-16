using Notes.Services;

namespace Notes.ViewModels;

// [QueryProperty(nameof(ItemId), nameof(ItemId))]
[QueryProperty(nameof(Note), nameof(Note))] // Assuming Note is a property of type Note
public partial class NoteViewModel : BaseViewModel
{

    // private string _itemId;
    // public string ItemId
    // {
    // 	get => _itemId;
    // 	set
    // 	{
    // 		_itemId = value;
    // 		LoadNote(_itemId);
    // 	}
    // }

    readonly NoteService noteService;

    public NoteViewModel(NoteService noteService)
    {
        Title = "Note";
        this.noteService = noteService;

        string appDataPath = FileSystem.AppDataDirectory;
        string randomFileName = $"{Path.GetRandomFileName()}.notes.txt";
        note = new Note
        {
            Filename = Path.Combine(appDataPath, randomFileName),
            Text = string.Empty,
            Date = DateTime.Now
        };
    }

    [ObservableProperty]
    Note? note;

    [RelayCommand]
    async Task SaveNote()
    {
        noteService.SaveNote(Note!.Filename, Note.Text);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    async Task DeleteNote()
    {
        noteService.DeleteNote(Note!.Filename);
        await Shell.Current.GoToAsync("..");
    }
}
