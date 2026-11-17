using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Drafts;

public sealed record SheetDraftSet
{
    public required IReadOnlyList<DbSheetTableRevision> Tables { get; init; }

    public required IReadOnlyList<DbSheetSectionRevision> Sections { get; init; }

    public required IReadOnlyList<DbSheetRowRevision> Rows { get; init; }

    public required IReadOnlyList<DbSheetColumnBlockRevision> ColumnBlocks { get; init; }

    public IReadOnlyList<IDbSheetRevision> All => [.. Tables, .. Sections, .. Rows, .. ColumnBlocks];

    public int Count => Tables.Count + Sections.Count + Rows.Count + ColumnBlocks.Count;
}
