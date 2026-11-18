namespace PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets.Properties;

public sealed record DoublePropertyValue : IPropertyValue
{
    public required double? Value { get; init; }

    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }
}
