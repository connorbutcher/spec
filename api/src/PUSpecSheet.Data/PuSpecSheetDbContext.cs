using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Phases;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.SheetTypes;
using PUSpecSheet.Domain.Templates;
using PUSpecSheet.Domain.Users;
using PUSpecSheet.Domain.Values;

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

    public DbSet<CellType> CellTypes => Set<CellType>();

    public DbSet<CellTypeOption> CellTypeOptions => Set<CellTypeOption>();

    public DbSet<TableTemplate> TableTemplates => Set<TableTemplate>();

    public DbSet<TableTemplateVersion> TableTemplateVersions => Set<TableTemplateVersion>();

    public DbSet<TemplateSection> TemplateSections => Set<TemplateSection>();

    public DbSet<TemplateRow> TemplateRows => Set<TemplateRow>();

    public DbSet<TemplateCell> TemplateCells => Set<TemplateCell>();

    public DbSet<TemplateColumnBlock> TemplateColumnBlocks => Set<TemplateColumnBlock>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<Sheet> Sheets => Set<Sheet>();

    public DbSet<SheetVersion> SheetVersions => Set<SheetVersion>();

    public DbSet<SheetTable> SheetTables => Set<SheetTable>();

    public DbSet<SheetTableRevision> SheetTableRevisions => Set<SheetTableRevision>();

    public DbSet<SheetSection> SheetSections => Set<SheetSection>();

    public DbSet<SheetSectionRevision> SheetSectionRevisions => Set<SheetSectionRevision>();

    public DbSet<SheetColumnBlock> SheetColumnBlocks => Set<SheetColumnBlock>();

    public DbSet<SheetColumnBlockRevision> SheetColumnBlockRevisions => Set<SheetColumnBlockRevision>();

    public DbSet<SheetRow> SheetRows => Set<SheetRow>();

    public DbSet<SheetCell> SheetCells => Set<SheetCell>();

    public DbSet<SheetRowRevision> SheetRowRevisions => Set<SheetRowRevision>();

    public DbSet<TextValue> TextValues => Set<TextValue>();

    public DbSet<NumericValue> NumericValues => Set<NumericValue>();

    public DbSet<DateValue> DateValues => Set<DateValue>();

    public DbSet<BooleanValue> BooleanValues => Set<BooleanValue>();

    public DbSet<OptionValue> OptionValues => Set<OptionValue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PuSpecSheetDbContext).Assembly);
    }
}
