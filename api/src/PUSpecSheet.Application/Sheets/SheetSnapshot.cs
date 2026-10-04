using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets;

/// <summary>Everything needed to build one view of a sheet, loaded in a handful of queries.</summary>
public sealed class SheetSnapshot
{
    public required Sheet Sheet { get; init; }

    /// <summary>The moment being shown, or null for the live view.</summary>
    public DateTime? AsOfUtc { get; init; }

    public int? ViewedVersionNumber { get; init; }

    public required IReadOnlyList<SheetVersion> Versions { get; init; }

    public required IReadOnlyList<SheetTable> Tables { get; init; }

    public required IReadOnlyList<SheetSection> Sections { get; init; }

    public required IReadOnlyList<SheetRow> Rows { get; init; }

    public required IReadOnlyList<SheetColumnBlock> ColumnBlocks { get; init; }

    public required IReadOnlyDictionary<int, RevisionResolution<SheetTableRevision>> TableRevisions { get; init; }

    public required IReadOnlyDictionary<int, RevisionResolution<SheetSectionRevision>> SectionRevisions { get; init; }

    public required IReadOnlyDictionary<int, RevisionResolution<SheetRowRevision>> RowRevisions { get; init; }

    public required IReadOnlyDictionary<int, RevisionResolution<SheetColumnBlockRevision>> ColumnBlockRevisions { get; init; }

    /// <summary>Cell values by row revision id, then by sheet cell id.</summary>
    public required IReadOnlyDictionary<int, Dictionary<int, CellValueBag>> Values { get; init; }

    public SheetChangeHistory Changes { get; set; } = SheetChangeHistory.Empty;

    public required IReadOnlyDictionary<int, string> UserNames { get; init; }

    /// <summary>Every section of every template version the sheet's tables use.</summary>
    public required IReadOnlyList<TemplateSection> TemplateSections { get; init; }

    /// <summary>Every column block of those template versions.</summary>
    public required IReadOnlyList<TemplateColumnBlock> TemplateColumnBlocks { get; init; }

    /// <summary>Every row, with cells, of those template sections.</summary>
    public required IReadOnlyList<TemplateRow> TemplateRows { get; init; }

    public required IReadOnlyList<TableTemplate> AvailableTemplates { get; init; }

    public required IReadOnlyDictionary<int, int> LatestTemplateVersions { get; init; }
}
