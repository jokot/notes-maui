using MediatR;

namespace Notes.Core.Commands;

public record AddTagToNoteCommand(string NoteId, string TagId) : IRequest<bool>;