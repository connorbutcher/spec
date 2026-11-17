using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Published.Queries;
using PUSpecSheet.DataAccess.Published.Sheets;
using PUSpecSheet.DataAccess.Published.Versions;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Published;

public sealed class PublishedCellDataService : IPublishedCellDataService
{
    private readonly PuSpecSheetDbContext _db;

    public PublishedCellDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PublishedCellRecord>> ReadAsync(
        ResolvedSheetVersion version,
        PublishedSelectionScope? scope,
        bool includeCaptions,
        CancellationToken cancellationToken)
    {
        var query = Query(version, scope, includeCaptions);
        return query is null ? [] : await query.ToListAsync(cancellationToken);
    }

    public IQueryable<PublishedCellRecord>? Query(ResolvedSheetVersion version, PublishedSelectionScope? scope, bool includeCaptions)
    {
        var cells = _db.SheetCells
            .AsNoTracking()
            .Where(cell => cell.SheetRow.SheetSection.SheetTable.SheetId == version.SheetId);

        if (scope is not null)
        {
            var filter = PublishedCellFilter.For(scope);
            if (filter is null)
            {
                return null;
            }

            cells = cells.Where(filter);
        }

        return includeCaptions ? WithCaptions(cells, version) : WithoutCaptions(cells, version);
    }

    // The sheet on the revision lets SQL Server find the revisions through the (sheet, published) index.
    private static IQueryable<PublishedCellRecord> WithCaptions(IQueryable<DbSheetCell> cells, ResolvedSheetVersion version)
    {
        var sheetId = version.SheetId;
        var moment = version.PublishedAtUtc;
        return cells.SelectMany(
            cell => cell.SheetRow.Revisions.Where(revision => revision.SheetId == sheetId
                && revision.Status == RevisionStatus.Published
                && revision.PublishedAtUtc <= moment
                && (revision.SupersededAtUtc == null || revision.SupersededAtUtc > moment)
                && !revision.IsDeleted),
            (cell, revision) => new PublishedCellRecord
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
                Caption = cell.TemplateCell.Caption,
            });
    }

    private static IQueryable<PublishedCellRecord> WithoutCaptions(IQueryable<DbSheetCell> cells, ResolvedSheetVersion version)
    {
        var sheetId = version.SheetId;
        var moment = version.PublishedAtUtc;
        return cells.SelectMany(
            cell => cell.SheetRow.Revisions.Where(revision => revision.SheetId == sheetId
                && revision.Status == RevisionStatus.Published
                && revision.PublishedAtUtc <= moment
                && (revision.SupersededAtUtc == null || revision.SupersededAtUtc > moment)
                && !revision.IsDeleted),
            (cell, revision) => new PublishedCellRecord
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
            });
    }
}
