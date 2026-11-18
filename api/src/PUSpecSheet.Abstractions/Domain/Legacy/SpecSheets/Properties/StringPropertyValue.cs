namespace PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets.Properties;

public sealed record StringPropertyValue : IPropertyValue
{
    public required string? Value { get; init; }

    public override string ToString()
    {
        return Value ?? string.Empty;
    }
}
