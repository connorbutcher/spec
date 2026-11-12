namespace PUSpecSheet.Contracts.SheetTypes;

public sealed record SheetTypeDto
{
    public SheetTypeDto(int id, string name, int displayOrder)
    {
        this.Id = id;
        this.Name = name;
        this.DisplayOrder = displayOrder;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public int DisplayOrder { get; init; }
}
