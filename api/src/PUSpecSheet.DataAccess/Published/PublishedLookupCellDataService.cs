using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Published.Lookups;
using PUSpecSheet.DataAccess.Published.Versions;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Published;

public sealed class PublishedLookupCellDataService : IPublishedLookupCellDataService
{
    private readonly PuSpecSheetDbContext _db;

    public PublishedLookupCellDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<List<PublishedLookupCellRecord>> ListAroundHitAsync(
        ResolvedSheetVersion version,
        PublishedLookupHit hit,
        IReadOnlyList<int>? sectionIds,
        CancellationToken cancellationToken)
    {
        return await Query(version, hit, sectionIds).ToListAsync(cancellationToken);
    }

    public async Task<List<PublishedLookupCellRecord>> ListOnSheetAsync(ResolvedSheetVersion version, CancellationToken cancellationToken)
    {
        return await QuerySheet(version).ToListAsync(cancellationToken);
    }

    public async Task<List<PublishedLookupCellRecord>> ListInRowsAsync(ResolvedSheetVersion version, IReadOnlyList<Guid> rowPublicIds, CancellationToken cancellationToken)
    {
        return await QueryRows(version, rowPublicIds).ToListAsync(cancellationToken);
    }

    public async Task<List<PublishedLookupCellRecord>> ListHeadersAsync(ResolvedSheetVersion version, IReadOnlyList<int> tableIds, CancellationToken cancellationToken)
    {
        return await QueryHeaders(version, tableIds).ToListAsync(cancellationToken);
    }

    public IQueryable<PublishedLookupCellRecord> Query(
        ResolvedSheetVersion version,
        PublishedLookupHit hit,
        IReadOnlyList<int>? sectionIds)
    {
        var tableId = hit.TableId;
        var cells = _db.SheetCells
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

    public IQueryable<PublishedLookupCellRecord> QuerySheet(ResolvedSheetVersion version)
    {
        var sheetId = version.SheetId;
        var cells = _db.SheetCells
            .AsNoTracking()
            .Where(cell => cell.SheetRow.SheetSection.SheetTable.SheetId == sheetId);

        return Project(cells, version);
    }

    public IQueryable<PublishedLookupCellRecord> QueryRows(ResolvedSheetVersion version, IReadOnlyList<Guid> rowPublicIds)
    {
        var sheetId = version.SheetId;
        var cells = _db.SheetCells
            .AsNoTracking()
            .Where(cell => rowPublicIds.Contains(cell.SheetRow.PublicId)
                && cell.SheetRow.SheetSection.SheetTable.SheetId == sheetId);

        return Project(cells, version);
    }

    public IQueryable<PublishedLookupCellRecord> QueryHeaders(ResolvedSheetVersion version, IReadOnlyList<int> tableIds)
    {
        var cells = _db.SheetCells
            .AsNoTracking()
            .Where(cell => tableIds.Contains(cell.SheetRow.SheetSection.SheetTableId)
                && cell.SheetRow.SheetSection.TemplateSection.Role == SectionRole.Header);

        return Project(cells, version);
    }

    private static IQueryable<PublishedLookupCellRecord> Project(IQueryable<DbSheetCell> cells, ResolvedSheetVersion version)
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
            (cell, revision) => new PublishedLookupCellRecord
            {
                RowRevisionId = revision.Id,
                RowId = cell.SheetRowId,
                RowPublicId = cell.SheetRow.PublicId,
                SectionId = cell.SheetRow.SheetSectionId,
                RowOrder = revision.DisplayOrder,
                CellId = cell.Id,
                CellPublicId = cell.PublicId,
                ColumnBlockId = cell.SheetColumnBlockId,
                Column = cell.TemplateCell.Column,
                Kind = cell.TemplateCell.CellType.Kind,
                Caption = cell.TemplateCell.Caption,
                LookupKey = cell.TemplateCell.LookupKey,
                IsHeader = cell.SheetRow.SheetSection.TemplateSection.Role == SectionRole.Header,
            });
    }
}
