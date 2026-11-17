namespace PUSpecSheet.Query.Models.Values;

public sealed record SlimIntValue : SlimValue
{
    public required int? Value { get; init; }
}
