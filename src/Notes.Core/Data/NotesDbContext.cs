namespace Notes.Core.Data;

public class NotesDbContext : DbContext
{
    public DbSet<Note> Notes { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<NoteTag> NoteTags { get; set; } = null!;

    public NotesDbContext(DbContextOptions<NotesDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Note entity
        modelBuilder.Entity<Note>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);
            
            // Properties
            entity.Property(e => e.Id)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Filename)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Text)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            entity.Property(e => e.IsPinned)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.BackgroundColor)
                .IsRequired()
                .HasMaxLength(7)
                .HasDefaultValue("#FFFFFF");

            // Indexes
            entity.HasIndex(e => e.Filename)
                .IsUnique()
                .HasDatabaseName("IX_Notes_Filename");
            
            entity.HasIndex(e => e.UpdatedAt)
                .HasDatabaseName("IX_Notes_UpdatedAt");

            // Table name
            entity.ToTable("Notes");
        });

        // Configure Tag entity
        modelBuilder.Entity<Tag>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);
            
            // Properties
            entity.Property(e => e.Id)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Color)
                .IsRequired()
                .HasMaxLength(7)
                .HasDefaultValue("#007ACC");

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Indexes
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("IX_Tags_Name");
            
            entity.HasIndex(e => e.UpdatedAt)
                .HasDatabaseName("IX_Tags_UpdatedAt");

            // Table name
            entity.ToTable("Tags");
        });

        // Configure NoteTag entity (junction table)
        modelBuilder.Entity<NoteTag>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);
            
            // Properties
            entity.Property(e => e.Id)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.NoteId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.TagId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Foreign key relationships
            entity.HasOne(nt => nt.Note)
                .WithMany(n => n.NoteTags)
                .HasForeignKey(nt => nt.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(nt => nt.Tag)
                .WithMany(t => t.NoteTags)
                .HasForeignKey(nt => nt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite unique index to prevent duplicate note-tag associations
            entity.HasIndex(e => new { e.NoteId, e.TagId })
                .IsUnique()
                .HasDatabaseName("IX_NoteTags_NoteId_TagId");
            
            entity.HasIndex(e => e.UpdatedAt)
                .HasDatabaseName("IX_NoteTags_UpdatedAt");

            // Table name
            entity.ToTable("NoteTags");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // This will be overridden by dependency injection, but provides a fallback
            optionsBuilder.UseSqlite("Data Source=notes.db");
        }
    }
}
