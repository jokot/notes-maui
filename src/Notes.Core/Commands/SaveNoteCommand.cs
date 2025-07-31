namespace Notes.Core.Commands;

public record SaveNoteCommand(Note Note) : IRequest<Note>; 