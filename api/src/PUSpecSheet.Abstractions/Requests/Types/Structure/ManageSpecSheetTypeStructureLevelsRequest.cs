namespace PUSpecSheet.Abstractions.Requests.Types.Structure;

public sealed record ManageSpecSheetTypeStructureLevelsRequest
{
    public required ICollection<CreateSpecSheetTypeStructureLevelRequest> CreateLevelRequests { get; init; }

    public required ICollection<UpdateSpecSheetTypeStructureLevelRequest> UpdateLevelRequests { get; init; }

    public required ICollection<int> DeleteLevelRequests { get; init; }
}
