using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Verification;

/// <summary>
/// A migrated sheet's whole history, read from the revision tables once, so that the sheet can be shown as it stood at
/// each version (and with its drafts) without going back to the database for each. <see cref="At"/> gives the same
/// <see cref="SheetSnapshot"/> shape a copy is read into: at a moment, the published revision whose range covers it; with
/// no moment, each item's draft where it has one, otherwise its current revision. An item is on the sheet when its
/// revision doesn't remove it and nothing it is in was removed.
/// </summary>
internal sealed class TargetSheetHistory
{
    private sealed record Revision<TData>(
        int Id,
        int ItemId,
        RevisionStatus Status,
        DateTime? PublishedDateTime,
        DateTime? SupersededDateTime,
        bool IsDeleted,
        TData Data);

    private sealed record TableData(string Title, int DisplayOrder, ITableProperties? ConfigurationOverride);

    private sealed record GroupData(int DisplayOrder, Guid? LinkedGroupReferenceId);

    private sealed record NoteOf(int ItemId, SheetTableNote Note);

    // A group or row item's parent is its table or group; a table has none, and keeps the sheet's id there.
    private sealed record ItemOf(int Id, Guid ReferenceId, int ParentId, int TemplateId);

    private sealed record CellItem(int Id, Guid ReferenceId, int RowId, int TableCellTemplateId);

    private sealed record ExternalReferenceRevision(int ItemId, DateTime PublishedDateTime, DateTime? SupersededDateTime, bool IsDeleted, SnapshotExternalReference Reference);

    private List<Revision<TableData>> _tableRevisions = [];
    private List<Revision<GroupData>> _groupRevisions = [];
    private List<Revision<int>> _rowRevisions = [];
    private List<Revision<string?>> _propertyRevisions = [];
    private List<Revision<int>> _linkRevisions = [];
    private List<ExternalReferenceRevision> _externalReferenceRevisions = [];
    private Dictionary<int, ItemOf> _tables = [];
    private Dictionary<int, ItemOf> _groups = [];
    private Dictionary<int, ItemOf> _rows = [];
    private Dictionary<int, int> _propertyTypes = [];
    private Dictionary<int, SnapshotLinkedTable> _links = [];
    private List<CellItem> _cells = [];
    private Dictionary<int, Dictionary<int, CellContentParts>> _contentsByRowRevision = [];
    private Dictionary<int, List<SheetWidgetValue>> _tableWidgetsByRevision = [];
    private Dictionary<int, List<SheetWidgetValue>> _groupWidgetsByRevision = [];
    private Dictionary<int, List<NoteOf>> _notesByRevision = [];

    private TargetSheetHistory()
    {
    }

