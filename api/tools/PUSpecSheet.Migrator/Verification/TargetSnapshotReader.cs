using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.Database;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Verification;

/// <summary>
/// Reads a migrated sheet back from the revision tables as it stood at a moment, into the same
/// <see cref="SheetSnapshot"/> shape a legacy copy is read into. It picks revisions with the app's own
/// <see cref="SheetRevisionQueryExtensions.InView"/>, so it sees what the app would show: at a moment, the
/// published revision whose range covers it; with no moment, each item's draft where it has one, otherwise its
/// current revision. An item is on the sheet when its revision doesn't remove it and nothing it is in was removed.
/// </summary>
internal static class TargetSnapshotReader
{
    public static async Task<SheetSnapshot> ReadAsync(PuSpecSheetDbContext db, int sheetId, DateTime? moment, CancellationToken cancellationToken)
    {
        // The revisions are read on their own and joined to their items here: the live view is a union of the
        // current and draft revisions, and navigating from a union is more than EF will translate.
        var tableRevisions = Latest(await db.SheetTableRevisions.AsNoTracking()
            .Where(revision => revision.SheetId == sheetId)
            .InView(moment)
            .Select(revision => new { revision.Id, ItemId = revision.SheetTableId, revision.Status, revision.IsDeleted, revision.DisplayOrder, revision.Title, revision.WidgetValues, revision.Notes })
            .ToListAsync(cancellationToken), revision => revision.ItemId, revision => revision.Status);
        var tableItems = await db.SheetTables.AsNoTracking()
            .Where(table => table.SheetId == sheetId)
            .Select(table => new { table.Id, table.PublicId, table.TableTemplateVersionId })
            .ToDictionaryAsync(table => table.Id, cancellationToken);

        var blockRevisions = Latest(await db.SheetColumnBlockRevisions.AsNoTracking()
            .Where(revision => revision.SheetId == sheetId)
            .InView(moment)
            .Select(revision => new { revision.Id, ItemId = revision.SheetColumnBlockId, revision.Status, revision.IsDeleted, revision.DisplayOrder })
            .ToListAsync(cancellationToken), revision => revision.ItemId, revision => revision.Status);
        var blockItems = await db.SheetColumnBlocks.AsNoTracking()
            .Where(block => block.SheetTable.SheetId == sheetId)
            .Select(block => new { block.Id, block.PublicId, block.SheetTableId, block.TemplateColumnBlockId })
            .ToDictionaryAsync(block => block.Id, cancellationToken);

        var sectionRevisions = Latest(await db.SheetSectionRevisions.AsNoTracking()
            .Where(revision => revision.SheetId == sheetId)
            .InView(moment)
            .Select(revision => new { revision.Id, ItemId = revision.SheetSectionId, revision.Status, revision.IsDeleted, revision.DisplayOrder, revision.WidgetValues, revision.LinkedSectionPublicId })
            .ToListAsync(cancellationToken), revision => revision.ItemId, revision => revision.Status);
        var sectionItems = await db.SheetSections.AsNoTracking()
            .Where(section => section.SheetTable.SheetId == sheetId)
            .Select(section => new { section.Id, section.PublicId, section.SheetTableId, section.ParentSheetSectionId, section.TemplateSectionId })
            .ToDictionaryAsync(section => section.Id, cancellationToken);

        var rowRevisions = Latest(await db.SheetRowRevisions.AsNoTracking()
            .Where(revision => revision.SheetId == sheetId)
            .InView(moment)
            .Select(revision => new { revision.Id, ItemId = revision.SheetRowId, revision.Status, revision.IsDeleted, revision.DisplayOrder })
            .ToListAsync(cancellationToken), revision => revision.ItemId, revision => revision.Status);
        var rowItems = await db.SheetRows.AsNoTracking()
            .Where(row => row.SheetSection.SheetTable.SheetId == sheetId)
            .Select(row => new { row.Id, row.PublicId, row.SheetSectionId, row.TemplateRowId })
            .ToDictionaryAsync(row => row.Id, cancellationToken);

        // What is on the sheet: not removed, and nothing it is in removed either.
        var tables = tableRevisions.Values
            .Where(table => !table.IsDeleted)
            .Select(table => new SnapshotTable
            {
                Id = table.ItemId,
                PublicId = tableItems[table.ItemId].PublicId,
                TableTemplateVersionId = tableItems[table.ItemId].TableTemplateVersionId,
                Title = table.Title,
                DisplayOrder = table.DisplayOrder,
                WidgetValues = table.WidgetValues,
                Notes = table.Notes,
            })
            .ToList();
        var tableIds = tables.Select(table => table.Id).ToHashSet();

        var blocks = blockRevisions.Values
            .Where(block => !block.IsDeleted && tableIds.Contains(blockItems[block.ItemId].SheetTableId))
            .Select(block => new SnapshotColumnBlock
            {
                Id = block.ItemId,
                PublicId = blockItems[block.ItemId].PublicId,
                TableId = blockItems[block.ItemId].SheetTableId,
                TemplateColumnBlockId = blockItems[block.ItemId].TemplateColumnBlockId,
                DisplayOrder = block.DisplayOrder,
            })
            .ToList();
        var blockIds = blocks.Select(block => block.Id).ToHashSet();

        var shown = new Dictionary<int, bool>();
        bool IsShown(int sectionId)
        {
            if (!shown.TryGetValue(sectionId, out var result))
            {
                var item = sectionItems[sectionId];
                result = sectionRevisions.TryGetValue(sectionId, out var section)
                    && !section.IsDeleted
                    && tableIds.Contains(item.SheetTableId)
                    && (item.ParentSheetSectionId is not { } parentId || IsShown(parentId));
                shown[sectionId] = result;
            }

            return result;
        }

        var sections = sectionRevisions.Values
            .Where(section => IsShown(section.ItemId))
            .Select(section => new SnapshotSection
            {
                Id = section.ItemId,
                PublicId = sectionItems[section.ItemId].PublicId,
                TableId = sectionItems[section.ItemId].SheetTableId,
                ParentSectionId = sectionItems[section.ItemId].ParentSheetSectionId,
                TemplateSectionId = sectionItems[section.ItemId].TemplateSectionId,
                DisplayOrder = section.DisplayOrder,
                WidgetValues = section.WidgetValues,
                LinkedSectionPublicId = section.LinkedSectionPublicId,
            })
            .ToList();
        var sectionIds = sections.Select(section => section.Id).ToHashSet();

        var visibleRows = rowRevisions.Values
            .Where(row => !row.IsDeleted && sectionIds.Contains(rowItems[row.ItemId].SheetSectionId))
            .ToList();
        var rows = visibleRows
            .Select(row => new SnapshotRow
            {
                Id = row.ItemId,
                PublicId = rowItems[row.ItemId].PublicId,
                SectionId = rowItems[row.ItemId].SheetSectionId,
                TemplateRowId = rowItems[row.ItemId].TemplateRowId,
                DisplayOrder = row.DisplayOrder,
            })
            .ToList();

        var rowIds = visibleRows.Select(row => row.ItemId).ToList();
        var revisionIds = visibleRows.Select(row => row.Id).ToList();
        var contents = await ReadContentsAsync(db, revisionIds, cancellationToken);

        var cells = (await db.SheetCells.AsNoTracking()
            .Where(cell => rowIds.Contains(cell.SheetRowId))
            .Select(cell => new { cell.Id, cell.PublicId, cell.SheetRowId, cell.TemplateCellId, cell.SheetColumnBlockId })
            .ToListAsync(cancellationToken))
            .Where(cell => cell.SheetColumnBlockId is not { } blockId || blockIds.Contains(blockId))
            .Select(cell => new SnapshotCell
            {
                Id = cell.Id,
                PublicId = cell.PublicId,
                RowId = cell.SheetRowId,
                TemplateCellId = cell.TemplateCellId,
                ColumnBlockId = cell.SheetColumnBlockId,
                Content = contents.TryGetValue(cell.Id, out var content) ? content.Build() : CellContent.Empty,
            })
            .ToList();

        var properties = Latest(await db.SheetPropertyRevisions.AsNoTracking()
            .Where(revision => revision.SheetId == sheetId)
            .InView(moment)
            .Select(revision => new { ItemId = revision.SheetPropertyId, revision.SheetProperty.SheetTypePropertyId, revision.Status, revision.IsDeleted, revision.Value })
            .ToListAsync(cancellationToken), revision => revision.ItemId, revision => revision.Status);

        var linkedTables = Latest(await db.SheetLinkedTableRevisions.AsNoTracking()
            .Where(revision => revision.SheetId == sheetId)
            .InView(moment)
            .Select(revision => new
            {
                ItemId = revision.SheetLinkedTableId,
                revision.Status,
                revision.IsDeleted,
                revision.DisplayOrder,
                revision.SheetLinkedTable.LinkedTablePublicId,
                revision.SheetLinkedTable.LinkedByUserId,
                revision.SheetLinkedTable.LinkedAtUtc,
            })
            .ToListAsync(cancellationToken), revision => revision.ItemId, revision => revision.Status);

        return new SheetSnapshot
        {
            Tables = tables,
            Sections = sections,
            ColumnBlocks = blocks,
            Rows = rows,
            Cells = cells,
            Properties = properties.Values
                .Where(property => !property.IsDeleted)
                .ToDictionary(property => property.SheetTypePropertyId, property => property.Value),
            LinkedTables = linkedTables.Values
                .Where(link => !link.IsDeleted)
                .Select(link => new SnapshotLinkedTable
                {
                    LinkedTablePublicId = link.LinkedTablePublicId,
                    DisplayOrder = link.DisplayOrder,
                    LinkedByUserId = link.LinkedByUserId,
                    LinkedAtUtc = link.LinkedAtUtc,
                })
                .ToList(),
            ExternalReferences = moment is { } at ? await ReadExternalReferencesAsync(db, sheetId, at, cancellationToken) : null,
        };
    }

