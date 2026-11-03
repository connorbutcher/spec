using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Every revision of one kind of item on a sheet, found by the sheet each revision carries, and the callers
/// add what they want of them (drafts, current ones, ...).
/// </summary>
internal static class SheetScopeQueryExtensions
{
    public static IQueryable<SheetTableRevision> TableRevisionsOf(this PuSpecSheetDbContext db, int sheetId)
    {
        return db.SheetTableRevisions.Where(revision => revision.SheetId == sheetId);
    }

    public static IQueryable<SheetSectionRevision> SectionRevisionsOf(this PuSpecSheetDbContext db, int sheetId)
    {
        return db.SheetSectionRevisions.Where(revision => revision.SheetId == sheetId);
    }

    public static IQueryable<SheetRowRevision> RowRevisionsOf(this PuSpecSheetDbContext db, int sheetId)
    {
        return db.SheetRowRevisions.Where(revision => revision.SheetId == sheetId);
    }

    public static IQueryable<SheetColumnBlockRevision> ColumnBlockRevisionsOf(this PuSpecSheetDbContext db, int sheetId)
    {
        return db.SheetColumnBlockRevisions.Where(revision => revision.SheetId == sheetId);
    }
}
