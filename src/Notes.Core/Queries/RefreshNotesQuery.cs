namespace Notes.Core.Queries;

public record RefreshNotesQuery() : IRequest<IEnumerable<Note>>;
