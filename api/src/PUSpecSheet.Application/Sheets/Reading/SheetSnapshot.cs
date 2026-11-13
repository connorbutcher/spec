using PUSpecSheet.Application.Sheets.History;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Reading;

/// <summary>Everything needed to build one view of a sheet, loaded in a handful of queries.</summary>
public sealed record SheetSnapshot
{
    public required DbSheet Sheet { get; init; }

    /// <summary>The moment being shown, or null for the live view.</summary>
    public DateTime? AsOfUtc { get; init; }

    public int? ViewedVersionNumber { get; init; }

    /// <summary>The submission whose waiting changes are shown over the published sheet, or null.</summary>
    public int? ReviewedSubmissionId { get; init; }

    public required IReadOnlyList<DbSheetVersion> Versions { get; init; }

    public required IReadOnlyList<DbSheetTable> Tables { get; init; }

    public required IReadOnlyList<DbSheetSection> Sections { get; init; }

    public required IReadOnlyList<DbSheetRow> Rows { get; init; }

    public required IReadOnlyList<DbSheetColumnBlock> ColumnBlocks { get; init; }

    public required IReadOnlyDictionary<int, RevisionResolution<DbSheetTableRevision>> TableRevisions { get; init; }

    public required IReadOnlyDictionary<int, RevisionResolution<DbSheetSectionRevision>> SectionRevisions { get; init; }

    public required IReadOnlyDictionary<int, RevisionResolution<DbSheetRowRevision>> RowRevisions { get; init; }

    public required IReadOnlyDictionary<int, RevisionResolution<DbSheetColumnBlockRevision>> ColumnBlockRevisions { get; init; }

    /// <summary>Cell values by row revision id, then by sheet cell id.</summary>
    public required IReadOnlyDictionary<int, Dictionary<int, CellValueBag>> Values { get; init; }

    public SheetChangeHistory Changes { get; init; } = SheetChangeHistory.Empty;

    public required IReadOnlyDictionary<int, string> UserNames { get; init; }

    /// <summary>Every section of every template version the sheet's tables use.</summary>
    public required IReadOnlyList<DbTemplateSection> TemplateSections { get; init; }

    /// <summary>Every column block of those template versions.</summary>
    public required IReadOnlyList<DbTemplateColumnBlock> TemplateColumnBlocks { get; init; }

    /// <summary>Every row, with cells, of those template sections.</summary>
    public required IReadOnlyList<DbTemplateRow> TemplateRows { get; init; }

    /// <summary>Every cell type with its options, by id: what a cell's value reads as when another cell links to it.</summary>
    public required IReadOnlyDictionary<int, DbCellType> CellTypes { get; init; }

    public required IReadOnlyList<DbTableTemplate> AvailableTemplates { get; init; }

    public required IReadOnlyDictionary<int, int> LatestTemplateVersions { get; init; }
}
