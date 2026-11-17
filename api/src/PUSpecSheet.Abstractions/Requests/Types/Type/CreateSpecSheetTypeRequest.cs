using PUSpecSheet.Abstractions.Requests.Types.Properties;
using PUSpecSheet.Abstractions.Requests.Types.Structure;

namespace PUSpecSheet.Abstractions.Requests.Types.Type;

public sealed record CreateSpecSheetTypeRequest
{
    public required string Type { get; init; }

    public required int DisplayOrder { get; init; }

    public required string TableOrientation { get; init; }

    public required bool CanCopyFromMigrated { get; init; }

    public required bool RequiresApprovalWorkflow { get; init; }

    public required ICollection<int> SpecSheetPhaseIds { get; init; }

    public required ICollection<CreateSpecSheetTypePropertyRequest> Properties { get; init; }

    public required ICollection<CreateSpecSheetTypeStructureLevelRequest> StructureLevels { get; init; }
}
