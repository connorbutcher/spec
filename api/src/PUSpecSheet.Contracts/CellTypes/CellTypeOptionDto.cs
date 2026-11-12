namespace PUSpecSheet.Contracts.CellTypes;

public sealed record CellTypeOptionDto
{
    public CellTypeOptionDto(int id, string value, int displayOrder)
    {
        this.Id = id;
        this.Value = value;
        this.DisplayOrder = displayOrder;
    }

    public int Id { get; init; }

    public string Value { get; init; }

    public int DisplayOrder { get; init; }
}
