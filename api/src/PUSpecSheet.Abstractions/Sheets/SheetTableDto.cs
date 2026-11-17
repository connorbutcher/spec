using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>
/// A table on a sheet, built from one version of a table template and keeping that layout. A horizontal
/// table also has <see cref="ColumnBlocks"/>, left to right, each running through every row.
/// </summary>
public sealed record SheetTableDto
{
    public required int Id { get; init; }

    public required Guid PublicId { get; init; }

    public required int TableTemplateId { get; init; }

    public required string TemplateName { get; init; }

    public required int TemplateVersionNumber { get; init; }

    public required TemplateOrientation Orientation { get; init; }

    public required int StickyColumnCount { get; init; }

    public string? Title { get; init; }

    public required int DisplayOrder { get; init; }

    public SheetLockDto? Lock { get; init; }

    public required bool IsPending { get; init; }

    public required IReadOnlyList<SheetSectionDto> Sections { get; init; }

    public required IReadOnlyList<AddableSectionDto> AddableSections { get; init; }

    public required IReadOnlyList<SheetColumnBlockDto> ColumnBlocks { get; init; }

    public required IReadOnlyList<AddableColumnBlockDto> AddableColumnBlocks { get; init; }

    /// <summary>The table's columns a linked dropdown on the sheet can take its choices from.</summary>
    public IReadOnlyList<SheetLinkableColumnDto> LinkableColumns { get; init; } = [];

    /// <summary>The table's rows a linked dropdown on the sheet can take its choices from, top to bottom.</summary>
    public IReadOnlyList<SheetLinkableRowDto> LinkableRows { get; init; } = [];
}
