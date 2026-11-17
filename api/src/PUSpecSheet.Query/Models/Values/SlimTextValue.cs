namespace PUSpecSheet.Query.Models.Values;

public record SlimTextValue : SlimValue
{
    public required string? Value { get; init; }
}
