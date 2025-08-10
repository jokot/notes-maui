using MediatR;
using Notes.Core.Interfaces;
using Notes.Core.Models;
using Notes.Core.Queries;

namespace Notes.Core.Handlers;

public class GetTagsByNoteIdQueryHandler : IRequestHandler<GetTagsByNoteIdQuery, IEnumerable<Tag>>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<GetTagsByNoteIdQueryHandler> _logger;

    public GetTagsByNoteIdQueryHandler(ITagRepository tagRepository, ILogger<GetTagsByNoteIdQueryHandler> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Tag>> Handle(GetTagsByNoteIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving tags for note ID: {NoteId}", request.NoteId);

            var tags = await _tagRepository.GetTagsByNoteIdAsync(request.NoteId, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved {Count} tags for note ID: {NoteId}", tags.Count(), request.NoteId);
            return tags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve tags for note ID: {NoteId}", request.NoteId);
            throw;
        }
    }
}