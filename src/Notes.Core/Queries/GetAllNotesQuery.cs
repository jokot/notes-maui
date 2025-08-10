namespace Notes.Core.Queries;

public record GetAllNotesQuery() : IRequest<IEnumerable<Note>>;
