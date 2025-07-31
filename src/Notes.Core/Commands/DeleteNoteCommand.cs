namespace Notes.Core.Commands;
 
public record DeleteNoteCommand(Note Note) : IRequest<bool>; 