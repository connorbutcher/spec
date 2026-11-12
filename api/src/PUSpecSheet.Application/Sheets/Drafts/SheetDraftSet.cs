using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

/// <summary>The drafts of every kind that one publish or submission takes from a sheet, tracked.</summary>
public sealed record SheetDraftSet
{
    public SheetDraftSet(
        IReadOnlyList<SheetTableRevision> tables,
        IReadOnlyList<SheetSectionRevision> sections,
        IReadOnlyList<SheetRowRevision> rows,
        IReadOnlyList<SheetColumnBlockRevision> columnBlocks)
    {
        this.Tables = tables;
        this.Sections = sections;
        this.Rows = rows;
        this.ColumnBlocks = columnBlocks;
    }

    public IReadOnlyList<SheetTableRevision> Tables { get; init; }

    public IReadOnlyList<SheetSectionRevision> Sections { get; init; }

    public IReadOnlyList<SheetRowRevision> Rows { get; init; }

    public IReadOnlyList<SheetColumnBlockRevision> ColumnBlocks { get; init; }

    public IReadOnlyList<ISheetRevision> All => [.. Tables, .. Sections, .. Rows, .. ColumnBlocks];

    public int Count => Tables.Count + Sections.Count + Rows.Count + ColumnBlocks.Count;
}
