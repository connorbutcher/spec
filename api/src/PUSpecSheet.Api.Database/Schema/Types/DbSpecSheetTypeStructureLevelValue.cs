namespace PUSpecSheet.Api.Database.Schema.Types;

public sealed record DbSpecSheetTypeStructureLevelValue
{
    public required int Id { get; init; }

    public required int SpecSheetTypeStructureLevelId { get; init; }

    public required Guid SpecSheetTypePropertyReferenceId { get; set; }

    public required int DisplayOrder { get; set; }
}
