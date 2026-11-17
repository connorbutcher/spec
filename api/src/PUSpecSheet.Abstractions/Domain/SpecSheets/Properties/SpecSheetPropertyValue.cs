using PUSpecSheet.Abstractions.Domain.Types;

namespace PUSpecSheet.Abstractions.Domain.SpecSheets.Properties;

public sealed record SpecSheetPropertyValue
{
    public required int Id { get; init; }

    public required int SpecSheetId { get; init; }

    public required int SpecSheetTypePropertyId { get; init; }

    public required IPropertyValue? PropertyValue { get; init; }

    public required SpecSheetTypeProperty SpecSheetTypeProperty { get; init; }
}
