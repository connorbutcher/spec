using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Works out <see cref="SheetChangeHistory"/> by walking each item's published revisions in order and
/// noting where a value, position or existence differs from the revision before it.
/// </summary>
internal static class SheetChangeHistoryCalculator
{
    /// <param name="rowRevisions">Every published revision of the sheet's rows up to the moment viewed.</param>
    /// <param name="sectionRevisions">The same for its sections.</param>
    /// <param name="columnBlockRevisions">The same for its column blocks.</param>
    /// <param name="values">The cell values of those row revisions, by revision id and then sheet cell id.</param>
    /// <param name="versionNumbers">The sheet's version numbers by version id.</param>
    /// <param name="rowSections">The section each row is in, by row id.</param>
    /// <param name="sectionParents">The parent of each section (null at the top), by section id.</param>
    public static SheetChangeHistory Calculate(
        IReadOnlyList<SheetRowRevision> rowRevisions,
        IReadOnlyList<SheetSectionRevision> sectionRevisions,
        IReadOnlyList<SheetColumnBlockRevision> columnBlockRevisions,
        IReadOnlyDictionary<int, Dictionary<int, CellValueBag>> values,
        IReadOnlyDictionary<int, int> versionNumbers,
        IReadOnlyDictionary<int, int> rowSections,
        IReadOnlyDictionary<int, int?> sectionParents)
    {
        var history = new SheetChangeHistory();

        SheetChange? Change(ISheetRevision revision)
        {
            if (revision.SheetVersionId is not { } versionId
                || revision.PublishedAtUtc is not { } at
                || !versionNumbers.TryGetValue(versionId, out var number))
            {
                return null;
            }

            return new SheetChange(number, DateTime.SpecifyKind(at, DateTimeKind.Utc), revision.AuthorUserId);
        }

        // Rows are taken in the order they were first published, whatever order the revisions arrive in.
        foreach (var group in rowRevisions.OrderBy(revision => revision.Id).GroupBy(revision => revision.SheetRowId))
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

        foreach (var revision in sectionRevisions.OrderBy(candidate => candidate.PublishedAtUtc).ThenBy(candidate => candidate.Id))
        {
            if (sectionParents.GetValueOrDefault(revision.SheetSectionId) is { } parentId
                && Change(revision) is { } change)
            {
                history.Sections[parentId] = change;
            }
        }

        foreach (var revision in columnBlockRevisions.OrderBy(candidate => candidate.PublishedAtUtc).ThenBy(candidate => candidate.Id))
        {
            if (Change(revision) is { } change)
            {
                history.ColumnBlocks[revision.SheetColumnBlockId] = change;
            }
        }

        return history;
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
