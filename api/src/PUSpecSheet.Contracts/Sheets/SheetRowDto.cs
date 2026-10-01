namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A row of a sheet section. A lock held by the viewer means they can edit it; a lock held by someone
/// else means it's read-only until they publish or discard. <see cref="IsPending"/> rows have never
/// been published, so only their author can see them.
/// </summary>
public sealed record SheetRowDto(
    int Id,
    Guid PublicId,
    int TemplateRowId,
    int DisplayOrder,
    SheetLockDto? Lock,
    bool IsPending,
    bool CanRemove,
    IReadOnlyList<SheetCellDto> Cells);
