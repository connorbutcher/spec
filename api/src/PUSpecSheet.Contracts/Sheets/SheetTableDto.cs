using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A table on a sheet, built from one version of a table template and keeping that layout. A horizontal
/// table also has <see cref="ColumnBlocks"/>, left to right, each running through every row.
/// </summary>
public sealed record SheetTableDto
{
    public SheetTableDto(
        int id,
        Guid publicId,
        int tableTemplateId,
        string templateName,
        int templateVersionNumber,
        TemplateOrientation orientation,
        int stickyColumnCount,
        string? title,
        int displayOrder,
        SheetLockDto? @lock,
        bool isPending,
        IReadOnlyList<SheetSectionDto> sections,
        IReadOnlyList<AddableSectionDto> addableSections,
        IReadOnlyList<SheetColumnBlockDto> columnBlocks,
        IReadOnlyList<AddableColumnBlockDto> addableColumnBlocks)
    {
        this.Id = id;
        this.PublicId = publicId;
        this.TableTemplateId = tableTemplateId;
        this.TemplateName = templateName;
        this.TemplateVersionNumber = templateVersionNumber;
        this.Orientation = orientation;
        this.StickyColumnCount = stickyColumnCount;
        this.Title = title;
        this.DisplayOrder = displayOrder;
        this.Lock = @lock;
        this.IsPending = isPending;
        this.Sections = sections;
        this.AddableSections = addableSections;
        this.ColumnBlocks = columnBlocks;
        this.AddableColumnBlocks = addableColumnBlocks;
    }

    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int TableTemplateId { get; init; }

    public string TemplateName { get; init; }

    public int TemplateVersionNumber { get; init; }

    public TemplateOrientation Orientation { get; init; }

    public int StickyColumnCount { get; init; }

    public string? Title { get; init; }

    public int DisplayOrder { get; init; }

    public SheetLockDto? Lock { get; init; }

    public bool IsPending { get; init; }

    public IReadOnlyList<SheetSectionDto> Sections { get; init; }

    public IReadOnlyList<AddableSectionDto> AddableSections { get; init; }

    public IReadOnlyList<SheetColumnBlockDto> ColumnBlocks { get; init; }

    public IReadOnlyList<AddableColumnBlockDto> AddableColumnBlocks { get; init; }

    /// <summary>The table's columns a linked dropdown on the sheet can take its choices from.</summary>
    public IReadOnlyList<SheetLinkableColumnDto> LinkableColumns { get; init; } = [];

    /// <summary>The table's rows a linked dropdown on the sheet can take its choices from, top to bottom.</summary>
    public IReadOnlyList<SheetLinkableRowDto> LinkableRows { get; init; } = [];
}
