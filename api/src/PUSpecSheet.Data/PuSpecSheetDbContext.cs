using Microsoft.EntityFrameworkCore;

namespace PUSpecSheet.Data;

/// <summary>
/// The EF Core unit of work for the spec sheet database. Entity configurations live in their own
/// <see cref="IEntityTypeConfiguration{TEntity}"/> classes in this assembly and are picked up here.
/// </summary>
public sealed class PuSpecSheetDbContext(DbContextOptions<PuSpecSheetDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PuSpecSheetDbContext).Assembly);
    }
}
