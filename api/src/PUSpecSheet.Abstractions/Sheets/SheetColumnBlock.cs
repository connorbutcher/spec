namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>
/// A copy of a column block on a horizontal sheet table. It runs through every row of the table: each
/// row's cells for it are the cells whose <see cref="SheetCell.SheetColumnBlockId"/> is this block's id.
/// <see cref="LastChange"/> is the publish that last added or moved the block.
/// </summary>
public sealed record SheetColumnBlock
{
    public required int Id { get; init; }

    public required Guid PublicId { get; init; }

    public required int TemplateColumnBlockId { get; init; }

    public required string Name { get; init; }

    public required int MinInstances { get; init; }

    public int? MaxInstances { get; init; }

    public required int InitialInstances { get; init; }

    public required int StickyColumnCount { get; init; }

    public required int DisplayOrder { get; init; }

    public SheetLock? Lock { get; init; }

    public required bool IsPending { get; init; }

    public required bool CanRemove { get; init; }

    public SheetChange? LastChange { get; init; }
}
