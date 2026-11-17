using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Abstractions.Requests.Types.Structure;

public sealed record CreateSpecSheetTypeStructureLevelRequest
{
    public required int SpecSheetTypeId { get; init; }

    public required StructureLevelType StructureLevelType { get; init; }

    public required int Level { get; init; }

    public required int SortOrientation { get; init; }

    public required ICollection<CreateSpecSheetTypeStructureLevelValueRequest> ValueRequests { get; init; }
}
