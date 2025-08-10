using System.Collections.ObjectModel;
using Notes.Core.Commands;
using Notes.Core.Models;
using Notes.Core.Queries;

namespace Notes.Maui.ViewModels.Features.Notes;

[QueryProperty(nameof(Note), nameof(Note))]
[QueryProperty(nameof(IsEdit), "IsEdit")]
public partial class NoteViewModel : BaseViewModel
{
    private readonly IMediator _mediator;

    private readonly string _initialNoteId;

    [ObservableProperty]
    private ObservableCollection<Tag> noteTags = new();

    [ObservableProperty]
    private ObservableCollection<Tag> availableTags = new();

    [ObservableProperty]
    private bool isTagsLoaded;

    [ObservableProperty]
    private bool isBackgroundColorBottomSheetVisible;

    [ObservableProperty]
    private bool isTagsBottomSheetVisible;

    public NoteViewModel(
        IMediator mediator,
        INavigationService navigationService, 
        ILogger<NoteViewModel> logger)
        : base(navigationService, logger)
    {
        Title = "Note";
        _mediator = mediator;
        note = new Note { Title = "Untitled" };
        _initialNoteId = note.Id; // Store the ID of the initial note
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditMode))]
    Note? note;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditMode))]
    bool isEdit;

    public bool IsEditMode 
    {
        get 
        {
            // Show delete button only when:
            // 1. IsEdit is true (we're in edit mode)
            // 2. Note is not null
            // 3. The note is not the initial note we created in constructor (it's a real note from the database)
            return IsEdit && Note != null && Note.Id != _initialNoteId;
        }
    }

    [RelayCommand]
    async Task SaveNote()
    {
        await ExecuteAsync(async () =>
        {
            if (Note != null)
            {
                // Ensure note has some content - either title or text
                if (string.IsNullOrWhiteSpace(Note.Text) && string.IsNullOrWhiteSpace(Note.Title))
                {
                    // If both are empty, set a default title
                    Note.Title = "Untitled";
                    Note.Text = "";
                }
                
                // Ensure note has a title
                if (string.IsNullOrWhiteSpace(Note.Title))
                {
                    if (!string.IsNullOrWhiteSpace(Note.Text))
                    {
                        // Generate title from first line of text or provide default
                        var firstLine = Note.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                        Note.Title = !string.IsNullOrWhiteSpace(firstLine) && firstLine.Length <= 50 
                            ? firstLine.Trim() 
                            : firstLine?.Substring(0, Math.Min(50, firstLine.Length)).Trim() + "..." ?? "Untitled";
                    }
                    else
                    {
                        Note.Title = "Untitled";
                    }
                }

                // Create command with the note object
                var command = new SaveNoteCommand(Note);

                // Use MediatR to send the command
                var savedNote = await _mediator.Send(command);
                
                // Update the current note with saved data
                Note = savedNote;
                
                // Load tags after saving if not already loaded
                if (!IsTagsLoaded)
                {
                    await LoadTagsAsync();
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

    [RelayCommand]
    void SetBackgroundColor(string color)
    {
        if (Note != null)
        {
            Note.BackgroundColor = color;
            OnPropertyChanged(nameof(Note));
            IsBackgroundColorBottomSheetVisible = false;
        }
    }

    [RelayCommand]
    void ShowBackgroundColorBottomSheet()
    {
        IsBackgroundColorBottomSheetVisible = true;
    }

    [RelayCommand]
    void HideBackgroundColorBottomSheet()
    {
        IsBackgroundColorBottomSheetVisible = false;
    }

    [RelayCommand]
    void ShowTagsBottomSheet()
    {
        IsTagsBottomSheetVisible = true;
        if (!IsTagsLoaded)
        {
            _ = LoadTagsAsync();
        }
    }

    [RelayCommand]
    void HideTagsBottomSheet()
    {
        IsTagsBottomSheetVisible = false;
    }

    [RelayCommand]
    async Task LoadTagsAsync()
    {
        await ExecuteAsync(async () =>
        {
            if (Note == null) return;

            // Load tags for this note
            var noteTagsQuery = new GetTagsByNoteIdQuery(Note.Id);
            var noteTagsResult = await _mediator.Send(noteTagsQuery);
            
            NoteTags.Clear();
            foreach (var tag in noteTagsResult)
            {
                NoteTags.Add(tag);
            }

            // Load all available tags
            var allTagsQuery = new GetAllTagsQuery();
            var allTagsResult = await _mediator.Send(allTagsQuery);
            
            AvailableTags.Clear();
            foreach (var tag in allTagsResult)
            {
                AvailableTags.Add(tag);
            }

            IsTagsLoaded = true;
        }, "Loading tags");
    }

    [RelayCommand]
    async Task AddTagToNoteAsync(Tag tag)
    {
        if (Note == null || tag == null) return;

        await ExecuteAsync(async () =>
        {
            var command = new AddTagToNoteCommand(Note.Id, tag.Id);
            var success = await _mediator.Send(command);
            
            if (success && !NoteTags.Any(t => t.Id == tag.Id))
            {
                NoteTags.Add(tag);
            }
        }, "Adding tag to note");
    }

    [RelayCommand]
    async Task RemoveTagFromNoteAsync(Tag tag)
    {
        if (Note == null || tag == null) return;

        await ExecuteAsync(async () =>
        {
            var command = new RemoveTagFromNoteCommand(Note.Id, tag.Id);
            var success = await _mediator.Send(command);
            
            if (success)
            {
                var existingTag = NoteTags.FirstOrDefault(t => t.Id == tag.Id);
                if (existingTag != null)
                {
                    NoteTags.Remove(existingTag);
                }
            }
        }, "Removing tag from note");
    }

    [RelayCommand]
    async Task CreateAndAddTagAsync(string tagName)
    {
        if (Note == null || string.IsNullOrWhiteSpace(tagName)) return;

        await ExecuteAsync(async () =>
        {
            // First create the tag
            var createCommand = new CreateTagCommand(tagName.Trim());
            var createdTag = await _mediator.Send(createCommand);
            
            if (createdTag != null)
            {
                // Add to available tags if not already there
                if (!AvailableTags.Any(t => t.Id == createdTag.Id))
                {
                    AvailableTags.Add(createdTag);
                }
                
                // Add to note
                await AddTagToNoteAsync(createdTag);
            }
        }, "Creating and adding tag");
    }
}