    public static async Task<TargetSheetHistory> LoadAsync(PUSpecSheetDbContext db, int sheetId, CancellationToken cancellationToken)
    {
        var sheetTables = db.SheetTables.AsNoTracking().Where(table => table.SheetId == sheetId);
        var sheetGroups = db.SheetTableGroups.AsNoTracking().Where(group => sheetTables.Any(table => table.Id == group.SheetTableId));
        var sheetRows = db.SheetTableRows.AsNoTracking().Where(row => sheetGroups.Any(group => group.Id == row.SheetTableGroupId));

        return new TargetSheetHistory
        {
            _tableRevisions = await db.SheetTableRevisions.AsNoTracking()
                .Where(revision => revision.SheetId == sheetId)
                .Select(revision => new Revision<TableData>(
                    revision.Id,
                    revision.SheetTableId,
                    revision.Status,
                    revision.PublishedDateTime,
                    revision.SupersededDateTime,
                    revision.IsDeleted,
                    new TableData(revision.Title, revision.DisplayOrder, revision.ConfigurationOverride)))
                .ToListAsync(cancellationToken),
            _groupRevisions = await db.SheetTableGroupRevisions.AsNoTracking()
                .Where(revision => revision.SheetId == sheetId)
                .Select(revision => new Revision<GroupData>(
                    revision.Id,
                    revision.SheetTableGroupId,
                    revision.Status,
                    revision.PublishedDateTime,
                    revision.SupersededDateTime,
                    revision.IsDeleted,
                    new GroupData(revision.DisplayOrder, revision.LinkedSheetTableGroupReferenceId)))
                .ToListAsync(cancellationToken),
            _rowRevisions = await db.SheetTableRowRevisions.AsNoTracking()
                .Where(revision => revision.SheetId == sheetId)
                .Select(revision => new Revision<int>(
                    revision.Id,
                    revision.SheetTableRowId,
                    revision.Status,
                    revision.PublishedDateTime,
                    revision.SupersededDateTime,
                    revision.IsDeleted,
                    revision.DisplayOrder))
                .ToListAsync(cancellationToken),
            _propertyRevisions = await db.SheetPropertyRevisions.AsNoTracking()
                .Where(revision => revision.SheetId == sheetId)
                .Select(revision => new Revision<string?>(
                    revision.Id,
                    revision.SheetPropertyId,
                    revision.Status,
                    revision.PublishedDateTime,
                    revision.SupersededDateTime,
                    revision.IsDeleted,
                    revision.PropertyValue))
                .ToListAsync(cancellationToken),
            _linkRevisions = await db.SheetLinkedTableRevisions.AsNoTracking()
                .Where(revision => revision.SheetId == sheetId)
                .Select(revision => new Revision<int>(
                    revision.Id,
                    revision.SheetLinkedTableId,
                    revision.Status,
                    revision.PublishedDateTime,
                    revision.SupersededDateTime,
                    revision.IsDeleted,
                    revision.DisplayOrder))
                .ToListAsync(cancellationToken),
            _tables = await sheetTables
                .Select(table => new ItemOf(table.Id, table.ReferenceId, table.SheetId, table.TableTemplateId))
                .ToDictionaryAsync(table => table.Id, cancellationToken),
            _groups = await sheetGroups
                .Select(group => new ItemOf(group.Id, group.ReferenceId, group.SheetTableId, group.TableGroupTemplateId))
                .ToDictionaryAsync(group => group.Id, cancellationToken),
            _rows = await sheetRows
                .Select(row => new ItemOf(row.Id, row.ReferenceId, row.SheetTableGroupId, row.TableRowTemplateId))
                .ToDictionaryAsync(row => row.Id, cancellationToken),
            _cells = await db.SheetTableCells.AsNoTracking()
                .Where(cell => sheetRows.Any(row => row.Id == cell.SheetTableRowId))
                .Select(cell => new CellItem(cell.Id, cell.ReferenceId, cell.SheetTableRowId, cell.TableCellTemplateId))
                .ToListAsync(cancellationToken),
            _propertyTypes = await db.SheetProperties.AsNoTracking()
                .Where(property => property.SheetId == sheetId)
                .ToDictionaryAsync(property => property.Id, property => property.SpecSheetTypePropertyId, cancellationToken),
            _links = await db.SheetLinkedTables.AsNoTracking()
                .Where(link => link.SheetId == sheetId)
                .Select(link => new { link.Id, link.TableReferenceId, link.LinkedByUserId, link.LinkedDateTime })
                .ToDictionaryAsync(
                    link => link.Id,
                    link => new SnapshotLinkedTable
                    {
                        TableReferenceId = link.TableReferenceId,
                        DisplayOrder = 0,
                        LinkedByUserId = link.LinkedByUserId,
                        LinkedDateTime = link.LinkedDateTime
                    },
                    cancellationToken),
            _externalReferenceRevisions = await ReadExternalReferencesAsync(db, sheetId, cancellationToken),
            _contentsByRowRevision = await ReadContentsAsync(db, sheetId, cancellationToken),
            _tableWidgetsByRevision = await ReadTableWidgetsAsync(db, sheetId, cancellationToken),
            _groupWidgetsByRevision = await ReadGroupWidgetsAsync(db, sheetId, cancellationToken),
            _notesByRevision = await ReadNotesAsync(db, sheetId, cancellationToken)
        };
    }

