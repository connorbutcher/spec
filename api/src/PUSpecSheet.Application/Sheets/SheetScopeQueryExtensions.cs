using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Every revision of one kind of item on a sheet. Each kind reaches its sheet through different tables, so
/// the path is written once here and the callers add what they want of them (drafts, current ones, ...).
/// </summary>
internal static class SheetScopeQueryExtensions
{
    public static IQueryable<SheetTableRevision> TableRevisionsOf(this PuSpecSheetDbContext db, int sheetId)
    {
        return db.SheetTableRevisions.Where(revision => revision.SheetTable.SheetId == sheetId);
    }

    public static IQueryable<SheetSectionRevision> SectionRevisionsOf(this PuSpecSheetDbContext db, int sheetId)
    {
        return db.SheetSectionRevisions.Where(revision => revision.SheetSection.SheetTable.SheetId == sheetId);
    }

    public static IQueryable<SheetRowRevision> RowRevisionsOf(this PuSpecSheetDbContext db, int sheetId)
    {
        return db.SheetRowRevisions.Where(revision => revision.SheetRow.SheetSection.SheetTable.SheetId == sheetId);
    }

    public static IQueryable<SheetColumnBlockRevision> ColumnBlockRevisionsOf(this PuSpecSheetDbContext db, int sheetId)
    {
        return db.SheetColumnBlockRevisions.Where(revision => revision.SheetColumnBlock.SheetTable.SheetId == sheetId);
    }
}
