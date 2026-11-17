namespace PUSpecSheet.Abstractions.Requests.Types.Structure;

public sealed record UpdateSpecSheetTypeStructureLevelValueRequest
{
    public required int Id { get; init; }

    public required Guid SpecSheetTypePropertyReferenceId { get; init; }

    public required int DisplayOrder { get; init; }
}
