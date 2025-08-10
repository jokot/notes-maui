using MediatR;
using Notes.Core.Models;

namespace Notes.Core.Queries;

public record GetAllTagsQuery() : IRequest<IEnumerable<Tag>>;