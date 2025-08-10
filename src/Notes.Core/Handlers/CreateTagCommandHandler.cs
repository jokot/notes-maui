using MediatR;
using Notes.Core.Commands;
using Notes.Core.Interfaces;
using Notes.Core.Models;

namespace Notes.Core.Handlers;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Tag>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<CreateTagCommandHandler> _logger;

    public CreateTagCommandHandler(ITagRepository tagRepository, ILogger<CreateTagCommandHandler> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task<Tag> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating tag with name: {Name}", request.Name);

            // Check if tag with same name already exists
            var existingTag = await _tagRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingTag != null)
            {
                _logger.LogWarning("Tag with name {Name} already exists", request.Name);
                return existingTag;
            }

            var tag = new Tag
            {
                Name = request.Name.Trim(),
                Color = request.Color
            };

            var createdTag = await _tagRepository.AddAsync(tag, cancellationToken);
            
            _logger.LogInformation("Successfully created tag with ID: {Id}", createdTag.Id);
            return createdTag;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tag with name: {Name}", request.Name);
            throw;
        }
    }
}