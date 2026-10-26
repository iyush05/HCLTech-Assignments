using Microsoft.EntityFrameworkCore;

public sealed class Contact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string Category { get; set; } = "Personal";
    public bool IsFavorite { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class ContactDbContext : DbContext
{
    public ContactDbContext(DbContextOptions<ContactDbContext> options) : base(options) { }

    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.FirstName).IsRequired().HasMaxLength(60);
            entity.Property(c => c.LastName).IsRequired().HasMaxLength(60);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(120);
            entity.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(30);
            entity.Property(c => c.Company).HasMaxLength(100);
            entity.Property(c => c.JobTitle).HasMaxLength(100);
            entity.Property(c => c.Category).IsRequired().HasMaxLength(30);
            entity.Property(c => c.Notes).HasMaxLength(500);

            entity.HasIndex(c => c.Email);
            entity.HasIndex(c => c.LastName);
            entity.HasIndex(c => c.Category);
            entity.HasIndex(c => c.IsFavorite);
            entity.HasIndex(c => c.CreatedAt);
        });
    }
}
