using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Reads the cells of the rows that were on a sheet at a version, in one query: each cell joined to its
/// row's revision at that moment. Nothing is loaded as an entity and no template is read beyond the
/// cell's column (and its caption when labels are asked for).
/// </summary>
public sealed class PublishedCellReader(PuSpecSheetDbContext db)
{
    /// <param name="version">The version to read.</param>
    /// <param name="scope">What to read, or null for the whole sheet.</param>
    /// <param name="includeCaptions">Whether to read each cell's caption.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    public async Task<IReadOnlyList<PublishedCellRecord>> ReadAsync(
        ResolvedSheetVersion version,
        PublishedSelectionScope? scope,
        bool includeCaptions,
        CancellationToken cancellationToken)
    {
        var query = Query(version, scope, includeCaptions);
        return query is null ? [] : await query.ToListAsync(cancellationToken);
    }

    /// <summary>The cell query, or null when the scope names nothing that is on the sheet.</summary>
    public IQueryable<PublishedCellRecord>? Query(ResolvedSheetVersion version, PublishedSelectionScope? scope, bool includeCaptions)
    {
        var cells = db.SheetCells
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

        var moment = version.PublishedAtUtc;
        return includeCaptions
            ? cells.SelectMany(
                cell => cell.SheetRow.Revisions.Where(revision => revision.Status == RevisionStatus.Published
                    && revision.PublishedAtUtc <= moment
                    && (revision.SupersededAtUtc == null || revision.SupersededAtUtc > moment)
                    && !revision.IsDeleted),
                (cell, revision) => new PublishedCellRecord(
                    revision.Id,
                    cell.SheetRowId,
                    cell.SheetRow.PublicId,
                    cell.SheetRow.SheetSectionId,
                    revision.DisplayOrder,
                    cell.Id,
                    cell.PublicId,
                    cell.SheetColumnBlockId,
                    cell.TemplateCell.Column,
                    cell.TemplateCell.Caption))
            : cells.SelectMany(
                cell => cell.SheetRow.Revisions.Where(revision => revision.Status == RevisionStatus.Published
                    && revision.PublishedAtUtc <= moment
                    && (revision.SupersededAtUtc == null || revision.SupersededAtUtc > moment)
                    && !revision.IsDeleted),
                (cell, revision) => new PublishedCellRecord(
                    revision.Id,
                    cell.SheetRowId,
                    cell.SheetRow.PublicId,
                    cell.SheetRow.SheetSectionId,
                    revision.DisplayOrder,
                    cell.Id,
                    cell.PublicId,
                    cell.SheetColumnBlockId,
                    cell.TemplateCell.Column,
                    null));
    }
}
