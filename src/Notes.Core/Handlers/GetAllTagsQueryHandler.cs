using MediatR;
using Notes.Core.Interfaces;
using Notes.Core.Models;
using Notes.Core.Queries;

namespace Notes.Core.Handlers;

public class GetAllTagsQueryHandler : IRequestHandler<GetAllTagsQuery, IEnumerable<Tag>>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<GetAllTagsQueryHandler> _logger;

    public GetAllTagsQueryHandler(ITagRepository tagRepository, ILogger<GetAllTagsQueryHandler> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Tag>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving all tags");

            var tags = await _tagRepository.GetAllAsync(cancellationToken);
            
            _logger.LogInformation("Successfully retrieved {Count} tags", tags.Count());
            return tags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all tags");
            throw;
        }
    }
}