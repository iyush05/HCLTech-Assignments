using Microsoft.EntityFrameworkCore;

public sealed class Note
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Color { get; set; } = "mint";
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class NotesDbContext : DbContext
{
    public NotesDbContext(DbContextOptions<NotesDbContext> options) : base(options) { }

    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Title).IsRequired().HasMaxLength(120);
            entity.Property(n => n.Content).IsRequired().HasMaxLength(5000);
            entity.Property(n => n.Category).IsRequired().HasMaxLength(40);
            entity.Property(n => n.Color).IsRequired().HasMaxLength(20);
            entity.HasIndex(n => n.Category);
            entity.HasIndex(n => n.IsPinned);
            entity.HasIndex(n => n.CreatedAt);
            entity.HasIndex(n => n.UpdatedAt);
        });
    }
}
