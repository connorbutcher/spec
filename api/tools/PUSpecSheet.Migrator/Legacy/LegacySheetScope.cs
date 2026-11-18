using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.SpecSheets;

namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// The queries that narrow the copied-sheet tables to one sheet's copies and what is in them. Each narrows the one
/// before, so everything read through them is of this sheet's copies only. Deleted tables, groups, rows and cells are
/// left out: the copy doesn't show them.
/// </summary>
internal sealed record LegacySheetScope
{
    public required IQueryable<DbSpecSheet> Copies { get; init; }

    public required IQueryable<DbSpecSheetTable> Tables { get; init; }

    public required IQueryable<DbSpecSheetTableGroup> Groups { get; init; }

    public required IQueryable<DbSpecSheetTableRow> Rows { get; init; }

    public required IQueryable<DbSpecSheetTableCell> Cells { get; init; }

    public void Deconstruct(
        out IQueryable<DbSpecSheet> copies,
        out IQueryable<DbSpecSheetTable> tables,
        out IQueryable<DbSpecSheetTableGroup> groups,
        out IQueryable<DbSpecSheetTableRow> rows,
        out IQueryable<DbSpecSheetTableCell> cells)
    {
        copies = Copies;
        tables = Tables;
        groups = Groups;
        rows = Rows;
        cells = Cells;
    }

    public static LegacySheetScope Of(PUSpecSheetDbContext dbContext, LegacySheet sheet)
    {
        var copies = dbContext.SpecSheets.AsNoTracking()
            .Where(copy => copy.SpecSheetPhaseId == sheet.PhaseId && copy.SpecSheetTypeId == sheet.SheetTypeId);
        var tables = dbContext.SpecSheetTables.AsNoTracking()
            .Where(table => !table.IsDeleted && copies.Any(copy => copy.Id == table.SpecSheetId));
        var groups = dbContext.SpecSheetTableGroups.AsNoTracking()
            .Where(group => !group.IsDeleted && tables.Any(table => table.Id == group.SpecSheetTableId));
        var rows = dbContext.SpecSheetTableRows.AsNoTracking()
            .Where(row => !row.IsDeleted && groups.Any(group => group.Id == row.SpecSheetTableGroupId));
        var cells = dbContext.SpecSheetTableCells.AsNoTracking()
            .Where(cell => !cell.IsDeleted && rows.Any(row => row.Id == cell.SpecSheetTableRowId));

        return new LegacySheetScope { Copies = copies, Tables = tables, Groups = groups, Rows = rows, Cells = cells };
    }
}
