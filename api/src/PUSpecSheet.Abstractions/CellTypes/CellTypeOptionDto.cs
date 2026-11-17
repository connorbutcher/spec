namespace PUSpecSheet.Abstractions.CellTypes;

public sealed record CellTypeOptionDto
{
    public required int Id { get; init; }

    public required string Value { get; init; }

    public required int DisplayOrder { get; init; }
}
