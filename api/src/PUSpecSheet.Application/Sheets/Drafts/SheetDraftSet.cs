using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

/// <summary>The drafts of every kind that one publish or submission takes from a sheet, tracked.</summary>
public sealed record SheetDraftSet
{
    public SheetDraftSet(
        IReadOnlyList<DbSheetTableRevision> tables,
        IReadOnlyList<DbSheetSectionRevision> sections,
        IReadOnlyList<DbSheetRowRevision> rows,
        IReadOnlyList<DbSheetColumnBlockRevision> columnBlocks)
    {
        this.Tables = tables;
        this.Sections = sections;
        this.Rows = rows;
        this.ColumnBlocks = columnBlocks;
    }

    public IReadOnlyList<DbSheetTableRevision> Tables { get; init; }

    public IReadOnlyList<DbSheetSectionRevision> Sections { get; init; }

    public IReadOnlyList<DbSheetRowRevision> Rows { get; init; }

    public IReadOnlyList<DbSheetColumnBlockRevision> ColumnBlocks { get; init; }

    public IReadOnlyList<IDbSheetRevision> All => [.. Tables, .. Sections, .. Rows, .. ColumnBlocks];

    public int Count => Tables.Count + Sections.Count + Rows.Count + ColumnBlocks.Count;
}
