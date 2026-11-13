using Microsoft.EntityFrameworkCore;

namespace PUSpecSheet.Database;

/// <summary>
/// The EF Core unit of work for the spec sheet database. The database is built database first: the schema is
/// owned by the SQL database project in <c>database/</c>, and the <c>Db*</c> entities and their
/// <see cref="IEntityTypeConfiguration{TEntity}"/> mappings in this assembly describe it to EF Core. A schema
/// change is made in the SQL project first, then mirrored here.
/// </summary>
public sealed class PuSpecSheetDbContext : DbContext
{
    public PuSpecSheetDbContext(DbContextOptions<PuSpecSheetDbContext> options)
        : base(options)
    {
    }

    public DbSet<DbSheetType> SheetTypes => Set<DbSheetType>();

    public DbSet<DbPhase> Phases => Set<DbPhase>();

    public DbSet<DbPhaseSheetType> PhaseSheetTypes => Set<DbPhaseSheetType>();

    public DbSet<DbCellType> CellTypes => Set<DbCellType>();

    public DbSet<DbCellTypeOption> CellTypeOptions => Set<DbCellTypeOption>();

    public DbSet<DbTableTemplate> TableTemplates => Set<DbTableTemplate>();

    public DbSet<DbTableTemplateVersion> TableTemplateVersions => Set<DbTableTemplateVersion>();

    public DbSet<DbTemplateSection> TemplateSections => Set<DbTemplateSection>();

    public DbSet<DbTemplateRow> TemplateRows => Set<DbTemplateRow>();

    public DbSet<DbTemplateCell> TemplateCells => Set<DbTemplateCell>();

    public DbSet<DbTemplateColumnBlock> TemplateColumnBlocks => Set<DbTemplateColumnBlock>();

    public DbSet<DbUser> Users => Set<DbUser>();

    public DbSet<DbRole> Roles => Set<DbRole>();

    public DbSet<DbPermission> Permissions => Set<DbPermission>();

    public DbSet<DbRolePermission> RolePermissions => Set<DbRolePermission>();

    public DbSet<DbUserRole> UserRoles => Set<DbUserRole>();

    public DbSet<DbSheet> Sheets => Set<DbSheet>();

    public DbSet<DbSheetVersion> SheetVersions => Set<DbSheetVersion>();

    public DbSet<DbSheetSubmission> SheetSubmissions => Set<DbSheetSubmission>();

    public DbSet<DbSheetSubmissionApprover> SheetSubmissionApprovers => Set<DbSheetSubmissionApprover>();

    public DbSet<DbSheetTable> SheetTables => Set<DbSheetTable>();

    public DbSet<DbSheetTableRevision> SheetTableRevisions => Set<DbSheetTableRevision>();

    public DbSet<DbSheetSection> SheetSections => Set<DbSheetSection>();

    public DbSet<DbSheetSectionRevision> SheetSectionRevisions => Set<DbSheetSectionRevision>();

    public DbSet<DbSheetColumnBlock> SheetColumnBlocks => Set<DbSheetColumnBlock>();

    public DbSet<DbSheetColumnBlockRevision> SheetColumnBlockRevisions => Set<DbSheetColumnBlockRevision>();

    public DbSet<DbSheetRow> SheetRows => Set<DbSheetRow>();

    public DbSet<DbSheetCell> SheetCells => Set<DbSheetCell>();

    public DbSet<DbSheetRowRevision> SheetRowRevisions => Set<DbSheetRowRevision>();

    public DbSet<DbTextValue> TextValues => Set<DbTextValue>();

    public DbSet<DbNumericValue> NumericValues => Set<DbNumericValue>();

    public DbSet<DbDateValue> DateValues => Set<DbDateValue>();

    public DbSet<DbBooleanValue> BooleanValues => Set<DbBooleanValue>();

    public DbSet<DbOptionValue> OptionValues => Set<DbOptionValue>();

    public DbSet<DbCellSettingsValue> CellSettings => Set<DbCellSettingsValue>();

    public DbSet<DbLabelledTextValue> LabelledTextValues => Set<DbLabelledTextValue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PuSpecSheetDbContext).Assembly);
    }
}
