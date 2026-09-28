using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Domain.Phases;
using PUSpecSheet.Domain.SheetTypes;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Data;

/// <summary>
/// The EF Core unit of work for the spec sheet database. Entity configurations live in their own
/// <see cref="IEntityTypeConfiguration{TEntity}"/> classes in this assembly and are picked up here.
/// </summary>
public sealed class PuSpecSheetDbContext(DbContextOptions<PuSpecSheetDbContext> options) : DbContext(options)
{
    public DbSet<SheetType> SheetTypes => Set<SheetType>();

    public DbSet<Phase> Phases => Set<Phase>();

    public DbSet<PhaseSheetType> PhaseSheetTypes => Set<PhaseSheetType>();

    public DbSet<TableTemplate> TableTemplates => Set<TableTemplate>();

    public DbSet<TemplateSection> TemplateSections => Set<TemplateSection>();

    public DbSet<TemplateCell> TemplateCells => Set<TemplateCell>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PuSpecSheetDbContext).Assembly);
    }
}
