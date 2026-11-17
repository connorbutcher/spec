using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Api.Database.Configuration.Access;
using PUSpecSheet.Api.Database.Configuration.Configuration;
using PUSpecSheet.Api.Database.Configuration.References;
using PUSpecSheet.Api.Database.Configuration.SpecSheets;
using PUSpecSheet.Api.Database.Configuration.Template;
using PUSpecSheet.Api.Database.Configuration.Types;
using PUSpecSheet.Api.Database.Configuration.Values;
using PUSpecSheet.Api.Database.Configuration.Workflow;
using PUSpecSheet.Api.Database.Conventions;
using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Configuration;
using PUSpecSheet.Api.Database.Schema.References;
using PUSpecSheet.Api.Database.Schema.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Template;
using PUSpecSheet.Api.Database.Schema.Types;
using PUSpecSheet.Api.Database.Schema.Values;
using PUSpecSheet.Api.Database.Schema.Workflow;

namespace PUSpecSheet.Api.Database;

public class PUSpecSheetDbContext : DbContext
{
    public PUSpecSheetDbContext(DbContextOptions<PUSpecSheetDbContext> options)
        : base(options)
    {
    }

    #region Access

    public required DbSet<DbUser> Users { get; set; }

    public required DbSet<DbPermission> Permissions { get; set; }

    public required DbSet<DbUserPermission> UserPermissions { get; set; }

    public required DbSet<DbSpecSheetTablePermission> SpecSheetTablePermissions { get; set; }

    public required DbSet<DbSpecSheetTypePermission> SpecSheetTypePermissions { get; set; }

    public required DbSet<DbSpecSheetPhaseObserver> SpecSheetPhaseObservers { get; set; }

    #endregion Access

    #region Dbo

    public required DbSet<DbSpecSheet> SpecSheets { get; set; }

    public required DbSet<DbSpecSheetTable> SpecSheetTables { get; set; }

    public required DbSet<DbSpecSheetTableWidget> SpecSheetTableWidgets { get; set; }

    public required DbSet<DbSpecSheetTableGroup> SpecSheetTableGroups { get; set; }

    public required DbSet<DbSpecSheetTableGroupWidget> SpecSheetTableGroupWidgets { get; set; }

    public required DbSet<DbSpecSheetTableRow> SpecSheetTableRows { get; set; }

    public required DbSet<DbSpecSheetTableCell> SpecSheetTableCells { get; set; }

    public required DbSet<DbSpecSheetPropertyValue> SpecSheetPropertyValues { get; set; }

    public required DbSet<DbSpecSheetTableNote> SpecSheetTableNotes { get; set; }

    #endregion Dbo

    #region Template

    public required DbSet<DbTableTemplate> TableTemplates { get; set; }

    public required DbSet<DbTableTemplateWidget> TableTemplateWidgets { get; set; }

    public required DbSet<DbTableGroupTemplate> TableGroupTemplates { get; set; }

    public required DbSet<DbTableGroupTemplateWidget> TableGroupTemplateWidgets { get; set; }

    public required DbSet<DbTableRowTemplate> TableRowTemplates { get; set; }

    public required DbSet<DbTableCellTemplate> TableCellTemplates { get; set; }

    public required DbSet<DbTableCellType> TableCellTypes { get; set; }

    #endregion Template

    #region Configuration

    public required DbSet<DbCellConfiguration> CellConfigurations { get; set; }

    public required DbSet<DbSpecSheetTypeStructureLevel> SpecSheetTypeStructureLevels { get; set; }

    public required DbSet<DbSpecSheetTypeStructureLevelValue> SpecSheetTypeStructureLevelValues { get; set; }

    #endregion Configuration

    #region Types

    public required DbSet<DbSpecSheetPhase> SpecSheetPhases { get; set; }

    public required DbSet<DbSpecSheetType> SpecSheetTypes { get; set; }

    public required DbSet<DbWidgetType> WidgetTypes { get; set; }

    public required DbSet<DbSpecSheetTypeProperty> SpecSheetTypeProperties { get; set; }

    #endregion Types

    #region Values

    public required DbSet<DbNumericValue> NumericValues { get; set; }

    public required DbSet<DbTextValue> TextValues { get; set; }

    public required DbSet<DbPairValue> PairValues { get; set; }

    public required DbSet<DbLinkedSpecSheetValue> LinkedSpecSheetValues { get; set; }

    public required DbSet<DbTableWidgetValue> TableWidgetValues { get; set; }

    public required DbSet<DbTableGroupWidgetValue> TableGroupWidgetValues { get; set; }

    #endregion Values

    #region Workflow

    public required DbSet<DbWorkflowStep> WorkflowSteps { get; set; }

    public required DbSet<DbWorkflowStepActor> WorkflowStepActors { get; set; }

    #endregion Workflow

    #region References

    public required DbSet<DbSpecSheetExternalReference> SpecSheetExternalReferences { get; set; }

    #endregion References

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserPermissionConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetTypePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetTablePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetPhaseObserverConfiguration());

        modelBuilder.ApplyConfiguration(new SpecSheetPhaseConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetTypeConfiguration());
        modelBuilder.ApplyConfiguration(new WidgetTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetTypePropertyConfiguration());

        modelBuilder.ApplyConfiguration(new SpecSheetConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetTableConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetTableWidgetConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetTableGroupConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetTableGroupWidgetConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetTableRowConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetTableCellConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetPropertyValueConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetTableNoteConfiguration());

        modelBuilder.ApplyConfiguration(new TableTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new TableTemplateWidgetConfiguration());
        modelBuilder.ApplyConfiguration(new TableGroupTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new TableGroupTemplateWidgetConfiguration());
        modelBuilder.ApplyConfiguration(new TableRowTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new TableCellTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new TableCellTypeConfiguration());

        modelBuilder.ApplyConfiguration(new CellConfigurationConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetTypeStructureLevelConfiguration());
        modelBuilder.ApplyConfiguration(new SpecSheetTypeStructureLevelValueConfiguration());

        modelBuilder.ApplyConfiguration(new NumericValueConfiguration());
        modelBuilder.ApplyConfiguration(new TextValueConfiguration());
        modelBuilder.ApplyConfiguration(new PairValueConfiguration());
        modelBuilder.ApplyConfiguration(new LinkedSpecSheetValueConfiguration());
        modelBuilder.ApplyConfiguration(new TableWidgetValueConfiguration());
        modelBuilder.ApplyConfiguration(new TableGroupWidgetValueConfiguration());

        modelBuilder.ApplyConfiguration(new WorkflowStepsConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowStepActorsConfiguration());

        modelBuilder.ApplyConfiguration(new SpecSheetExternalReferenceConfiguration());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<DateTimeUtcConverter>();
    }
}
