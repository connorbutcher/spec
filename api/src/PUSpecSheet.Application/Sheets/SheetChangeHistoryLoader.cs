using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Reads what <see cref="SheetChangeHistoryCalculator"/> needs: every published revision of the sheet's
/// rows, sections and column blocks up to the moment viewed, and the values of those row revisions. The
/// live view's history is kept until the sheet's next version (see <see cref="SheetChangeHistoryCache"/>).
/// </summary>
public sealed class SheetChangeHistoryLoader(PuSpecSheetDbContext db, RowValueStore valueStore, SheetChangeHistoryCache cache)
{
    public async Task<SheetChangeHistory> LoadAsync(
        SheetSnapshot sheet,
        DateTime? moment,
        CancellationToken cancellationToken)
    {
        // Changes come from published revisions, and those only exist once a version has been published.
        if (sheet.Versions.Count == 0)
        {
            return SheetChangeHistory.Empty;
        }

        if (moment is not null)
        {
            return await BuildAsync(sheet, moment, cancellationToken);
        }

        var key = string.Create(CultureInfo.InvariantCulture, $"{sheet.Sheet.Id}:{sheet.Versions[^1].Id}");
        if (cache.TryGet(key, out var kept))
        {
            return kept;
        }

        var history = await BuildAsync(sheet, moment, cancellationToken);
        cache.Set(key, history, history.Count);
        return history;
    }

    private async Task<SheetChangeHistory> BuildAsync(SheetSnapshot sheet, DateTime? moment, CancellationToken cancellationToken)
    {
        // Found through the items' ids, like the sheet's own revisions (see SheetSnapshotLoader).
        var rowIds = sheet.Rows.Select(row => row.Id).ToList();
        var sectionIds = sheet.Sections.Select(section => section.Id).ToList();
        var columnBlockIds = sheet.ColumnBlocks.Select(block => block.Id).ToList();

        var rowRevisions = await Published(
                db.SheetRowRevisions.Where(revision => rowIds.Contains(revision.SheetRowId)),
                moment)
            .ToListAsync(cancellationToken);
        var sectionRevisions = await Published(
                db.SheetSectionRevisions.Where(revision => sectionIds.Contains(revision.SheetSectionId)),
                moment)
            .ToListAsync(cancellationToken);
        var columnBlockRevisions = await Published(
                db.SheetColumnBlockRevisions.Where(revision => columnBlockIds.Contains(revision.SheetColumnBlockId)),
                moment)
            .ToListAsync(cancellationToken);
        var values = await valueStore.LoadAsync(rowRevisions.Select(revision => revision.Id).ToList(), cancellationToken);

        return SheetChangeHistoryCalculator.Calculate(
            rowRevisions,
            sectionRevisions,
            columnBlockRevisions,
            values,
            sheet.Versions.ToDictionary(version => version.Id, version => version.VersionNumber),
            sheet.Rows.ToDictionary(row => row.Id, row => row.SheetSectionId),
            sheet.Sections.ToDictionary(section => section.Id, section => section.ParentSheetSectionId));
    }

    private static IQueryable<TRevision> Published<TRevision>(IQueryable<TRevision> query, DateTime? moment)
        where TRevision : class, ISheetRevision
    {
        return query
            .AsNoTracking()
            .Where(revision => revision.Status == RevisionStatus.Published
                && (moment == null || revision.PublishedAtUtc <= moment));
    }
}
