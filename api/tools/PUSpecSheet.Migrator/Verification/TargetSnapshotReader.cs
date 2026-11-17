using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Verification;

/// <summary>
/// Reads a migrated sheet back from the revision tables as it stood at a moment, into the same
/// <see cref="SheetSnapshot"/> shape a copy is read into. At a moment it sees the published revision whose range covers
/// it; with no moment, each item's draft where it has one, otherwise its current revision. An item is on the sheet when
/// its revision doesn't remove it and nothing it is in was removed.
/// </summary>
internal static class TargetSnapshotReader
{
    public static async Task<SheetSnapshot> ReadAsync(PUSpecSheetDbContext db, int sheetId, DateTime? moment, CancellationToken cancellationToken)
    {
        var sheetTables = db.SheetTables.AsNoTracking().Where(table => table.SheetId == sheetId);
        var sheetGroups = db.SheetTableGroups.AsNoTracking().Where(group => sheetTables.Any(table => table.Id == group.SheetTableId));
        var sheetRows = db.SheetTableRows.AsNoTracking().Where(row => sheetGroups.Any(group => group.Id == row.SheetTableGroupId));

        var tableRevisions = (await db.SheetTableRevisions.AsNoTracking()
                .Where(revision => revision.SheetId == sheetId)
                .InView(moment)
                .Select(revision => new
                {
                    ItemId = revision.SheetTableId,
                    revision.Status,
                    revision.IsDeleted,
                    revision.DisplayOrder,
                    revision.Title,
                    revision.ConfigurationOverride,
                    revision.WidgetValues,
                    revision.Notes
                })
                .ToListAsync(cancellationToken))
            .ShownPerItem(revision => revision.ItemId, revision => revision.Status);
        var tableItems = await sheetTables
            .Select(table => new { table.Id, table.ReferenceId, table.TableTemplateId })
            .ToDictionaryAsync(table => table.Id, cancellationToken);

        var groupRevisions = (await db.SheetTableGroupRevisions.AsNoTracking()
                .Where(revision => revision.SheetId == sheetId)
                .InView(moment)
                .Select(revision => new
                {
                    ItemId = revision.SheetTableGroupId,
                    revision.Status,
                    revision.IsDeleted,
                    revision.DisplayOrder,
                    revision.WidgetValues,
                    revision.LinkedSheetTableGroupReferenceId
                })
                .ToListAsync(cancellationToken))
            .ShownPerItem(revision => revision.ItemId, revision => revision.Status);
        var groupItems = await sheetGroups
            .Select(group => new { group.Id, group.ReferenceId, group.SheetTableId, group.TableGroupTemplateId })
            .ToDictionaryAsync(group => group.Id, cancellationToken);

        var rowRevisions = (await db.SheetTableRowRevisions.AsNoTracking()
                .Where(revision => revision.SheetId == sheetId)
                .InView(moment)
                .Select(revision => new { revision.Id, ItemId = revision.SheetTableRowId, revision.Status, revision.IsDeleted, revision.DisplayOrder })
                .ToListAsync(cancellationToken))
            .ShownPerItem(revision => revision.ItemId, revision => revision.Status);
        var rowItems = await sheetRows
            .Select(row => new { row.Id, row.ReferenceId, row.SheetTableGroupId, row.TableRowTemplateId })
            .ToDictionaryAsync(row => row.Id, cancellationToken);

        // What is on the sheet: not removed, and nothing it is in removed either.
        var tables = tableRevisions.Values
            .Where(table => !table.IsDeleted)
            .Select(table => new SnapshotTable
            {
                Id = table.ItemId,
                ReferenceId = tableItems[table.ItemId].ReferenceId,
                TableTemplateId = tableItems[table.ItemId].TableTemplateId,
                Title = table.Title,
                DisplayOrder = table.DisplayOrder,
                ConfigurationOverride = table.ConfigurationOverride,
                WidgetValues = table.WidgetValues?.ToList() ?? [],
                Notes = table.Notes?.ToList() ?? []
            })
            .ToList();
        var tableIds = tables.Select(table => table.Id).ToHashSet();

        var groups = groupRevisions.Values
            .Where(group => !group.IsDeleted && tableIds.Contains(groupItems[group.ItemId].SheetTableId))
            .Select(group => new SnapshotGroup
            {
                Id = group.ItemId,
                ReferenceId = groupItems[group.ItemId].ReferenceId,
                TableId = groupItems[group.ItemId].SheetTableId,
                TableGroupTemplateId = groupItems[group.ItemId].TableGroupTemplateId,
                DisplayOrder = group.DisplayOrder,
                WidgetValues = group.WidgetValues?.ToList() ?? [],
                LinkedGroupReferenceId = group.LinkedSheetTableGroupReferenceId
            })
            .ToList();
        var groupIds = groups.Select(group => group.Id).ToHashSet();

        var shownRows = rowRevisions.Values
            .Where(row => !row.IsDeleted && groupIds.Contains(rowItems[row.ItemId].SheetTableGroupId))
            .ToList();
        var rows = shownRows
            .Select(row => new SnapshotRow
            {
                Id = row.ItemId,
                ReferenceId = rowItems[row.ItemId].ReferenceId,
                GroupId = rowItems[row.ItemId].SheetTableGroupId,
                TableRowTemplateId = rowItems[row.ItemId].TableRowTemplateId,
                DisplayOrder = row.DisplayOrder
            })
            .ToList();

        var shownRowIds = shownRows.Select(row => row.ItemId).ToHashSet();
        var contents = await ReadContentsAsync(db, sheetId, shownRows.Select(row => row.Id).ToHashSet(), cancellationToken);
        var cells = (await db.SheetTableCells.AsNoTracking()
                .Where(cell => sheetRows.Any(row => row.Id == cell.SheetTableRowId))
                .Select(cell => new { cell.Id, cell.ReferenceId, cell.SheetTableRowId, cell.TableCellTemplateId })
                .ToListAsync(cancellationToken))
            .Where(cell => shownRowIds.Contains(cell.SheetTableRowId))
            .Select(cell => new SnapshotCell
            {
                Id = cell.Id,
                ReferenceId = cell.ReferenceId,
                RowId = cell.SheetTableRowId,
                TableCellTemplateId = cell.TableCellTemplateId,
                Content = contents.TryGetValue(cell.Id, out var content) ? content.Build() : CellContent.Empty
            })
            .ToList();

        var propertyItems = await db.SheetProperties.AsNoTracking()
            .Where(property => property.SheetId == sheetId)
            .ToDictionaryAsync(property => property.Id, property => property.SpecSheetTypePropertyId, cancellationToken);
        var properties = (await db.SheetPropertyRevisions.AsNoTracking()
                .Where(revision => revision.SheetId == sheetId)
                .InView(moment)
                .Select(revision => new { ItemId = revision.SheetPropertyId, revision.Status, revision.IsDeleted, revision.PropertyValue })
                .ToListAsync(cancellationToken))
            .ShownPerItem(revision => revision.ItemId, revision => revision.Status);

        var linkedItems = await db.SheetLinkedTables.AsNoTracking()
            .Where(link => link.SheetId == sheetId)
            .Select(link => new { link.Id, link.TableReferenceId, link.LinkedByUserId, link.LinkedDateTime })
            .ToDictionaryAsync(link => link.Id, cancellationToken);
        var linkedTables = (await db.SheetLinkedTableRevisions.AsNoTracking()
                .Where(revision => revision.SheetId == sheetId)
                .InView(moment)
                .Select(revision => new { ItemId = revision.SheetLinkedTableId, revision.Status, revision.IsDeleted, revision.DisplayOrder })
                .ToListAsync(cancellationToken))
            .ShownPerItem(revision => revision.ItemId, revision => revision.Status);

        return new SheetSnapshot
        {
            Tables = tables,
            Groups = groups,
            Rows = rows,
            Cells = cells,
            Properties = properties.Values
                .Where(property => !property.IsDeleted)
                .ToDictionary(property => propertyItems[property.ItemId], property => property.PropertyValue),
            LinkedTables = linkedTables.Values
                .Where(link => !link.IsDeleted)
                .Select(link => new SnapshotLinkedTable
                {
                    TableReferenceId = linkedItems[link.ItemId].TableReferenceId,
                    DisplayOrder = link.DisplayOrder,
                    LinkedByUserId = linkedItems[link.ItemId].LinkedByUserId,
                    LinkedDateTime = linkedItems[link.ItemId].LinkedDateTime
                })
                .ToList(),
            ExternalReferences = moment is { } at ? await ReadExternalReferencesAsync(db, sheetId, at, cancellationToken) : null
        };
    }

