namespace PUSpecSheet.Abstractions.Domain.Values.PropertyValues;

public sealed record BoolPropertyValue : IPropertyValue
{
    public required bool? Value { get; init; }

    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }
}
