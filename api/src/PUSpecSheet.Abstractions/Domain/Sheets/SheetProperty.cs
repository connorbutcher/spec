using PUSpecSheet.Abstractions.Domain.Types;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

public record SheetProperty
{
    public required int Id { get; init; }

    public required int SheetId { get; init; }

    public required int SpecSheetTypePropertyId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required SheetItemRevision Revision { get; init; }

    public required string? Value { get; init; }

    public SpecSheetTypeProperty? SpecSheetTypeProperty { get; init; }
}
