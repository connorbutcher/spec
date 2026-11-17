namespace PUSpecSheet.Query.Models.Values;

public sealed record SlimDateTimeValue : SlimValue
{
    public required DateTime? Value { get; init; }
}
