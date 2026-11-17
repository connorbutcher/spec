namespace PUSpecSheet.Abstractions.Requests.Types.Structure;

public sealed record CreateSpecSheetTypeStructureLevelValueRequest
{
    public required int SpecSheetTypeStructureLevelId { get; init; }

    public required Guid SpecSheetTypePropertyReferenceId { get; init; }

    public required int DisplayOrder { get; init; }
}
