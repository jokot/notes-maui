namespace Notes.Core.Data;

public class NotesDbContext : DbContext
{
    public DbSet<Note> Notes { get; set; } = null!;

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

            entity.Property(e => e.Filename)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Text)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Indexes
            entity.HasIndex(e => e.Filename)
                .IsUnique()
                .HasDatabaseName("IX_Notes_Filename");
            
            entity.HasIndex(e => e.UpdatedAt)
                .HasDatabaseName("IX_Notes_UpdatedAt");

            // Table name
            entity.ToTable("Notes");
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
