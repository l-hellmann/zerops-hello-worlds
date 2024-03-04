using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Entry> Entries { get; set; }
}

public class Entry
{
    public int Id { get; set; }
    public string Data { get; set; }
}
