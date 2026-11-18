using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets.Properties;
using PUSpecSheet.Abstractions.Domain.Types;

namespace PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;

public record SpecSheetSummary
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

    public required ICollection<SpecSheetPropertyValueSummary> PropertyValues { get; init; }
}
