using MediatR;
using Notes.Core.Commands;
using Notes.Core.Interfaces;

namespace Notes.Core.Handlers;

public class AddTagToNoteCommandHandler : IRequestHandler<AddTagToNoteCommand, bool>
{
    private readonly ITagRepository _tagRepository;
    private readonly IRepository<Note> _noteRepository;
    private readonly ILogger<AddTagToNoteCommandHandler> _logger;

    public AddTagToNoteCommandHandler(
        ITagRepository tagRepository,
        IRepository<Note> noteRepository,
        ILogger<AddTagToNoteCommandHandler> logger)
    {
        _tagRepository = tagRepository;
        _noteRepository = noteRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(AddTagToNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Adding tag {TagId} to note {NoteId}", request.TagId, request.NoteId);

            // Verify note exists
            var note = await _noteRepository.GetByIdAsync(request.NoteId, cancellationToken);
            if (note == null)
            {
                _logger.LogWarning("Note with ID {NoteId} not found", request.NoteId);
                return false;
            }

            // Verify tag exists
            var tag = await _tagRepository.GetByIdAsync(request.TagId, cancellationToken);
            if (tag == null)
            {
                _logger.LogWarning("Tag with ID {TagId} not found", request.TagId);
                return false;
            }

            var result = await _tagRepository.AddTagToNoteAsync(request.NoteId, request.TagId, cancellationToken);
            
            if (result)
            {
                _logger.LogInformation("Successfully added tag {TagId} to note {NoteId}", request.TagId, request.NoteId);
            }
            else
            {
                _logger.LogWarning("Tag {TagId} is already associated with note {NoteId}", request.TagId, request.NoteId);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add tag {TagId} to note {NoteId}", request.TagId, request.NoteId);
            throw;
        }
    }
}