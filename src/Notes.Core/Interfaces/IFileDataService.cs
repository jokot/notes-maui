using System;

namespace Notes.Core.Interfaces;

public interface IFileDataService
{
    Task<List<Note>> LoadNotesAsync();
    Task SaveNoteAsync(Note note);
    Task DeleteNoteAsync(string filename);
    Task<bool> NoteExistsAsync(string filename);
    string GenerateFilename();
}