    /// <summary>
    /// The sheet as it stood at <paramref name="moment"/>, or with its drafts when that is null.
    /// </summary>
    public SheetSnapshot At(DateTime? moment)
    {
        var tableRevisions = Shown(_tableRevisions, moment);
        var groupRevisions = Shown(_groupRevisions, moment);
        var rowRevisions = Shown(_rowRevisions, moment);

        // What is on the sheet: not removed, and nothing it is in removed either.
        var tables = tableRevisions.Values
            .Where(table => !table.IsDeleted)
            .Select(table => new SnapshotTable
            {
                Id = table.ItemId,
                ReferenceId = _tables[table.ItemId].ReferenceId,
                TableTemplateId = _tables[table.ItemId].TemplateId,
                Title = table.Data.Title,
                DisplayOrder = table.Data.DisplayOrder,
                ConfigurationOverride = table.Data.ConfigurationOverride,
                WidgetValues = _tableWidgetsByRevision.GetValueOrDefault(table.Id) ?? [],
                Notes = (_notesByRevision.GetValueOrDefault(table.Id) ?? []).Select(note => note.Note).ToList()
            })
            .ToList();
        var tableIds = tables.Select(table => table.Id).ToHashSet();

        var groups = groupRevisions.Values
            .Where(group => !group.IsDeleted && tableIds.Contains(_groups[group.ItemId].ParentId))
            .Select(group => new SnapshotGroup
            {
                Id = group.ItemId,
                ReferenceId = _groups[group.ItemId].ReferenceId,
                TableId = _groups[group.ItemId].ParentId,
                TableGroupTemplateId = _groups[group.ItemId].TemplateId,
                DisplayOrder = group.Data.DisplayOrder,
                WidgetValues = _groupWidgetsByRevision.GetValueOrDefault(group.Id) ?? [],
                LinkedGroupReferenceId = group.Data.LinkedGroupReferenceId
            })
            .ToList();
        var groupIds = groups.Select(group => group.Id).ToHashSet();

        var shownRows = rowRevisions.Values
            .Where(row => !row.IsDeleted && groupIds.Contains(_rows[row.ItemId].ParentId))
            .ToList();
        var rows = shownRows
            .Select(row => new SnapshotRow
            {
                Id = row.ItemId,
                ReferenceId = _rows[row.ItemId].ReferenceId,
                GroupId = _rows[row.ItemId].ParentId,
                TableRowTemplateId = _rows[row.ItemId].TemplateId,
                DisplayOrder = row.Data
            })
            .ToList();

        var revisionOfRow = shownRows.ToDictionary(row => row.ItemId, row => row.Id);
        var cells = _cells
            .Where(cell => revisionOfRow.ContainsKey(cell.RowId))
            .Select(cell => new SnapshotCell
            {
                Id = cell.Id,
                ReferenceId = cell.ReferenceId,
                RowId = cell.RowId,
                TableCellTemplateId = cell.TableCellTemplateId,
                Content = _contentsByRowRevision.TryGetValue(revisionOfRow[cell.RowId], out var contents) && contents.TryGetValue(cell.Id, out var parts)
                    ? parts.Build()
                    : CellContent.Empty
            })
            .ToList();

        return new SheetSnapshot
        {
            Tables = tables,
            Groups = groups,
            Rows = rows,
            Cells = cells,
            Properties = Shown(_propertyRevisions, moment).Values
                .Where(property => !property.IsDeleted)
                .ToDictionary(property => _propertyTypes[property.ItemId], property => property.Data),
            LinkedTables = Shown(_linkRevisions, moment).Values
                .Where(link => !link.IsDeleted)
                .Select(link => _links[link.ItemId] with { DisplayOrder = link.Data })
                .ToList(),
            ExternalReferences = moment is { } at ? ExternalReferencesAt(at) : null
        };
    }

    // The revisions in view, then the one to show for each item: its draft where it has one alongside its published one.
    private static Dictionary<int, Revision<TData>> Shown<TData>(List<Revision<TData>> revisions, DateTime? moment)
    {
        var shown = new Dictionary<int, Revision<TData>>();
        foreach (var revision in revisions.Where(revision => InView(revision, moment)))
        {
            if (!shown.TryGetValue(revision.ItemId, out var held) || (revision.Status == RevisionStatus.Draft && held.Status != RevisionStatus.Draft))
            {
                shown[revision.ItemId] = revision;
            }
        }

        return shown;
    }

