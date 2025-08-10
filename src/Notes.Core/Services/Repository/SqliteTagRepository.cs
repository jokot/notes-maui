using Microsoft.EntityFrameworkCore;
using Notes.Core.Data;
using Notes.Core.Interfaces;
using Notes.Core.Models;

namespace Notes.Core.Services.Repository;

public class SqliteTagRepository : BaseRepository<Tag>, ITagRepository
{
    private readonly NotesDbContext _context;

    public SqliteTagRepository(NotesDbContext context, ILogger<SqliteTagRepository> logger)
        : base(logger)
    {
        _context = context;
    }

    public override async Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Retrieving all tags");
            
            var tags = await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);
            
            LogInformation("Successfully retrieved {Count} tags", tags.Count);
            return tags;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to retrieve all tags");
            throw;
        }
    }

    public override async Task<Tag?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Retrieving tag with ID: {Id}", id);
            
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
            
            if (tag != null)
            {
                LogInformation("Successfully retrieved tag with ID: {Id}", id);
            }
            else
            {
                LogWarning("Tag with ID {Id} not found", id);
            }
            
            return tag;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to retrieve tag with ID: {Id}", id);
            throw;
        }
    }

    public override async Task<Tag> AddAsync(Tag entity, CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Adding new tag with name: {Name}", entity.Name);
            
            // Use base class helper methods
            EnsureEntityId(entity);
            UpdateTimestamp(entity);

            _context.Tags.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            LogInformation("Successfully added tag with ID: {Id}", entity.Id);
            return entity;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to add tag with name: {Name}", entity.Name);
            throw;
        }
    }

    public override async Task<Tag> UpdateAsync(Tag entity, CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Updating tag with ID: {Id}", entity.Id);
            
            var existingTag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == entity.Id, cancellationToken);
                
            if (existingTag == null)
            {
                LogWarning("Tag with ID {Id} not found for update", entity.Id);
                throw new InvalidOperationException($"Tag with ID {entity.Id} not found");
            }

            // Update properties
            existingTag.Name = entity.Name;
            existingTag.Color = entity.Color;
            UpdateTimestamp(existingTag);

            await _context.SaveChangesAsync(cancellationToken);
            
            LogInformation("Successfully updated tag with ID: {Id}", entity.Id);
            return existingTag;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to update tag with ID: {Id}", entity.Id);
            throw;
        }
    }

    public override async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Deleting tag with ID: {Id}", id);
            
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
                
            if (tag == null)
            {
                LogWarning("Tag with ID {Id} not found for deletion", id);
                return false;
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync(cancellationToken);
            
            LogInformation("Successfully deleted tag with ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to delete tag with ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Tag>> GetTagsByNoteIdAsync(string noteId, CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Retrieving tags for note ID: {NoteId}", noteId);
            
            var tags = await _context.NoteTags
                .Where(nt => nt.NoteId == noteId)
                .Include(nt => nt.Tag)
                .Select(nt => nt.Tag)
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);
            
            LogInformation("Successfully retrieved {Count} tags for note ID: {NoteId}", tags.Count, noteId);
            return tags;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to retrieve tags for note ID: {NoteId}", noteId);
            throw;
        }
    }

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Retrieving tag with name: {Name}", name);
            
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower(), cancellationToken);
            
            if (tag != null)
            {
                LogInformation("Successfully retrieved tag with name: {Name}", name);
            }
            else
            {
                LogWarning("Tag with name {Name} not found", name);
            }
            
            return tag;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to retrieve tag with name: {Name}", name);
            throw;
        }
    }

    public async Task<bool> AddTagToNoteAsync(string noteId, string tagId, CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Adding tag {TagId} to note {NoteId}", tagId, noteId);
            
            // Check if association already exists
            var existingAssociation = await _context.NoteTags
                .FirstOrDefaultAsync(nt => nt.NoteId == noteId && nt.TagId == tagId, cancellationToken);
                
            if (existingAssociation != null)
            {
                LogWarning("Tag {TagId} is already associated with note {NoteId}", tagId, noteId);
                return false;
            }

            var noteTag = new NoteTag
            {
                NoteId = noteId,
                TagId = tagId
            };
            
            if (string.IsNullOrEmpty(noteTag.Id))
            {
                noteTag.Id = Guid.NewGuid().ToString();
            }
            noteTag.UpdatedAt = DateTime.UtcNow;

            _context.NoteTags.Add(noteTag);
            await _context.SaveChangesAsync(cancellationToken);
            
            LogInformation("Successfully added tag {TagId} to note {NoteId}", tagId, noteId);
            return true;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to add tag {TagId} to note {NoteId}", tagId, noteId);
            throw;
        }
    }

    public async Task<bool> RemoveTagFromNoteAsync(string noteId, string tagId, CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Removing tag {TagId} from note {NoteId}", tagId, noteId);
            
            var noteTag = await _context.NoteTags
                .FirstOrDefaultAsync(nt => nt.NoteId == noteId && nt.TagId == tagId, cancellationToken);
                
            if (noteTag == null)
            {
                LogWarning("Association between tag {TagId} and note {NoteId} not found", tagId, noteId);
                return false;
            }

            _context.NoteTags.Remove(noteTag);
            await _context.SaveChangesAsync(cancellationToken);
            
            LogInformation("Successfully removed tag {TagId} from note {NoteId}", tagId, noteId);
            return true;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to remove tag {TagId} from note {NoteId}", tagId, noteId);
            throw;
        }
    }

    public async Task<IEnumerable<Note>> GetNotesByTagIdAsync(string tagId, CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Retrieving notes for tag ID: {TagId}", tagId);
            
            var notes = await _context.NoteTags
                .Where(nt => nt.TagId == tagId)
                .Include(nt => nt.Note)
                .Select(nt => nt.Note)
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync(cancellationToken);
            
            LogInformation("Successfully retrieved {Count} notes for tag ID: {TagId}", notes.Count, tagId);
            return notes;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to retrieve notes for tag ID: {TagId}", tagId);
            throw;
        }
    }
}