namespace PUSpecSheet.Abstractions.SheetTypes;

public sealed record SheetTypeDto
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required int DisplayOrder { get; init; }

    /// <summary>The values that tell one sheet of this type from another within a phase, in display order. Empty when a phase has one sheet of the type.</summary>
    public IReadOnlyList<SheetTypeIdentifierDto> Identifiers { get; init; } = [];
}
