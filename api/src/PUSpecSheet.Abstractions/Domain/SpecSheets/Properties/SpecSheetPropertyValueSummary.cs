namespace PUSpecSheet.Abstractions.Domain.SpecSheets.Properties;

public sealed record SpecSheetPropertyValueSummary
{
    public required IPropertyValue? PropertyValue { get; init; }

    public required string PropertyKey { get; init; }

    public required string PropertyName { get; init; }

    public required Guid ReferenceId { get; init; }

    public required string ValueControlType { get; init; }
}
