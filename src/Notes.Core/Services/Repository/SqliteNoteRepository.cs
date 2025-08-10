namespace Notes.Core.Services.Repository;

public class SqliteNoteRepository : BaseRepository<Note>
{
    private readonly NotesDbContext _context;

    public SqliteNoteRepository(NotesDbContext context, ILogger<SqliteNoteRepository> logger)
        : base(logger)
    {
        _context = context;
    }

    public override async Task<IEnumerable<Note>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Retrieving all notes from database");
            
            var notes = await _context.Notes
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync(cancellationToken);
                
            LogInformation("Retrieved {Count} notes from database", notes.Count);
            return notes;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to retrieve notes from database");
            throw;
        }
    }

    public override async Task<Note?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Retrieving note with ID: {Id}", id);
            
            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
                
            if (note != null)
            {
                LogInformation("Found note with ID: {Id}", id);
            }
            else
            {
                LogWarning("Note with ID: {Id} not found", id);
            }
            
            return note;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to retrieve note with ID: {Id}", id);
            throw;
        }
    }

    public override async Task<Note> AddAsync(Note entity, CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Adding new note with filename: {Filename}", entity.Filename);
            
            // Use base class helper methods
            EnsureEntityId(entity);
            
            // Generate unique filename if not provided
            if (string.IsNullOrEmpty(entity.Filename))
            {
                entity.Filename = GenerateUniqueFilename();
            }
            
            UpdateTimestamp(entity);

            _context.Notes.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            LogInformation("Successfully added note with ID: {Id}", entity.Id);
            return entity;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to add note with filename: {Filename}", entity.Filename);
            throw;
        }
    }

    private string GenerateUniqueFilename()
    {
        // Generate a unique filename using timestamp and GUID
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var uniqueId = Guid.NewGuid().ToString()[..8]; // First 8 characters of GUID
        return $"{timestamp}-{uniqueId}.notes.txt";
    }

    public async Task<IEnumerable<Note>> GetAllForceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Force refreshing all notes from database");
            // For database implementation, this is the same as GetAllAsync
            // since the database is always the source of truth
            return await GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to force refresh notes from database");
            throw;
        }
    }

    public override async Task<Note> UpdateAsync(Note entity, CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Updating note with ID: {Id}", entity.Id);
            
            var existingNote = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == entity.Id, cancellationToken);
                
            if (existingNote == null)
            {
                LogWarning("Note with ID {Id} not found for update", entity.Id);
                throw new InvalidOperationException($"Note with ID {entity.Id} not found");
            }

            // Update properties
            existingNote.Text = entity.Text;
            existingNote.Filename = entity.Filename;
            UpdateTimestamp(existingNote);

            await _context.SaveChangesAsync(cancellationToken);
            
            LogInformation("Successfully updated note with ID: {Id}", entity.Id);
            return existingNote;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to update note with ID: {Id}", entity.Id);
            throw;
        }
    }

    public override async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Deleting note with ID: {Id}", id);
            
            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
                
            if (note == null)
            {
                LogWarning("Note with ID {Id} not found for deletion", id);
                return false;
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync(cancellationToken);
            
            LogInformation("Successfully deleted note with ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to delete note with ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            LogDebug("Checking if note exists with ID: {Id}", id);
            return await _context.Notes
                .AnyAsync(n => n.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to check if note exists with ID: {Id}", id);
            throw;
        }
    }
}
