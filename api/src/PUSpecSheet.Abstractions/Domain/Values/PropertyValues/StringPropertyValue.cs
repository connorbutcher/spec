namespace PUSpecSheet.Abstractions.Domain.Values.PropertyValues;

public sealed record StringPropertyValue : IPropertyValue
{
    public required string? Value { get; init; }

    public override string ToString()
    {
        return Value ?? string.Empty;
    }
}
