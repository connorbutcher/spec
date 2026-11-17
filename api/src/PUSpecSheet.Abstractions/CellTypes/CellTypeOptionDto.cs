namespace PUSpecSheet.Abstractions.CellTypes;

public sealed record CellTypeOptionDto
{
    public CellTypeOptionDto(int id, string value, int displayOrder)
    {
        Id = id;
        Value = value;
        DisplayOrder = displayOrder;
    }

    public int Id { get; init; }

    public string Value { get; init; }

    public int DisplayOrder { get; init; }
}
