using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Works out <see cref="SheetChangeHistory"/> by walking each item's published revisions in order and
/// noting where a value, position or existence differs from the revision before it.
/// </summary>
public sealed class SheetChangeHistoryLoader(PuSpecSheetDbContext db, RowValueStore valueStore)
{
    public async Task<SheetChangeHistory> LoadAsync(
        SheetSnapshot sheet,
        DateTime? moment,
        CancellationToken cancellationToken)
    {
        var versionNumbers = sheet.Versions.ToDictionary(version => version.Id, version => version.VersionNumber);
        var sheetId = sheet.Sheet.Id;

        var rowRevisions = await Published(
                db.SheetRowRevisions.Where(revision => revision.SheetRow.SheetSection.SheetTable.SheetId == sheetId),
                moment)
            .ToListAsync(cancellationToken);
        var sectionRevisions = await Published(
                db.SheetSectionRevisions.Where(revision => revision.SheetSection.SheetTable.SheetId == sheetId),
                moment)
            .ToListAsync(cancellationToken);
        var blockRevisions = await Published(
                db.SheetColumnBlockRevisions.Where(revision => revision.SheetColumnBlock.SheetTable.SheetId == sheetId),
                moment)
            .ToListAsync(cancellationToken);
        var values = await valueStore.LoadAsync(rowRevisions.Select(revision => revision.Id).ToList(), cancellationToken);

        var history = new SheetChangeHistory();
        var rowSections = sheet.Rows.ToDictionary(row => row.Id, row => row.SheetSectionId);
        var sectionParents = sheet.Sections.ToDictionary(section => section.Id, section => section.ParentSheetSectionId);

        SheetChangeDto? Change(ISheetRevision revision)
        {
            if (revision.SheetVersionId is not { } versionId
                || revision.PublishedAtUtc is not { } at
                || !versionNumbers.TryGetValue(versionId, out var number))
            {
                return null;
            }

            return new SheetChangeDto(
                number,
                DateTime.SpecifyKind(at, DateTimeKind.Utc),
                sheet.UserNames.GetValueOrDefault(revision.AuthorUserId, "Unknown user"));
        }

        foreach (var group in rowRevisions.GroupBy(revision => revision.SheetRowId))
        {
            SheetRowRevision? previous = null;
            foreach (var revision in group.OrderBy(candidate => candidate.RevisionNumber))
            {
                if (Change(revision) is { } change)
                {
                    var current = values.GetValueOrDefault(revision.Id) ?? [];
                    var before = previous is null ? [] : values.GetValueOrDefault(previous.Id) ?? [];
                    var cellsChanged = false;
                    foreach (var cellId in current.Keys.Union(before.Keys))
                    {
                        if (!Same(current.GetValueOrDefault(cellId), before.GetValueOrDefault(cellId)))
                        {
                            history.Cells[cellId] = change;
                            cellsChanged = true;
                        }
                    }

                    if (cellsChanged || previous is null)
                    {
                        history.Rows[group.Key] = change;
                    }

                    var restructured = previous is null
                        || revision.IsDeleted != previous.IsDeleted
                        || revision.DisplayOrder != previous.DisplayOrder;
                    if (restructured && rowSections.TryGetValue(group.Key, out var sectionId))
                    {
                        history.Sections[sectionId] = change;
                    }
                }

                previous = revision;
            }
        }

        foreach (var revision in sectionRevisions.OrderBy(candidate => candidate.PublishedAtUtc))
        {
            if (sectionParents.GetValueOrDefault(revision.SheetSectionId) is { } parentId
                && Change(revision) is { } change)
            {
                history.Sections[parentId] = change;
            }
        }

        foreach (var revision in blockRevisions.OrderBy(candidate => candidate.PublishedAtUtc))
        {
            if (Change(revision) is { } change)
            {
                history.ColumnBlocks[revision.SheetColumnBlockId] = change;
            }
        }

        return history;
    }

    private static IQueryable<TRevision> Published<TRevision>(IQueryable<TRevision> query, DateTime? moment)
        where TRevision : class, ISheetRevision
    {
        return query
            .AsNoTracking()
            .Where(revision => revision.Status == RevisionStatus.Published
                && (moment == null || revision.PublishedAtUtc <= moment));
    }

    private static bool Same(CellValueBag? left, CellValueBag? right)
    {
        return left?.Text == right?.Text
            && left?.Number == right?.Number
            && left?.Date == right?.Date
            && left?.Boolean == right?.Boolean
            && left?.OptionId == right?.OptionId;
    }
}
