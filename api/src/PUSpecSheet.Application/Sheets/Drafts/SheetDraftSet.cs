using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

/// <summary>The drafts of every kind that one publish or submission takes from a sheet, tracked.</summary>
public sealed record SheetDraftSet(
    IReadOnlyList<SheetTableRevision> Tables,
    IReadOnlyList<SheetSectionRevision> Sections,
    IReadOnlyList<SheetRowRevision> Rows,
    IReadOnlyList<SheetColumnBlockRevision> ColumnBlocks)
{
    public IReadOnlyList<ISheetRevision> All => [.. Tables, .. Sections, .. Rows, .. ColumnBlocks];

    public int Count => Tables.Count + Sections.Count + Rows.Count + ColumnBlocks.Count;
}
