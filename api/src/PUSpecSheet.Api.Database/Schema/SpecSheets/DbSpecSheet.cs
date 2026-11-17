using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.References;
using PUSpecSheet.Api.Database.Schema.Types;
using PUSpecSheet.Api.Database.Schema.Workflow;

namespace PUSpecSheet.Api.Database.Schema.SpecSheets;

public record DbSpecSheet
{
    public required int Id { get; init; }

    public int? LegacySpecSheetId { get; init; }

    public required int SpecSheetPhaseId { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public required bool IsPublished { get; set; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required bool HasBeenMigrated { get; init; }

    public virtual DbUser? CreatedByUser { get; init; }

    public int? PublishedByUserId { get; set; }

    public DateTime? PublishedDateTime { get; set; }

    public string? ChangeLog { get; set; }

    public virtual DbUser? PublishedByUser { get; init; }

    public virtual DbSpecSheetPhase? SpecSheetPhase { get; init; }

    public virtual DbSpecSheetType? SpecSheetType { get; init; }

    public virtual ICollection<DbSpecSheetTable> Tables { get; init; } = [];

    public virtual ICollection<DbSpecSheetPropertyValue> PropertyValues { get; init; } = [];

    public virtual ICollection<DbWorkflowStep> WorkflowSteps { get; init; } = [];

    public virtual ICollection<DbSpecSheetExternalReference> ExternalReferences { get; init; } = [];
}