    // External references have no drafts: as of a moment is the revision published at or before it and not yet replaced.
    private static async Task<List<SnapshotExternalReference>> ReadExternalReferencesAsync(
        PUSpecSheetDbContext db,
        int sheetId,
        DateTime moment,
        CancellationToken cancellationToken)
    {
        var referenceIds = await db.SheetExternalReferences.AsNoTracking()
            .Where(reference => reference.SheetId == sheetId)
            .ToDictionaryAsync(reference => reference.Id, reference => reference.ReferenceId, cancellationToken);

        return (await db.SheetExternalReferenceRevisions.AsNoTracking()
                .Where(revision => revision.SheetId == sheetId
                    && revision.PublishedDateTime <= moment
                    && (revision.SupersededDateTime == null || revision.SupersededDateTime > moment)
                    && !revision.IsDeleted)
                .Select(revision => new
                {
                    revision.SheetExternalReferenceId,
                    revision.ExternalId,
                    revision.SystemName,
                    revision.OwnerName,
                    revision.Url,
                    revision.Type,
                    revision.Grouping,
                    revision.SystemIconUrl,
                    revision.Description
                })
                .ToListAsync(cancellationToken))
            .Select(revision => new SnapshotExternalReference
            {
                ReferenceId = referenceIds[revision.SheetExternalReferenceId],
                ExternalId = revision.ExternalId,
                SystemName = revision.SystemName,
                OwnerName = revision.OwnerName,
                Url = revision.Url,
                Type = revision.Type,
                Grouping = revision.Grouping,
                SystemIconUrl = revision.SystemIconUrl,
                Description = revision.Description
            })
            .ToList();
    }

