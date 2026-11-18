using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets.Properties;

namespace PUSpecSheet.Abstractions.Requests.SpecSheets.Properties;

public record CreateSpecSheetPropertyValueRequest
{
    public required int SpecSheetTypePropertyId { get; init; }

    public IPropertyValue? PropertyValue { get; init; }
}
