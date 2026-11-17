namespace PUSpecSheet.Abstractions.SheetTypes;

public sealed record SheetTypeDto
{
    public SheetTypeDto(int id, string name, int displayOrder)
    {
        Id = id;
        Name = name;
        DisplayOrder = displayOrder;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public int DisplayOrder { get; init; }

    /// <summary>The values that tell one sheet of this type from another within a phase, in display order. Empty when a phase has one sheet of the type.</summary>
    public IReadOnlyList<SheetTypeIdentifierDto> Identifiers { get; init; } = [];
}
