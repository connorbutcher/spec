namespace PUSpecSheet.Abstractions.Domain.Values.PropertyValues;

public sealed record IntPropertyValue : IPropertyValue
{
    public required int? Value { get; init; }

    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }
}