    private static bool InView<TData>(Revision<TData> revision, DateTime? moment)
    {
        if (moment is { } at)
        {
            return revision.Status == RevisionStatus.Published
                && revision.PublishedDateTime <= at
                && (revision.SupersededDateTime is null || revision.SupersededDateTime > at);
        }

        return revision.Status == RevisionStatus.Draft
            || (revision.Status == RevisionStatus.Published && revision.SupersededDateTime is null);
    }

    // The widgets of every table revision of the sheet, by revision.
    private static async Task<Dictionary<int, List<SheetWidgetValue>>> ReadTableWidgetsAsync(PUSpecSheetDbContext db, int sheetId, CancellationToken cancellationToken)
    {
        var rows = await db.SheetTableWidgetValues.AsNoTracking()
            .Where(value => db.SheetTableRevisions.Any(revision => revision.Id == value.SheetTableRevisionId && revision.SheetId == sheetId))
            .Select(value => new { value.SheetTableRevisionId, value.SheetTableWidget!.TableTemplateWidgetId, value.Value })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.SheetTableRevisionId)
            .ToDictionary(
                revision => revision.Key,
                revision => revision.Select(row => new SheetWidgetValue { TemplateWidgetId = row.TableTemplateWidgetId, Value = row.Value }).ToList());
    }

    private static async Task<Dictionary<int, List<SheetWidgetValue>>> ReadGroupWidgetsAsync(PUSpecSheetDbContext db, int sheetId, CancellationToken cancellationToken)
    {
        var rows = await db.SheetTableGroupWidgetValues.AsNoTracking()
            .Where(value => db.SheetTableGroupRevisions.Any(revision => revision.Id == value.SheetTableGroupRevisionId && revision.SheetId == sheetId))
            .Select(value => new { value.SheetTableGroupRevisionId, value.SheetTableGroupWidget!.TableGroupTemplateWidgetId, value.Value })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.SheetTableGroupRevisionId)
            .ToDictionary(
                revision => revision.Key,
                revision => revision.Select(row => new SheetWidgetValue { TemplateWidgetId = row.TableGroupTemplateWidgetId, Value = row.Value }).ToList());
    }

    // The notes of every table revision of the sheet, by revision, oldest first as the copies read them.
    private static async Task<Dictionary<int, List<NoteOf>>> ReadNotesAsync(PUSpecSheetDbContext db, int sheetId, CancellationToken cancellationToken)
    {
        var rows = await db.SheetTableNoteValues.AsNoTracking()
            .Where(value => db.SheetTableRevisions.Any(revision => revision.Id == value.SheetTableRevisionId && revision.SheetId == sheetId))
            .Select(value => new
            {
                value.SheetTableRevisionId,
                value.SheetTableNoteId,
                value.Note,
                value.IsObsolete,
                value.SheetTableNote!.SourceSystem,
                value.SheetTableNote.CreatedByUserId,
                value.SheetTableNote.CreatedDateTime
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.SheetTableRevisionId)
            .ToDictionary(
                revision => revision.Key,
                revision => revision
                    .OrderBy(row => row.CreatedDateTime)
                    .ThenBy(row => row.SheetTableNoteId)
                    .Select(row => new NoteOf(row.SheetTableNoteId, new SheetTableNote
                    {
                        Note = row.Note,
                        SourceSystem = row.SourceSystem,
                        IsObsolete = row.IsObsolete,
                        CreatedByUserId = row.CreatedByUserId,
                        CreatedDateTime = row.CreatedDateTime
                    }))
                    .ToList());
    }

    // External references have no drafts: as of a moment is the revision published at or before it and not yet replaced.
    private List<SnapshotExternalReference> ExternalReferencesAt(DateTime moment)
    {
        return _externalReferenceRevisions
            .Where(revision => revision.PublishedDateTime <= moment
                && (revision.SupersededDateTime is null || revision.SupersededDateTime > moment)
                && !revision.IsDeleted)
            .Select(revision => revision.Reference)
            .ToList();
    }

    private static async Task<List<ExternalReferenceRevision>> ReadExternalReferencesAsync(
        PUSpecSheetDbContext db,
        int sheetId,
        CancellationToken cancellationToken)
    {
        var referenceIds = await db.SheetExternalReferences.AsNoTracking()
            .Where(reference => reference.SheetId == sheetId)
            .ToDictionaryAsync(reference => reference.Id, reference => reference.ReferenceId, cancellationToken);

        return (await db.SheetExternalReferenceRevisions.AsNoTracking()
                .Where(revision => revision.SheetId == sheetId)
                .Select(revision => new
                {
                    revision.SheetExternalReferenceId,
                    revision.PublishedDateTime,
                    revision.SupersededDateTime,
                    revision.IsDeleted,
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
            .Select(revision => new ExternalReferenceRevision(
                revision.SheetExternalReferenceId,
                revision.PublishedDateTime,
                revision.SupersededDateTime,
                revision.IsDeleted,
                new SnapshotExternalReference
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
                }))
            .ToList();
    }

    /// <summary>
    /// The values of every row revision of the sheet, by revision and then by cell. A row revision has one set of values
    /// for each cell, so what a moment shows is the values of the revisions it shows.
    /// </summary>
    private static async Task<Dictionary<int, Dictionary<int, CellContentParts>>> ReadContentsAsync(
        PUSpecSheetDbContext db,
        int sheetId,
        CancellationToken cancellationToken)
    {
        var contents = new Dictionary<int, Dictionary<int, CellContentParts>>();
        var sheetRevisions = db.SheetTableRowRevisions.AsNoTracking().Where(revision => revision.SheetId == sheetId);

        CellContentParts For(int revisionId, int cellId)
        {
            if (!contents.TryGetValue(revisionId, out var cells))
            {
                cells = [];
                contents[revisionId] = cells;
            }

            if (!cells.TryGetValue(cellId, out var parts))
            {
                parts = new CellContentParts();
                cells[cellId] = parts;
            }

            return parts;
        }

        foreach (var value in await db.SheetTextValues.AsNoTracking()
            .Where(value => sheetRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId))
            .Select(value => new { value.SheetTableRowRevisionId, value.SheetTableCellId, value.Value })
            .ToListAsync(cancellationToken))
        {
            For(value.SheetTableRowRevisionId, value.SheetTableCellId).Text = value.Value;
        }

        foreach (var value in await db.SheetNumericValues.AsNoTracking()
            .Where(value => sheetRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId))
            .Select(value => new { value.SheetTableRowRevisionId, value.SheetTableCellId, value.Value })
            .ToListAsync(cancellationToken))
        {
            For(value.SheetTableRowRevisionId, value.SheetTableCellId).Number = value.Value;
        }

        foreach (var value in await db.SheetPairValues.AsNoTracking()
            .Where(value => sheetRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId))
            .Select(value => new { value.SheetTableRowRevisionId, value.SheetTableCellId, value.PairValueX, value.PairValueY })
            .ToListAsync(cancellationToken))
        {
            var parts = For(value.SheetTableRowRevisionId, value.SheetTableCellId);
            parts.PairX = value.PairValueX;
            parts.PairY = value.PairValueY;
        }

        foreach (var value in await db.SheetLinkedSheetValues.AsNoTracking()
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
        {
            For(value.SheetTableRowRevisionId, value.SheetTableCellId).Linked = new LinkedSheetTarget
            {
                SheetTypeId = value.LinkedSpecSheetTypeId,
                PhaseId = value.LinkedSpecSheetPhaseId,
                TableReferenceId = value.TableReferenceId,
                ItemReferenceId = value.LinkedReferenceId
            };
        }

        foreach (var value in await db.SheetCellConfigurationOverrides.AsNoTracking()
            .Where(value => sheetRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId))
            .Select(value => new { value.SheetTableRowRevisionId, value.SheetTableCellId, value.ConfigurationOverride })
            .ToListAsync(cancellationToken))
        {
            For(value.SheetTableRowRevisionId, value.SheetTableCellId).ConfigurationOverride = value.ConfigurationOverride;
        }

        return contents;
    }
}
