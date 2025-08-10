using MediatR;

namespace Notes.Core.Commands;

public record RemoveTagFromNoteCommand(string NoteId, string TagId) : IRequest<bool>;