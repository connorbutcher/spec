using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Published.Lookups;
using PUSpecSheet.Application.Published.Versions;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Entities.Sheets;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Published.Queries;

/// <summary>
/// Reads the cells that belong to a lookup match, in one query, each joined to its row's revision at the
/// version. A match in a column block reads that block's cells, the rows' own cells and the header, and
/// leaves the table's other blocks alone. A match in a row's own cell reads its sections' rows and the header.
/// </summary>
public static class PublishedLookupCellQueries
{
    /// <param name="db">The database.</param>
    /// <param name="version">The version of the sheet the match is on.</param>
    /// <param name="hit">The cell that was found.</param>
    /// <param name="sectionIds">The sections whose rows belong to the match, or null for the whole table.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    public static async Task<List<PublishedLookupCellRecord>> ListAroundHitAsync(
        PuSpecSheetDbContext db,
        ResolvedSheetVersion version,
        PublishedLookupHit hit,
        IReadOnlyList<int>? sectionIds,
        CancellationToken cancellationToken)
    {
        return await Query(db, version, hit, sectionIds).ToListAsync(cancellationToken);
    }

    /// <summary>Every cell on the sheet at the version.</summary>
    public static async Task<List<PublishedLookupCellRecord>> ListOnSheetAsync(PuSpecSheetDbContext db, ResolvedSheetVersion version, CancellationToken cancellationToken)
    {
        return await QuerySheet(db, version).ToListAsync(cancellationToken);
    }

    /// <summary>The cells of the rows with the public identifiers.</summary>
    public static async Task<List<PublishedLookupCellRecord>> ListInRowsAsync(PuSpecSheetDbContext db, ResolvedSheetVersion version, IReadOnlyList<Guid> rowPublicIds, CancellationToken cancellationToken)
    {
        return await QueryRows(db, version, rowPublicIds).ToListAsync(cancellationToken);
    }

    /// <summary>The header cells of the tables.</summary>
    public static async Task<List<PublishedLookupCellRecord>> ListHeadersAsync(PuSpecSheetDbContext db, ResolvedSheetVersion version, IReadOnlyList<int> tableIds, CancellationToken cancellationToken)
    {
        return await QueryHeaders(db, version, tableIds).ToListAsync(cancellationToken);
    }

    public static IQueryable<PublishedLookupCellRecord> Query(
        PuSpecSheetDbContext db,
        ResolvedSheetVersion version,
        PublishedLookupHit hit,
        IReadOnlyList<int>? sectionIds)
    {
        var tableId = hit.TableId;
        var cells = db.SheetCells
            .AsNoTracking()
            .Where(cell => cell.SheetRow.SheetSection.SheetTableId == tableId);

        if (hit.ColumnBlockId is { } blockId)
        {
            // The whole header comes too, so the block is named the same way as when rows are read by id.
            cells = cells.Where(cell => cell.SheetColumnBlockId == null
                || cell.SheetColumnBlockId == blockId
                || cell.SheetRow.SheetSection.TemplateSection.Role == SectionRole.Header);
        }
        else if (sectionIds is not null)
        {
            // The header comes too: it holds the headings and the cells that say which part a block is.
            cells = cells.Where(cell => sectionIds.Contains(cell.SheetRow.SheetSectionId)
                || cell.SheetRow.SheetSection.TemplateSection.Role == SectionRole.Header);
        }

        return Project(cells, version);
    }

    /// <summary>Every cell of the sheet's rows at the version, headings included.</summary>
    public static IQueryable<PublishedLookupCellRecord> QuerySheet(PuSpecSheetDbContext db, ResolvedSheetVersion version)
    {
        var sheetId = version.SheetId;
        var cells = db.SheetCells
            .AsNoTracking()
            .Where(cell => cell.SheetRow.SheetSection.SheetTable.SheetId == sheetId);

        return Project(cells, version);
    }

    /// <summary>The cells of some rows of the sheet, found by the rows' public identifiers.</summary>
    public static IQueryable<PublishedLookupCellRecord> QueryRows(PuSpecSheetDbContext db, ResolvedSheetVersion version, IReadOnlyList<Guid> rowPublicIds)
    {
        var sheetId = version.SheetId;
        var cells = db.SheetCells
            .AsNoTracking()
            .Where(cell => rowPublicIds.Contains(cell.SheetRow.PublicId)
                && cell.SheetRow.SheetSection.SheetTable.SheetId == sheetId);

        return Project(cells, version);
    }

    /// <summary>The cells of some tables' headers: their headings, and the cells that name each column block.</summary>
    public static IQueryable<PublishedLookupCellRecord> QueryHeaders(PuSpecSheetDbContext db, ResolvedSheetVersion version, IReadOnlyList<int> tableIds)
    {
        var cells = db.SheetCells
            .AsNoTracking()
            .Where(cell => tableIds.Contains(cell.SheetRow.SheetSection.SheetTableId)
                && cell.SheetRow.SheetSection.TemplateSection.Role == SectionRole.Header);

        return Project(cells, version);
    }

    private static IQueryable<PublishedLookupCellRecord> Project(IQueryable<SheetCell> cells, ResolvedSheetVersion version)
    {
        // The sheet on the revision lets SQL Server find the revisions through the (sheet, published) index.
        var sheetId = version.SheetId;
        var moment = version.PublishedAtUtc;
        return cells.SelectMany(
            cell => cell.SheetRow.Revisions.Where(revision => revision.SheetId == sheetId
                && revision.Status == RevisionStatus.Published
                && revision.PublishedAtUtc <= moment
                && (revision.SupersededAtUtc == null || revision.SupersededAtUtc > moment)
                && !revision.IsDeleted),
            (cell, revision) => new PublishedLookupCellRecord(
                revision.Id,
                cell.SheetRowId,
                cell.SheetRow.PublicId,
                cell.SheetRow.SheetSectionId,
                revision.DisplayOrder,
                cell.Id,
                cell.PublicId,
                cell.SheetColumnBlockId,
                cell.TemplateCell.Column,
                cell.TemplateCell.CellType.Kind,
                cell.TemplateCell.Caption,
                cell.TemplateCell.LookupKey,
                cell.SheetRow.SheetSection.TemplateSection.Role == SectionRole.Header));
    }
}
