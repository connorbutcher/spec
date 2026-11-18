using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Api.Database.Configuration.Access;
using PUSpecSheet.Api.Database.Configuration.Approval;
using PUSpecSheet.Api.Database.Configuration.Cell;
using PUSpecSheet.Api.Database.Configuration.Configuration;
using PUSpecSheet.Api.Database.Configuration.Legacy.References;
using PUSpecSheet.Api.Database.Configuration.Spec;
using PUSpecSheet.Api.Database.Configuration.Legacy.SpecSheets;
using PUSpecSheet.Api.Database.Configuration.Template;
using PUSpecSheet.Api.Database.Configuration.Types;
using PUSpecSheet.Api.Database.Configuration.Legacy.Values;
using PUSpecSheet.Api.Database.Configuration.Legacy.Workflow;
using PUSpecSheet.Api.Database.Conventions;
using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.Cell;
using PUSpecSheet.Api.Database.Schema.Configuration;
using PUSpecSheet.Api.Database.Schema.Legacy.References;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Template;
using PUSpecSheet.Api.Database.Schema.Types;
using PUSpecSheet.Api.Database.Schema.Legacy.Values;
using PUSpecSheet.Api.Database.Schema.Legacy.Workflow;

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

    public required DbSet<DbSpecSheetLinkedTable> SpecSheetLinkedTables { get; set; }

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

    #region Sheets

    public required DbSet<DbSheet> Sheets { get; set; }

    public required DbSet<DbSheetVersion> SheetVersions { get; set; }

    public required DbSet<DbSheetTable> SheetTables { get; set; }

    public required DbSet<DbSheetTableRevision> SheetTableRevisions { get; set; }

    public required DbSet<DbSheetTableWidget> SheetTableWidgets { get; set; }

    public required DbSet<DbSheetTableWidgetValue> SheetTableWidgetValues { get; set; }

    public required DbSet<DbSheetTableNote> SheetTableNotes { get; set; }

    public required DbSet<DbSheetTableNoteValue> SheetTableNoteValues { get; set; }

    public required DbSet<DbSheetTableGroup> SheetTableGroups { get; set; }

    public required DbSet<DbSheetTableGroupWidget> SheetTableGroupWidgets { get; set; }

    public required DbSet<DbSheetTableGroupWidgetValue> SheetTableGroupWidgetValues { get; set; }

    public required DbSet<DbSheetTableGroupRevision> SheetTableGroupRevisions { get; set; }

    public required DbSet<DbSheetTableRow> SheetTableRows { get; set; }

    public required DbSet<DbSheetTableRowRevision> SheetTableRowRevisions { get; set; }

    public required DbSet<DbSheetTableCell> SheetTableCells { get; set; }

    public required DbSet<DbSheetProperty> SheetProperties { get; set; }

    public required DbSet<DbSheetPropertyRevision> SheetPropertyRevisions { get; set; }

    public required DbSet<DbSheetLinkedTable> SheetLinkedTables { get; set; }

    public required DbSet<DbSheetLinkedTableRevision> SheetLinkedTableRevisions { get; set; }

    public required DbSet<DbSheetExternalReference> SheetExternalReferences { get; set; }

    public required DbSet<DbSheetExternalReferenceRevision> SheetExternalReferenceRevisions { get; set; }

    #endregion Sheets

    #region Cell values

    public required DbSet<DbSheetTextValue> SheetTextValues { get; set; }

    public required DbSet<DbSheetNumericValue> SheetNumericValues { get; set; }

    public required DbSet<DbSheetPairValue> SheetPairValues { get; set; }

    public required DbSet<DbSheetLinkedSheetValue> SheetLinkedSheetValues { get; set; }

    public required DbSet<DbSheetCellConfigurationOverride> SheetCellConfigurationOverrides { get; set; }

    #endregion Cell values

    #region Approval

    public required DbSet<DbSheetSubmission> SheetSubmissions { get; set; }

    public required DbSet<DbSheetSubmissionApprover> SheetSubmissionApprovers { get; set; }

    #endregion Approval

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
        modelBuilder.ApplyConfiguration(new SpecSheetLinkedTableConfiguration());

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

        modelBuilder.ApplyConfiguration(new SheetConfiguration());
        modelBuilder.ApplyConfiguration(new SheetVersionConfiguration());
        modelBuilder.ApplyConfiguration(new SheetTableConfiguration());
        modelBuilder.ApplyConfiguration(new SheetTableRevisionConfiguration());
        modelBuilder.ApplyConfiguration(new SheetTableGroupConfiguration());
        modelBuilder.ApplyConfiguration(new SheetTableGroupRevisionConfiguration());
        modelBuilder.ApplyConfiguration(new SheetTableWidgetConfiguration());
        modelBuilder.ApplyConfiguration(new SheetTableWidgetValueConfiguration());
        modelBuilder.ApplyConfiguration(new SheetTableNoteConfiguration());
        modelBuilder.ApplyConfiguration(new SheetTableNoteValueConfiguration());
        modelBuilder.ApplyConfiguration(new SheetTableGroupWidgetConfiguration());
        modelBuilder.ApplyConfiguration(new SheetTableGroupWidgetValueConfiguration());
        modelBuilder.ApplyConfiguration(new SheetTableRowConfiguration());
        modelBuilder.ApplyConfiguration(new SheetTableRowRevisionConfiguration());
        modelBuilder.ApplyConfiguration(new SheetTableCellConfiguration());
        modelBuilder.ApplyConfiguration(new SheetPropertyConfiguration());
        modelBuilder.ApplyConfiguration(new SheetPropertyRevisionConfiguration());
        modelBuilder.ApplyConfiguration(new SheetLinkedTableConfiguration());
        modelBuilder.ApplyConfiguration(new SheetLinkedTableRevisionConfiguration());
        modelBuilder.ApplyConfiguration(new SheetExternalReferenceConfiguration());
        modelBuilder.ApplyConfiguration(new SheetExternalReferenceRevisionConfiguration());

        modelBuilder.ApplyConfiguration(new SheetTextValueConfiguration());
        modelBuilder.ApplyConfiguration(new SheetNumericValueConfiguration());
        modelBuilder.ApplyConfiguration(new SheetPairValueConfiguration());
        modelBuilder.ApplyConfiguration(new SheetLinkedSheetValueConfiguration());
        modelBuilder.ApplyConfiguration(new SheetCellConfigurationOverrideConfiguration());

        modelBuilder.ApplyConfiguration(new SheetSubmissionConfiguration());
        modelBuilder.ApplyConfiguration(new SheetSubmissionApproverConfiguration());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<DateTimeUtcConverter>();
    }
}
