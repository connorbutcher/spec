namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A copy of a column block on a horizontal sheet table. It runs through every row of the table: each
/// row's cells for it are the cells whose <see cref="SheetCellDto.SheetColumnBlockId"/> is this block's id.
/// <see cref="LastChange"/> is the publish that last added or moved the block.
/// </summary>
public sealed record SheetColumnBlockDto
{
    public SheetColumnBlockDto(
        int id,
        Guid publicId,
        int templateColumnBlockId,
        string name,
        int minInstances,
        int? maxInstances,
        int initialInstances,
        int stickyColumnCount,
        int displayOrder,
        SheetLockDto? @lock,
        bool isPending,
        bool canRemove,
        SheetChangeDto? lastChange)
    {
        this.Id = id;
        this.PublicId = publicId;
        this.TemplateColumnBlockId = templateColumnBlockId;
        this.Name = name;
        this.MinInstances = minInstances;
        this.MaxInstances = maxInstances;
        this.InitialInstances = initialInstances;
        this.StickyColumnCount = stickyColumnCount;
        this.DisplayOrder = displayOrder;
        this.Lock = @lock;
        this.IsPending = isPending;
        this.CanRemove = canRemove;
        this.LastChange = lastChange;
    }

    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int TemplateColumnBlockId { get; init; }

    public string Name { get; init; }

    public int MinInstances { get; init; }

    public int? MaxInstances { get; init; }

    public int InitialInstances { get; init; }

    public int StickyColumnCount { get; init; }

    public int DisplayOrder { get; init; }

    public SheetLockDto? Lock { get; init; }

    public bool IsPending { get; init; }

    public bool CanRemove { get; init; }

    public SheetChangeDto? LastChange { get; init; }
}
