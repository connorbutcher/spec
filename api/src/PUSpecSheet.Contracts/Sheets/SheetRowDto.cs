namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A row of a sheet section. A lock held by the viewer means they can edit it; a lock held by someone
/// else means it's read-only until they publish or discard. <see cref="IsPending"/> rows have never
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
        this.Id = id;
        this.PublicId = publicId;
        this.TemplateRowId = templateRowId;
        this.DisplayOrder = displayOrder;
        this.Lock = @lock;
        this.IsPending = isPending;
        this.CanRemove = canRemove;
        this.Cells = cells;
        this.LastChange = lastChange;
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
