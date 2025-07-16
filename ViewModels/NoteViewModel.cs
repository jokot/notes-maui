using Notes.Services;

namespace Notes.ViewModels;

[QueryProperty(nameof(Note), nameof(Note))]
public partial class NoteViewModel : BaseViewModel
{

    readonly NoteService noteService;

    public NoteViewModel(NoteService noteService)
    {
        Title = "Note";
        this.noteService = noteService;
        note = new Note();
    }

    [ObservableProperty]
    Note? note;

    [RelayCommand]
    async Task SaveNote()
    {
        await noteService.SaveNote(Note!.Filename, Note.Text);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    async Task DeleteNote()
    {
        await noteService.DeleteNote(Note!.Filename);
        await Shell.Current.GoToAsync("..");
    }
}
