using System;

namespace Notes.Core.Interfaces;

public interface IFileDataService
{
    Task<List<Note>> LoadNotesAsync();
    Task SaveNoteAsync(string filename, string text);
    Task DeleteNoteAsync(string filename);
    Task<bool> NoteExistsAsync(string filename);
    
    // Filename generation methods
    string GenerateUniqueFilename();
    string GenerateFilename(string title);
}