    // External references have no drafts: as of a moment is the revision published at or before it and not yet replaced.
    private static async Task<List<SnapshotExternalReference>> ReadExternalReferencesAsync(
        PuSpecSheetDbContext db,
        int sheetId,
        DateTime moment,
        CancellationToken cancellationToken)
    {
        return await db.SheetExternalReferenceRevisions.AsNoTracking()
            .Where(revision => revision.SheetId == sheetId
                && revision.PublishedAtUtc <= moment
                && (revision.SupersededAtUtc == null || revision.SupersededAtUtc > moment)
                && !revision.IsDeleted)
            .Select(revision => new SnapshotExternalReference
            {
                ReferenceId = revision.SheetExternalReference.ReferenceId,
                ExternalId = revision.ExternalId,
                SystemName = revision.SystemName,
                OwnerName = revision.OwnerName,
                Url = revision.Url,
                Type = revision.Type,
                Grouping = revision.Grouping,
                SystemIconUrl = revision.SystemIconUrl,
                Description = revision.Description,
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>The values of the given row revisions, by cell. A cell has one revision in view, so one set of values.</summary>
    private static async Task<Dictionary<int, CellContentParts>> ReadContentsAsync(
        PuSpecSheetDbContext db,
        List<int> revisionIds,
        CancellationToken cancellationToken)
    {
        var parts = new Dictionary<int, CellContentParts>();
        CellContentParts For(int cellId)
        {
            if (!parts.TryGetValue(cellId, out var cell))
            {
                cell = new CellContentParts();
                parts[cellId] = cell;
            }

            return cell;
        }

        foreach (var value in await db.SheetRowRevisions.AsNoTracking()
            .Where(revision => revisionIds.Contains(revision.Id))
            .SelectMany(revision => revision.TextValues)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken))
        {
            For(value.SheetCellId).Text = value.Value;
        }

        foreach (var value in await db.SheetRowRevisions.AsNoTracking()
            .Where(revision => revisionIds.Contains(revision.Id))
            .SelectMany(revision => revision.NumericValues)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken))
        {
            For(value.SheetCellId).Number = value.Value;
        }

        foreach (var value in await db.SheetRowRevisions.AsNoTracking()
            .Where(revision => revisionIds.Contains(revision.Id))
            .SelectMany(revision => revision.DateValues)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken))
        {
            For(value.SheetCellId).Date = value.Value;
        }

        foreach (var value in await db.SheetRowRevisions.AsNoTracking()
            .Where(revision => revisionIds.Contains(revision.Id))
            .SelectMany(revision => revision.BooleanValues)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken))
        {
            For(value.SheetCellId).Boolean = value.Value;
        }

