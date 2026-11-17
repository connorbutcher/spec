namespace PUSpecSheet.Abstractions.Templates;

/// <summary>
/// A table template at one version, with that version's whole section tree, rows and cells, each level
/// in display order, and for a horizontal table its column blocks left to right. <see cref="IsEditable"/>
/// is true only for the latest version while no sheet uses it.
/// </summary>
public sealed record TableTemplateDto
{
    public TableTemplateDto(
        int id,
        int sheetTypeId,
        string name,
        int displayOrder,
        int versionId,
        int versionNumber,
        TemplateOrientation orientation,
        int stickyColumnCount,
        bool isEditable,
        IReadOnlyList<TableTemplateVersionSummaryDto> versions,
        IReadOnlyList<TemplateSectionDto> sections,
        IReadOnlyList<TemplateColumnBlockDto> columnBlocks)
    {
        Id = id;
        SheetTypeId = sheetTypeId;
        Name = name;
        DisplayOrder = displayOrder;
        VersionId = versionId;
        VersionNumber = versionNumber;
        Orientation = orientation;
        StickyColumnCount = stickyColumnCount;
        IsEditable = isEditable;
        Versions = versions;
        Sections = sections;
        ColumnBlocks = columnBlocks;
    }

    public int Id { get; init; }

    public int SheetTypeId { get; init; }

    public string Name { get; init; }

    public int DisplayOrder { get; init; }

    public int VersionId { get; init; }

    public int VersionNumber { get; init; }

    public TemplateOrientation Orientation { get; init; }

    public int StickyColumnCount { get; init; }

    public bool IsEditable { get; init; }

    public IReadOnlyList<TableTemplateVersionSummaryDto> Versions { get; init; }

    public IReadOnlyList<TemplateSectionDto> Sections { get; init; }

    public IReadOnlyList<TemplateColumnBlockDto> ColumnBlocks { get; init; }
}
