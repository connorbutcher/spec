namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A copy of a column block on a horizontal sheet table. It runs through every row of the table: each
/// row's cells for it are the cells whose <see cref="SheetCellDto.SheetColumnBlockId"/> is this block's id.
/// <see cref="LastChange"/> is the publish that last added or moved the block.
/// </summary>
public sealed record SheetColumnBlockDto(
    int Id,
    Guid PublicId,
    int TemplateColumnBlockId,
    string Name,
    int MinInstances,
    int? MaxInstances,
    int InitialInstances,
    int StickyColumnCount,
    int DisplayOrder,
    SheetLockDto? Lock,
    bool IsPending,
    bool CanRemove,
    SheetChangeDto? LastChange);
