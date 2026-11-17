namespace PUSpecSheet.Abstractions.Domain.Types;

public sealed record SpecSheetTypeStructureLevelValue
{
    public required int Id { get; init; }

    public required int SpecSheetTypeStructureLevelId { get; init; }

    public required Guid SpecSheetTypePropertyReferenceId { get; set; }

    public required int DisplayOrder { get; set; }
}