        foreach (var value in await db.SheetRowRevisions.AsNoTracking()
            .Where(revision => revisionIds.Contains(revision.Id))
            .SelectMany(revision => revision.OptionValues)
            .Select(value => new { value.SheetCellId, value.CellTypeOptionId })
            .ToListAsync(cancellationToken))
        {
            For(value.SheetCellId).OptionId = value.CellTypeOptionId;
        }

        foreach (var value in await db.SheetRowRevisions.AsNoTracking()
            .Where(revision => revisionIds.Contains(revision.Id))
            .SelectMany(revision => revision.LabelledTextValues)
            .Select(value => new { value.SheetCellId, value.SlotKey, value.Position, value.Value })
            .ToListAsync(cancellationToken))
        {
            For(value.SheetCellId).LabelledText.Add(new LabelledSlot
            {
                SlotKey = value.SlotKey,
                Position = value.Position,
                Value = value.Value,
            });
        }

        foreach (var value in await db.SheetRowRevisions.AsNoTracking()
            .Where(revision => revisionIds.Contains(revision.Id))
            .SelectMany(revision => revision.LinkedSheetValues)
            .Select(value => new { value.SheetCellId, value.LinkedSheetTypeId, value.LinkedPhaseId, value.LinkedTablePublicId, value.LinkedItemPublicId })
            .ToListAsync(cancellationToken))
        {
            For(value.SheetCellId).Linked = new LinkedSheetTarget
            {
                SheetTypeId = value.LinkedSheetTypeId,
                PhaseId = value.LinkedPhaseId,
                TablePublicId = value.LinkedTablePublicId,
                ItemPublicId = value.LinkedItemPublicId,
            };
        }

        foreach (var value in await db.SheetRowRevisions.AsNoTracking()
            .Where(revision => revisionIds.Contains(revision.Id))
            .SelectMany(revision => revision.CellSettings)
            .Select(value => new { value.SheetCellId, value.Settings })
            .ToListAsync(cancellationToken))
        {
            For(value.SheetCellId).Settings = value.Settings;
        }

        return parts;
    }

    /// <summary>The revision in view for each item: its draft where it has one alongside its published revision.</summary>
    private static Dictionary<int, T> Latest<T>(List<T> revisions, Func<T, int> itemId, Func<T, RevisionStatus> status)
    {
        return revisions
            .GroupBy(itemId)
            .ToDictionary(group => group.Key, group => group.OrderBy(revision => status(revision) == RevisionStatus.Draft ? 0 : 1).First());
    }
}
