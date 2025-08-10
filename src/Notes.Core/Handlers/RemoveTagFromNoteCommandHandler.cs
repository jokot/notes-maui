using MediatR;
using Notes.Core.Commands;
using Notes.Core.Interfaces;

namespace Notes.Core.Handlers;

public class RemoveTagFromNoteCommandHandler : IRequestHandler<RemoveTagFromNoteCommand, bool>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<RemoveTagFromNoteCommandHandler> _logger;

    public RemoveTagFromNoteCommandHandler(
        ITagRepository tagRepository,
        ILogger<RemoveTagFromNoteCommandHandler> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(RemoveTagFromNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Removing tag {TagId} from note {NoteId}", request.TagId, request.NoteId);

            var result = await _tagRepository.RemoveTagFromNoteAsync(request.NoteId, request.TagId, cancellationToken);
            
            if (result)
            {
                _logger.LogInformation("Successfully removed tag {TagId} from note {NoteId}", request.TagId, request.NoteId);
            }
            else
            {
                _logger.LogWarning("Association between tag {TagId} and note {NoteId} not found", request.TagId, request.NoteId);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove tag {TagId} from note {NoteId}", request.TagId, request.NoteId);
            throw;
        }
    }
}