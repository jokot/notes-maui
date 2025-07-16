using Notes.Models;

namespace Notes.Services;


public class NoteService
{

    public async Task<List<Note>> GetNotes()
    {
        List<Note> noteList = [];

        // Get the folder where the notes are stored.
        string appDataPath = FileSystem.AppDataDirectory;

        // Use Linq extensions to load the *.notes.txt files.
        var files = Directory.EnumerateFiles(appDataPath, "*.notes.txt");

        foreach (var file in files)
        {
            string text = await File.ReadAllTextAsync(file);
            DateTime date = File.GetLastWriteTime(file);

            noteList.Add(new Note
            {
                Filename = file,
                Text = text,
                Date = date
            });
        }
        return [.. noteList.OrderBy(note => note.Date)];
    }

    public async Task<Note> GetNote(string filename)
    {
        Models.Note note = new Models.Note();
        note.Filename = filename;

        if (File.Exists(filename))
        {
            note.Date = File.GetCreationTime(filename);
            note.Text = await File.ReadAllTextAsync(filename);
        }

        return note;
    }

    public async void SaveNote(String filename, string text)
    {
        await File.WriteAllTextAsync(filename, text);
    }

    public void DeleteNote(string filename)
    {
        if (File.Exists(filename))
            File.Delete(filename);
    }
}
