namespace Notes.Core.Interfaces;

public interface ITagRepository : IRepository<Tag>
{
    Task<IEnumerable<Tag>> GetTagsByNoteIdAsync(string noteId, CancellationToken cancellationToken = default);
    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> AddTagToNoteAsync(string noteId, string tagId, CancellationToken cancellationToken = default);
    Task<bool> RemoveTagFromNoteAsync(string noteId, string tagId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Note>> GetNotesByTagIdAsync(string tagId, CancellationToken cancellationToken = default);
}