    /// <summary>
    /// The values of the row revisions shown, by cell. A row has one revision shown, so each cell has one set of values.
    /// </summary>
    private static async Task<Dictionary<int, CellContentParts>> ReadContentsAsync(
        PUSpecSheetDbContext db,
        int sheetId,
        HashSet<int> shownRevisionIds,
        CancellationToken cancellationToken)
    {
        var parts = new Dictionary<int, CellContentParts>();
        var sheetRevisions = db.SheetTableRowRevisions.AsNoTracking().Where(revision => revision.SheetId == sheetId);

        CellContentParts For(int cellId)
        {
            if (!parts.TryGetValue(cellId, out var cell))
            {
                cell = new CellContentParts();
                parts[cellId] = cell;
            }

            return cell;
        }

        foreach (var value in (await db.SheetTextValues.AsNoTracking()
                .Where(value => sheetRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId))
                .Select(value => new { value.SheetTableRowRevisionId, value.SheetTableCellId, value.Value })
                .ToListAsync(cancellationToken))
            .Where(value => shownRevisionIds.Contains(value.SheetTableRowRevisionId)))
        {
            For(value.SheetTableCellId).Text = value.Value;
        }

        foreach (var value in (await db.SheetNumericValues.AsNoTracking()
                .Where(value => sheetRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId))
                .Select(value => new { value.SheetTableRowRevisionId, value.SheetTableCellId, value.Value })
                .ToListAsync(cancellationToken))
            .Where(value => shownRevisionIds.Contains(value.SheetTableRowRevisionId)))
        {
            For(value.SheetTableCellId).Number = value.Value;
        }

        foreach (var value in (await db.SheetPairValues.AsNoTracking()
                .Where(value => sheetRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId))
                .Select(value => new { value.SheetTableRowRevisionId, value.SheetTableCellId, value.PairValueX, value.PairValueY })
                .ToListAsync(cancellationToken))
            .Where(value => shownRevisionIds.Contains(value.SheetTableRowRevisionId)))
        {
            var cell = For(value.SheetTableCellId);
            cell.PairX = value.PairValueX;
            cell.PairY = value.PairValueY;
        }

        foreach (var value in (await db.SheetLinkedSheetValues.AsNoTracking()
                .Where(value => sheetRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId))
                .Select(value => new
                {
                    value.SheetTableRowRevisionId,
                    value.SheetTableCellId,
                    value.LinkedSpecSheetTypeId,
                    value.LinkedSpecSheetPhaseId,
                    value.TableReferenceId,
                    value.LinkedReferenceId
                })
                .ToListAsync(cancellationToken))
            .Where(value => shownRevisionIds.Contains(value.SheetTableRowRevisionId)))
        {
            For(value.SheetTableCellId).Linked = new LinkedSheetTarget
            {
                SheetTypeId = value.LinkedSpecSheetTypeId,
                PhaseId = value.LinkedSpecSheetPhaseId,
                TableReferenceId = value.TableReferenceId,
                ItemReferenceId = value.LinkedReferenceId
            };
        }

        foreach (var value in (await db.SheetCellConfigurationOverrides.AsNoTracking()
                .Where(value => sheetRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId))
                .Select(value => new { value.SheetTableRowRevisionId, value.SheetTableCellId, value.ConfigurationOverride })
                .ToListAsync(cancellationToken))
            .Where(value => shownRevisionIds.Contains(value.SheetTableRowRevisionId)))
        {
            For(value.SheetTableCellId).ConfigurationOverride = value.ConfigurationOverride;
        }

        return parts;
    }
}
