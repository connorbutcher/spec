using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.References;
using PUSpecSheet.Abstractions.Domain.SpecSheets.Properties;
using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Abstractions.Domain.Workflow;

namespace PUSpecSheet.Abstractions.Domain.SpecSheets;

public record SpecSheet
{
    public required int Id { get; init; }

    public int? LegacySpecSheetId { get; init; }

    public required int SpecSheetPhaseId { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public required bool IsPublished { get; init; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required User CreatedByUser { get; init; }

    public required SpecSheetType SpecSheetType { get; init; }

    public required SpecSheetPhase SpecSheetPhase { get; init; }

    public required bool HasBeenMigrated { get; init; }

    public int? PublishedByUserId { get; init; }

    public DateTime? PublishedDateTime { get; init; }

    public User? PublishedByUser { get; init; }

    public string? ChangeLog { get; init; }

    public virtual ICollection<SpecSheetTable> Tables { get; set; } = [];

    public required ICollection<SpecSheetPropertyValue> PropertyValues { get; init; }

    public required ICollection<WorkflowStep> WorkflowSteps { get; init; }

    public required ICollection<SpecSheetExternalReference> ExternalReferences { get; init; }
}
