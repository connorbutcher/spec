using PUSpecSheet.DataAccess.Sheets.Values;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.History;

internal static class SheetChangeHistoryCalculator
{
    public static SheetChangeHistory Calculate(
        IReadOnlyList<DbSheetRowRevision> rowRevisions,
        IReadOnlyList<DbSheetSectionRevision> sectionRevisions,
        IReadOnlyList<DbSheetColumnBlockRevision> columnBlockRevisions,
        IReadOnlyDictionary<int, Dictionary<int, CellValueBag>> values,
        IReadOnlyDictionary<int, int> versionNumbers,
        IReadOnlyDictionary<int, int> rowSections,
        IReadOnlyDictionary<int, int?> sectionParents)
    {
        var history = new SheetChangeHistory();
        foreach (var group in rowRevisions.GroupBy(revision => revision.SheetRowId))
        {
            RecordRow(history, group, values, versionNumbers, rowSections);
        }

        foreach (var revision in sectionRevisions)
        {
            if (sectionParents.GetValueOrDefault(revision.SheetSectionId) is { } parentId
                && ChangeOf(revision, versionNumbers) is { } change)
            {
                KeepNewest(history.Sections, parentId, change);
            }
        }

        foreach (var revision in columnBlockRevisions.OrderBy(candidate => candidate.PublishedAtUtc).ThenBy(candidate => candidate.Id))
        {
            if (ChangeOf(revision, versionNumbers) is { } change)
            {
                history.ColumnBlocks[revision.SheetColumnBlockId] = change;
            }
        }

        return history;
    }

    private static void RecordRow(
        SheetChangeHistory history,
        IGrouping<int, DbSheetRowRevision> row,
        IReadOnlyDictionary<int, Dictionary<int, CellValueBag>> values,
        IReadOnlyDictionary<int, int> versionNumbers,
        IReadOnlyDictionary<int, int> rowSections)
    {
        DbSheetRowRevision? previous = null;
        foreach (var revision in row.OrderBy(candidate => candidate.RevisionNumber))
        {
            if (ChangeOf(revision, versionNumbers) is { } change)
            {
                var cellsChanged = RecordCells(history, revision, previous, values, change);
                if (cellsChanged || previous is null)
                {
                    history.Rows[row.Key] = change;
                }

                var restructured = previous is null
                    || revision.IsDeleted != previous.IsDeleted
                    || revision.DisplayOrder != previous.DisplayOrder;
                if (restructured && rowSections.TryGetValue(row.Key, out var sectionId))
                {
                    KeepNewest(history.Sections, sectionId, change);
                }
            }

            previous = revision;
        }
    }

    private static bool RecordCells(
        SheetChangeHistory history,
        DbSheetRowRevision revision,
        DbSheetRowRevision? previous,
        IReadOnlyDictionary<int, Dictionary<int, CellValueBag>> values,
        SheetChangeRecord change)
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

        return cellsChanged;
    }

    private static SheetChangeRecord? ChangeOf(IDbSheetRevision revision, IReadOnlyDictionary<int, int> versionNumbers)
    {
        if (revision.SheetVersionId is not { } versionId
            || revision.PublishedAtUtc is not { } at
            || !versionNumbers.TryGetValue(versionId, out var number))
        {
            return null;
        }

        return new SheetChangeRecord
        {
            VersionNumber = number,
            AtUtc = DateTime.SpecifyKind(at, DateTimeKind.Utc),
            AuthorUserId = revision.AuthorUserId,
        };
    }

    private static void KeepNewest(Dictionary<int, SheetChangeRecord> changes, int itemId, SheetChangeRecord change)
    {
        if (!changes.TryGetValue(itemId, out var recorded) || change.VersionNumber >= recorded.VersionNumber)
        {
            changes[itemId] = change;
        }
    }

    private static bool Same(CellValueBag? left, CellValueBag? right)
    {
        return left?.Text == right?.Text
            && left?.Number == right?.Number
            && left?.Date == right?.Date
            && left?.Boolean == right?.Boolean
            && left?.OptionId == right?.OptionId
            && SlotValues.Same(left?.Slots, right?.Slots)
            && left?.Settings == right?.Settings;
    }
}
