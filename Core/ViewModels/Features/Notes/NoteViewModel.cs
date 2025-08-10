namespace Notes.Core.ViewModels;

[QueryProperty(nameof(Note), nameof(Note))]
public partial class NoteViewModel : BaseViewModel
{
    private readonly IRepository<Note> _noteRepository;

    public NoteViewModel(IRepository<Note> noteRepository, INavigationService navigationService, ILogger<NoteViewModel> logger)
        : base(navigationService, logger)
    {
        Title = "Note";
        _noteRepository = noteRepository;
        note = new Note();
    }

    [ObservableProperty]
    Note? note;

    [RelayCommand]
    async Task SaveNote()
    {
        await ExecuteAsync(async () =>
        {
            if (Note != null)
            {
                if (await _noteRepository.ExitsAsync(Note.Id))
                {
                    await _noteRepository.UpdateAsync(Note);
                }
                else
                {
                    await _noteRepository.AddAsync(Note);
                }
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
                await _noteRepository.DeleteAsync(Note.Id);
            }
            await NavigationService.GoBackAsync();
        }, nameof(DeleteNote));
    }
}
