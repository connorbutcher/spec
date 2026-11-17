namespace PUSpecSheet.Abstractions.Domain.SpecSheets.Properties;

public sealed record DateTimePropertyValue : IPropertyValue
{
    public required DateTime? Value { get; init; }

    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }
}
