using MediatR;
using Notes.Core.Models;

namespace Notes.Core.Queries;

public record GetTagsByNoteIdQuery(string NoteId) : IRequest<IEnumerable<Tag>>;