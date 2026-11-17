using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Schema.Types;

public record DbSpecSheetPhase
{
    public required int Id { get; init; }

    public required int? ParentSpecSheetPhaseId { get; set; }

    public required string Phase { get; set; }

    public required string PhaseShort { get; set; }

    public required bool IsObsolete { get; set; }

    public required int DisplayOrder { get; set; }

    public virtual DbSpecSheetPhase? ParentSpecSheetPhase { get; init; }

    public virtual ICollection<DbSpecSheetType> SpecSheetTypes { get; init; } = [];

    public virtual ICollection<DbSpecSheetPhaseObserver> PhaseObservers { get; init; } = [];
}
