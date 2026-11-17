using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Drafts;

/// <summary>The drafts of every kind that one publish or submission takes from a sheet, tracked.</summary>
public sealed record SheetDraftSet
{
    public required IReadOnlyList<DbSheetTableRevision> Tables { get; init; }

    public required IReadOnlyList<DbSheetSectionRevision> Sections { get; init; }

    public required IReadOnlyList<DbSheetRowRevision> Rows { get; init; }

    public required IReadOnlyList<DbSheetColumnBlockRevision> ColumnBlocks { get; init; }

    public IReadOnlyList<IDbSheetRevision> All => [.. Tables, .. Sections, .. Rows, .. ColumnBlocks];

    public int Count => Tables.Count + Sections.Count + Rows.Count + ColumnBlocks.Count;
}
