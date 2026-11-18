using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

public record DbSpecSheetPropertyValue
{
    public required int Id { get; init; }

    public required int SpecSheetId { get; init; }

    public required int SpecSheetTypePropertyId { get; init; }

    public required string? PropertyValue { get; set; }

    public virtual DbSpecSheetTypeProperty? SpecSheetTypeProperty { get; init; }
}
