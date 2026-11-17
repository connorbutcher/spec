namespace PUSpecSheet.Query.Models.Values;

public sealed record SlimBoolValue : SlimValue
{
    public required bool? Value { get; init; }
}
