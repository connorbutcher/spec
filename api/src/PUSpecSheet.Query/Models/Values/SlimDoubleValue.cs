namespace PUSpecSheet.Query.Models.Values;

public record SlimDoubleValue : SlimValue
{
    public required double? Value { get; set; }
}
