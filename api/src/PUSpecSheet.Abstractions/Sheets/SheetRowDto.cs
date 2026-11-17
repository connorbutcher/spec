namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>
/// A row of a sheet section. A lock held by the viewer means they can edit it; a lock held by someone
/// else means it's read-only until they publish it or change it back. <see cref="IsPending"/> rows have never
/// been published, so only their author can see them.
/// </summary>
public sealed record SheetRowDto
{
    public SheetRowDto(
        int id,
        Guid publicId,
        int templateRowId,
        int displayOrder,
        SheetLockDto? @lock,
        bool isPending,
        bool canRemove,
        IReadOnlyList<SheetCellDto> cells,
        SheetChangeDto? lastChange)
    {
        Id = id;
        PublicId = publicId;
        TemplateRowId = templateRowId;
        DisplayOrder = displayOrder;
        Lock = @lock;
        IsPending = isPending;
        CanRemove = canRemove;
        Cells = cells;
        LastChange = lastChange;
    }

    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int TemplateRowId { get; init; }

    public int DisplayOrder { get; init; }

    public SheetLockDto? Lock { get; init; }

    public bool IsPending { get; init; }

    public bool CanRemove { get; init; }

    public IReadOnlyList<SheetCellDto> Cells { get; init; }

    public SheetChangeDto? LastChange { get; init; }
}
