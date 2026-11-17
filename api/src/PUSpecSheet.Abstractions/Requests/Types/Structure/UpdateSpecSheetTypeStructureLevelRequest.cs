namespace PUSpecSheet.Abstractions.Requests.Types.Structure;

public sealed record UpdateSpecSheetTypeStructureLevelRequest
{
    public required int Id { get; init; }

    public required int Level { get; init; }

    public required int SortOrientation { get; init; }

    public ManageSpecSheetTypeStructureLevelValueRequest? ManageLevelValueRequest { get; init; }
}
