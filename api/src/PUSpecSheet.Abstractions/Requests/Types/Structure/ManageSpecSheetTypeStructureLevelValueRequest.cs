namespace PUSpecSheet.Abstractions.Requests.Types.Structure;

public sealed record ManageSpecSheetTypeStructureLevelValueRequest
{
    public required ICollection<CreateSpecSheetTypeStructureLevelValueRequest> CreateLevelValueRequests { get; init; }

    public required ICollection<UpdateSpecSheetTypeStructureLevelValueRequest> UpdateLevelValueRequests { get; init; }

    public required ICollection<int> DeleteLevelValueRequests { get; init; }
}
