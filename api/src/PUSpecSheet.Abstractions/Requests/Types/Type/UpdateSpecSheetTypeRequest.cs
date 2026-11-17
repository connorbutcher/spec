using PUSpecSheet.Abstractions.Requests.Types.Properties;
using PUSpecSheet.Abstractions.Requests.Types.Structure;

namespace PUSpecSheet.Abstractions.Requests.Types.Type;

public sealed record UpdateSpecSheetTypeRequest
{
    public required int Id { get; init; }

    public required string Type { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool CanCopyFromMigrated { get; init; }

    public required bool RequiresApprovalWorkflow { get; init; }

    public required ICollection<int> NewSpecSheetPhaseIds { get; init; }

    public required ICollection<int> RemovedSpecSheetPhaseIds { get; init; }

    public ManageSpecSheetTypePropertiesRequest? Properties { get; init; }

    public ManageSpecSheetTypeStructureLevelsRequest? Structure { get; init; }
}
