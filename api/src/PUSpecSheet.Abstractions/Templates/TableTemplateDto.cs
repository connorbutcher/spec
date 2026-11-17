namespace PUSpecSheet.Abstractions.Templates;

/// <summary>
/// A table template at one version, with that version's whole section tree, rows and cells, each level
/// in display order, and for a horizontal table its column blocks left to right. <see cref="IsEditable"/>
/// is true only for the latest version while no sheet uses it.
/// </summary>
public sealed record TableTemplateDto
{
    public required int Id { get; init; }

    public required int SheetTypeId { get; init; }

    public required string Name { get; init; }

    public required int DisplayOrder { get; init; }

    public required int VersionId { get; init; }

    public required int VersionNumber { get; init; }

    public required TemplateOrientation Orientation { get; init; }

    public required int StickyColumnCount { get; init; }

    public required bool IsEditable { get; init; }

    public required IReadOnlyList<TableTemplateVersionSummaryDto> Versions { get; init; }

    public required IReadOnlyList<TemplateSectionDto> Sections { get; init; }

    public required IReadOnlyList<TemplateColumnBlockDto> ColumnBlocks { get; init; }
}
