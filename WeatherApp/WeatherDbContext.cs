using Microsoft.EntityFrameworkCore;

public sealed class FavoriteCity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

public sealed class RecentSearch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Query { get; set; } = string.Empty;
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
}

public sealed class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options) { }

    public DbSet<FavoriteCity> Favorites => Set<FavoriteCity>();
    public DbSet<RecentSearch> Searches => Set<RecentSearch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FavoriteCity>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.City).IsRequired().HasMaxLength(100);
            entity.Property(f => f.Country).IsRequired().HasMaxLength(100);
            entity.HasIndex(f => new { f.City, f.Country }).IsUnique();
        });

        modelBuilder.Entity<RecentSearch>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Query).IsRequired().HasMaxLength(100);
            entity.HasIndex(s => s.SearchedAt);
        });
    }
}
