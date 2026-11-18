using System.Globalization;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Migrator.Legacy;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// Turns the copies of one sheet into its history on the revision tables. Each published copy, oldest first, becomes a
/// version, and each item gets a published revision only when it is new or differs from its last one; the revision
/// replaces the one before at the version's moment. An item missing from a copy gets a removing revision, unless what
/// it was in went too. The working copy, if there is one, becomes drafts by its editor. Workflow steps become
/// submissions.
/// </summary>
internal sealed class SheetHistoryBuilder
{
    private const string Removed = "removed";

    private readonly DbSheet _sheet;
    private readonly DateTime _now;
    private readonly List<string> _warnings = [];
    private readonly KeyAssigner _assigner;
    private readonly List<BuiltVersion> _checks = [];
    private readonly SubmissionPlanner _submissions;
    private readonly RevisionFactory _revisions;
    private readonly List<IDbSheetRevision> _writtenForCopy = [];
    private readonly HashSet<Guid> _claimedReferenceIds = [];
    private readonly Dictionary<int, ItemTrack<DbSheetTable, DbSheetTableRevision>> _tables = [];
    private readonly Dictionary<int, ItemTrack<DbSheetTableGroup, DbSheetTableGroupRevision>> _groups = [];
    private readonly Dictionary<int, ItemTrack<DbSheetTableRow, DbSheetTableRowRevision>> _rows = [];
    private readonly Dictionary<int, ItemTrack<DbSheetProperty, DbSheetPropertyRevision>> _properties = [];
    private readonly Dictionary<Guid, ItemTrack<DbSheetLinkedTable, DbSheetLinkedTableRevision>> _linkedTables = [];
    private readonly Dictionary<Guid, ExternalReferenceTrack> _references = [];
    private readonly Dictionary<CellKey, DbSheetTableCell> _cells = [];
    private readonly Dictionary<(int TableKey, int TemplateWidgetId), DbSheetTableWidget> _tableWidgets = [];
    private readonly Dictionary<(int GroupKey, int TemplateWidgetId), DbSheetTableGroupWidget> _groupWidgets = [];
    private readonly Dictionary<NoteKey, DbSheetTableNote> _notes = [];
    private SnapshotKeys _previousKeys = new();
    private HashSet<int> _previousProperties = [];
    private HashSet<Guid> _previousLinkedTables = [];
    private HashSet<Guid> _previousReferences = [];

    private SheetHistoryBuilder(DbSheet sheet, DateTime now)
    {
        _sheet = sheet;
        _now = now;
        _assigner = new KeyAssigner(_warnings);
        _submissions = new SubmissionPlanner(sheet, _warnings);
        _revisions = new RevisionFactory(sheet);
    }

    /// <param name="sheet">The sheet, already saved so that it has its id.</param>
    /// <param name="copies">Every copy of the sheet.</param>
    /// <param name="now">When the working copy's drafts are written.</param>
    public static BuiltSheet Build(DbSheet sheet, IReadOnlyList<LegacyCopy> copies, DateTime now)
    {
        var builder = new SheetHistoryBuilder(sheet, now);
        builder.BuildAll(copies);
        return new BuiltSheet
        {
            Sheet = sheet,
            Checks = builder._checks,
            Submissions = builder._submissions.Planned,
            Warnings = builder._warnings
        };
    }

    private void BuildAll(IReadOnlyList<LegacyCopy> copies)
    {
        var published = copies.Where(copy => !copy.IsWorkingCopy).OrderBy(copy => copy.PublishedDateTime).ThenBy(copy => copy.Id).ToList();
        var working = copies.Where(copy => copy.IsWorkingCopy).ToList();
        if (working.Count > 1)
        {
            throw new MigrationException($"It has {working.Count} unpublished copies; a sheet can only have one working copy.");
        }

        DateTime? last = null;
        var number = 0;
        foreach (var copy in published)
        {
            number++;
            var at = copy.PublishedDateTime!.Value;
            if (last is { } previous && at <= previous)
            {
                // Two versions at one moment can't be told apart "as of" a time, so the later moves to just after.
                at = previous.AddTicks(1);
                _warnings.Add(string.Create(
                    CultureInfo.InvariantCulture,
                    $"Version {number} (copy {copy.Id}) was published at {copy.PublishedDateTime:O}, the same moment as the version before it; it is moved to {at:O}."));
            }

            var version = new DbSheetVersion
            {
                Id = 0,
                SheetId = _sheet.Id,
                VersionNumber = number,
                PublishedDateTime = at,
                PublishedByUserId = copy.UserId,
                ChangeLog = copy.ChangeLog,
                SourceSpecSheetId = copy.Id,
                LegacySpecSheetId = copy.LegacySpecSheetId,
                HasBeenMigrated = copy.HasBeenMigrated,
                SourceCreatedByUserId = copy.CreatedByUserId,
                SourceCreatedDateTime = copy.CreatedDateTime
            };
            _sheet.Versions.Add(version);

            Apply(copy, version, at);
            _submissions.Plan(copy, version, at, _writtenForCopy);
            _checks.Add(new BuiltVersion { VersionNumber = number, Moment = at, Expected = copy.Snapshot });
            last = at;
        }

        if (working.Count == 1)
        {
            var copy = working[0];
            if (last is { } lastPublished && copy.CreatedDateTime < lastPublished)
            {
                _warnings.Add($"The working copy (copy {copy.Id}) was started before the last version was published; it is kept as drafts on top of that version.");
            }

            Apply(copy, null, _now);
            _submissions.Plan(copy, null, _now, _writtenForCopy);
            _checks.Add(new BuiltVersion { VersionNumber = number + 1, Moment = null, Expected = copy.Snapshot });
        }
    }

    private void Apply(LegacyCopy copy, DbSheetVersion? version, DateTime at)
    {
        _writtenForCopy.Clear();
        var snapshot = copy.Snapshot;
        var keys = _assigner.Assign(snapshot);
        var context = new WriteContext { Version = version, At = at, UserId = copy.UserId };

        // Orders are kept as the copy has them, so an item whose order the copy changed gets a revision for it.
        foreach (var table in snapshot.Tables)
        {
            ApplyTable(context, keys, table);
        }

        foreach (var group in snapshot.Groups)
        {
            ApplyGroup(context, keys, group);
        }

        foreach (var row in snapshot.Rows)
        {
            ApplyRow(context, snapshot, keys, row);
        }

        RemoveMissing(context, keys);
        ApplyProperties(context, snapshot);
        ApplyLinkedTables(context, snapshot);
        if (version is not null)
        {
            ApplyExternalReferences(version, at, snapshot.ExternalReferences ?? []);
            _previousKeys = keys;
        }
    }

    private void ApplyTable(WriteContext context, SnapshotKeys keys, SnapshotTable table)
    {
        var key = keys.Tables[table.Id];
        if (!_tables.TryGetValue(key, out var track))
        {
            var item = new DbSheetTable
            {
                Id = 0,
                SheetId = _sheet.Id,
                TableTemplateId = table.TableTemplateId,
                ReferenceId = Claim(table.ReferenceId, "table"),
                CreatedDateTime = context.At
            };
            _sheet.Tables.Add(item);
            track = new ItemTrack<DbSheetTable, DbSheetTableRevision>(item, null, null);
            _tables[key] = track;
        }

        var state = $"{table.DisplayOrder}|{table.Title}|{SnapshotJson.Of(table.ConfigurationOverride)}|{SnapshotJson.Of(table.WidgetValues)}|{SnapshotJson.Of(table.Notes)}";
        Write(context, track, state, revisionNumber => _revisions.TableRevision(
            context,
            revisionNumber,
            table.Title,
            table.DisplayOrder,
            table.ConfigurationOverride,
            TableWidgetRows(context, key, table.WidgetValues),
            NoteRows(context, key, table.Notes),
            isDeleted: false));
    }

    private void ApplyGroup(WriteContext context, SnapshotKeys keys, SnapshotGroup group)
    {
        var key = keys.Groups[group.Id];
        var tableKey = keys.Tables[group.TableId];
        if (!_groups.TryGetValue(key, out var track))
        {
            var item = new DbSheetTableGroup
            {
                Id = 0,
                SheetTableId = 0,
                TableGroupTemplateId = group.TableGroupTemplateId,
                ReferenceId = Claim(group.ReferenceId, "group"),
                CreatedDateTime = context.At
            };
            _tables[tableKey].Item.Groups.Add(item);
            track = new ItemTrack<DbSheetTableGroup, DbSheetTableGroupRevision>(item, tableKey, null);
            _groups[key] = track;
        }

        var state = $"{group.DisplayOrder}|{SnapshotJson.Of(group.WidgetValues)}|{group.LinkedGroupReferenceId}";
        Write(context, track, state, revisionNumber => _revisions.GroupRevision(
            context,
            revisionNumber,
            group.DisplayOrder,
            group.LinkedGroupReferenceId,
            GroupWidgetRows(context, key, group.WidgetValues),
            isDeleted: false));
    }

    // The rows that give a table revision its widgets: each is a new row for the widget's item, which the table has
    // from the first copy that has the widget.
    private List<DbSheetTableWidgetValue> TableWidgetRows(WriteContext context, int tableKey, IReadOnlyList<SheetWidgetValue> widgets)
    {
        var rows = new List<DbSheetTableWidgetValue>();
        foreach (var widget in widgets)
        {
            var itemKey = (tableKey, widget.TemplateWidgetId);
            if (!_tableWidgets.TryGetValue(itemKey, out var item))
            {
                item = new DbSheetTableWidget
                {
                    Id = 0,
                    SheetTableId = 0,
                    TableTemplateWidgetId = widget.TemplateWidgetId,
                    CreatedDateTime = context.At
                };
                _tables[tableKey].Item.Widgets.Add(item);
                _tableWidgets[itemKey] = item;
            }

            rows.Add(new DbSheetTableWidgetValue
            {
                SheetTableRevisionId = 0,
                SheetTableWidgetId = 0,
                Value = widget.Value,
                SheetTableWidget = item
            });
        }

        return rows;
    }

    private List<DbSheetTableGroupWidgetValue> GroupWidgetRows(WriteContext context, int groupKey, IReadOnlyList<SheetWidgetValue> widgets)
    {
        var rows = new List<DbSheetTableGroupWidgetValue>();
        foreach (var widget in widgets)
        {
            var itemKey = (groupKey, widget.TemplateWidgetId);
            if (!_groupWidgets.TryGetValue(itemKey, out var item))
            {
                item = new DbSheetTableGroupWidget
                {
                    Id = 0,
                    SheetTableGroupId = 0,
                    TableGroupTemplateWidgetId = widget.TemplateWidgetId,
                    CreatedDateTime = context.At
                };
                _groups[groupKey].Item.Widgets.Add(item);
                _groupWidgets[itemKey] = item;
            }

            rows.Add(new DbSheetTableGroupWidgetValue
            {
                SheetTableGroupRevisionId = 0,
                SheetTableGroupWidgetId = 0,
                Value = widget.Value,
                SheetTableGroupWidget = item
            });
        }

        return rows;
    }

    // A note is its item from when it was written; the copies repeat it, with whether it is obsolete as it was then.
    private List<DbSheetTableNoteValue> NoteRows(WriteContext context, int tableKey, IReadOnlyList<SheetTableNote> notes)
    {
        var rows = new List<DbSheetTableNoteValue>();
        var seen = new Dictionary<(int UserId, DateTime CreatedDateTime, string SourceSystem), int>();
        foreach (var note in notes)
        {
            var identity = (note.CreatedByUserId, note.CreatedDateTime, note.SourceSystem);
            var ordinal = seen.GetValueOrDefault(identity);
            seen[identity] = ordinal + 1;

            var itemKey = new NoteKey
            {
                TableKey = tableKey,
                CreatedByUserId = note.CreatedByUserId,
                CreatedDateTime = note.CreatedDateTime,
                SourceSystem = note.SourceSystem,
                Ordinal = ordinal
            };
            if (!_notes.TryGetValue(itemKey, out var item))
            {
                item = new DbSheetTableNote
                {
                    Id = 0,
                    SheetTableId = 0,
                    SourceSystem = note.SourceSystem,
                    CreatedByUserId = note.CreatedByUserId,
                    CreatedDateTime = note.CreatedDateTime
                };
                _tables[tableKey].Item.Notes.Add(item);
                _notes[itemKey] = item;
            }

            rows.Add(new DbSheetTableNoteValue
            {
                SheetTableRevisionId = 0,
                SheetTableNoteId = 0,
                Note = note.Note,
                IsObsolete = note.IsObsolete,
                SheetTableNote = item
            });
        }

        return rows;
    }

    private void ApplyRow(WriteContext context, SheetSnapshot snapshot, SnapshotKeys keys, SnapshotRow row)
    {
        var key = keys.Rows[row.Id];
        var groupKey = keys.Groups[row.GroupId];
        if (!_rows.TryGetValue(key, out var track))
        {
            var item = new DbSheetTableRow
            {
                Id = 0,
                SheetTableGroupId = 0,
                TableRowTemplateId = row.TableRowTemplateId,
                ReferenceId = Claim(row.ReferenceId, "row"),
                CreatedDateTime = context.At
            };
            _groups[groupKey].Item.Rows.Add(item);
            track = new ItemTrack<DbSheetTableRow, DbSheetTableRowRevision>(item, null, groupKey);
            _rows[key] = track;
        }

        var contents = new List<(DbSheetTableCell Cell, CellContent Content)>();
        foreach (var cell in snapshot.CellsOf(row.Id))
        {
            var sheetCell = CellFor(track.Item, key, cell, context.At);
            if (!cell.Content.IsEmpty)
            {
                contents.Add((sheetCell, cell.Content));
            }
        }

        var state = $"{row.DisplayOrder}|{RowSignature(contents)}";
        var written = Write(context, track, state, revisionNumber =>
        {
            var revision = _revisions.RowRevision(context, revisionNumber, row.DisplayOrder, isDeleted: false);
            RevisionFactory.AddValues(revision, contents);
            return revision;
        });

        if (written && context.Version is not null)
        {
            track.PublishedCells = contents;
        }
    }

    // What a row's cells hold, from the cells that hold something, in an order that doesn't depend on the copy's.
    private static string RowSignature(List<(DbSheetTableCell Cell, CellContent Content)> contents)
    {
        var cells = contents
            .Select(content => $"{content.Cell.TableCellTemplateId}={content.Content.Signature}")
            .Order(StringComparer.Ordinal);
        return string.Join('\n', cells);
    }

    private DbSheetTableCell CellFor(DbSheetTableRow row, int rowKey, SnapshotCell cell, DateTime at)
    {
        var cellKey = new CellKey { RowKey = rowKey, TableCellTemplateId = cell.TableCellTemplateId };
        if (!_cells.TryGetValue(cellKey, out var sheetCell))
        {
            sheetCell = new DbSheetTableCell
            {
                Id = 0,
                SheetTableRowId = 0,
                TableCellTemplateId = cell.TableCellTemplateId,
                ReferenceId = Claim(cell.ReferenceId, "cell"),
                CreatedDateTime = at
            };
            row.Cells.Add(sheetCell);
            _cells[cellKey] = sheetCell;
        }

        return sheetCell;
    }

    private void RemoveMissing(WriteContext context, SnapshotKeys keys)
    {
        var tablesNow = keys.Tables.Values.ToHashSet();
        var groupsNow = keys.Groups.Values.ToHashSet();
        var rowsNow = keys.Rows.Values.ToHashSet();

        foreach (var key in _previousKeys.Tables.Values.Where(key => !tablesNow.Contains(key)))
        {
            var track = _tables[key];
            var last = track.LastPublished!;
            Write(context, track, Removed, revisionNumber => _revisions.TableRevision(
                context,
                revisionNumber,
                last.Title,
                last.DisplayOrder,
                last.ConfigurationOverride,
                last.WidgetValues.Select(value => value with { SheetTableRevisionId = 0 }),
                last.NoteValues.Select(value => value with { SheetTableRevisionId = 0 }),
                isDeleted: true));
        }

        foreach (var key in _previousKeys.Groups.Values.Where(key => !groupsNow.Contains(key)))
        {
            var track = _groups[key];
            if (tablesNow.Contains(track.TableKey!.Value))
            {
                var last = track.LastPublished!;
                Write(context, track, Removed, revisionNumber => _revisions.GroupRevision(
                    context,
                    revisionNumber,
                    last.DisplayOrder,
                    last.LinkedSheetTableGroupReferenceId,
                    last.WidgetValues.Select(value => value with { SheetTableGroupRevisionId = 0 }),
                    isDeleted: true));
            }
        }

        foreach (var key in _previousKeys.Rows.Values.Where(key => !rowsNow.Contains(key)))
        {
            var track = _rows[key];
            if (groupsNow.Contains(track.GroupKey!.Value))
            {
                // A removed row keeps its last values, so what was removed can still be seen.
                var last = track.LastPublished!;
                var kept = track.PublishedCells;
                Write(context, track, Removed, revisionNumber =>
                {
                    var revision = _revisions.RowRevision(context, revisionNumber, last.DisplayOrder, isDeleted: true);
                    RevisionFactory.AddValues(revision, kept);
                    return revision;
                });
            }
        }
    }

    private void ApplyProperties(WriteContext context, SheetSnapshot snapshot)
    {
        foreach (var (propertyId, value) in snapshot.Properties)
        {
            if (!_properties.TryGetValue(propertyId, out var track))
            {
                var item = new DbSheetProperty
                {
                    Id = 0,
                    SheetId = _sheet.Id,
                    SpecSheetTypePropertyId = propertyId,
                    CreatedDateTime = context.At
                };
                _sheet.Properties.Add(item);
                track = new ItemTrack<DbSheetProperty, DbSheetPropertyRevision>(item, null, null);
                _properties[propertyId] = track;
            }

            Write(context, track, $"={value}", revisionNumber => _revisions.PropertyRevision(context, revisionNumber, value, isDeleted: false));
        }

        foreach (var propertyId in _previousProperties.Where(id => !snapshot.Properties.ContainsKey(id)))
        {
            var track = _properties[propertyId];
            var last = track.LastPublished!;
            Write(context, track, Removed, revisionNumber => _revisions.PropertyRevision(context, revisionNumber, last.PropertyValue, isDeleted: true));
        }

        if (context.Version is not null)
        {
            _previousProperties = snapshot.Properties.Keys.ToHashSet();
        }
    }

    private void ApplyLinkedTables(WriteContext context, SheetSnapshot snapshot)
    {
        foreach (var link in snapshot.LinkedTables)
        {
            if (!_linkedTables.TryGetValue(link.TableReferenceId, out var track))
            {
                var item = new DbSheetLinkedTable
                {
                    Id = 0,
                    SheetId = _sheet.Id,
                    TableReferenceId = link.TableReferenceId,
                    LinkedByUserId = link.LinkedByUserId,
                    LinkedDateTime = link.LinkedDateTime,
                    CreatedDateTime = context.At
                };
                _sheet.LinkedTables.Add(item);
                track = new ItemTrack<DbSheetLinkedTable, DbSheetLinkedTableRevision>(item, null, null);
                _linkedTables[link.TableReferenceId] = track;
            }

            Write(context, track, $"{link.DisplayOrder}", revisionNumber => _revisions.LinkedTableRevision(context, revisionNumber, link.DisplayOrder, isDeleted: false));
        }

        var now = snapshot.LinkedTables.Select(link => link.TableReferenceId).ToHashSet();
        foreach (var tableReferenceId in _previousLinkedTables.Where(id => !now.Contains(id)))
        {
            var track = _linkedTables[tableReferenceId];
            var last = track.LastPublished!;
            Write(context, track, Removed, revisionNumber => _revisions.LinkedTableRevision(context, revisionNumber, last.DisplayOrder, isDeleted: true));
        }

        if (context.Version is not null)
        {
            _previousLinkedTables = now;
        }
    }

    // External references are pushed for a version, not edited, so they have no drafts or authors: each version gets a
    // revision for a reference that is new, changed or gone.
    private void ApplyExternalReferences(DbSheetVersion version, DateTime at, IReadOnlyList<SnapshotExternalReference> references)
    {
        foreach (var reference in references)
        {
            if (!_references.TryGetValue(reference.ReferenceId, out var track))
            {
                var item = new DbSheetExternalReference
                {
                    Id = 0,
                    SheetId = _sheet.Id,
                    ReferenceId = reference.ReferenceId,
                    CreatedDateTime = at
                };
                _sheet.ExternalReferences.Add(item);
                track = new ExternalReferenceTrack(item);
                _references[reference.ReferenceId] = track;
            }

            if (!string.Equals(track.Signature, reference.Signature, StringComparison.Ordinal))
            {
                WriteReference(track, version, at, reference, isDeleted: false);
                track.Signature = reference.Signature;
            }
        }

        var now = references.Select(reference => reference.ReferenceId).ToHashSet();
        foreach (var referenceId in _previousReferences.Where(id => !now.Contains(id)))
        {
            var track = _references[referenceId];
            var last = track.Last!;
            WriteReference(
                track,
                version,
                at,
                new SnapshotExternalReference
                {
                    ReferenceId = referenceId,
                    ExternalId = last.ExternalId,
                    SystemName = last.SystemName,
                    OwnerName = last.OwnerName,
                    Url = last.Url,
                    Type = last.Type,
                    Grouping = last.Grouping,
                    SystemIconUrl = last.SystemIconUrl,
                    Description = last.Description
                },
                isDeleted: true);
            track.Signature = Removed;
        }

        _previousReferences = now;
    }

    private void WriteReference(ExternalReferenceTrack track, DbSheetVersion version, DateTime at, SnapshotExternalReference reference, bool isDeleted)
    {
        var revision = new DbSheetExternalReferenceRevision
        {
            Id = 0,
            SheetExternalReferenceId = 0,
            SheetId = _sheet.Id,
            SheetVersionId = 0,
            RevisionNumber = ++track.RevisionCount,
            ExternalId = reference.ExternalId,
            SystemName = reference.SystemName,
            OwnerName = reference.OwnerName,
            Url = reference.Url,
            Type = reference.Type,
            Grouping = reference.Grouping,
            SystemIconUrl = reference.SystemIconUrl,
            Description = reference.Description,
            IsDeleted = isDeleted,
            CreatedDateTime = at,
            PublishedDateTime = at,
            SupersededDateTime = null,
            SheetVersion = version
        };
        track.Item.Revisions.Add(revision);
        if (track.Last is { } replaced)
        {
            replaced.SupersededDateTime = at;
        }

        track.Last = revision;
    }

    // Writes a revision unless the item's last published revision already records this state. A published revision
    // replaces the one before it at the version's moment; a draft leaves it alone.
    private bool Write<TItem, TRevision>(WriteContext context, ItemTrack<TItem, TRevision> track, string state, Func<int, TRevision> create)
        where TItem : class
        where TRevision : class, IDbSheetRevision
    {
        if (string.Equals(track.PublishedState, state, StringComparison.Ordinal))
        {
            return false;
        }

        var revision = create(++track.RevisionCount);
        Attach(track.Item, revision);
        _writtenForCopy.Add(revision);

        if (context.Version is null)
        {
            return true;
        }

        if (track.LastPublished is { } replaced)
        {
            replaced.SupersededDateTime = context.At;
        }

        track.LastPublished = revision;
        track.PublishedState = state;
        return true;
    }

    private static void Attach(object item, IDbSheetRevision revision)
    {
        switch (item, revision)
        {
            case (DbSheetTable table, DbSheetTableRevision tableRevision):
                table.Revisions.Add(tableRevision);
                break;
            case (DbSheetTableGroup group, DbSheetTableGroupRevision groupRevision):
                group.Revisions.Add(groupRevision);
                break;
            case (DbSheetTableRow row, DbSheetTableRowRevision rowRevision):
                row.Revisions.Add(rowRevision);
                break;
            case (DbSheetProperty property, DbSheetPropertyRevision propertyRevision):
                property.Revisions.Add(propertyRevision);
                break;
            case (DbSheetLinkedTable linked, DbSheetLinkedTableRevision linkedRevision):
                linked.Revisions.Add(linkedRevision);
                break;
            default:
                throw new InvalidOperationException($"A {revision.GetType().Name} can't belong to a {item.GetType().Name}.");
        }
    }

    // Items keep their ReferenceID. One that turns up twice on the sheet (an item moved to another parent, say, which
    // becomes a new item) is given a new one for the second item, as two items can't share one.
    private Guid Claim(Guid referenceId, string what)
    {
        if (referenceId != Guid.Empty && _claimedReferenceIds.Add(referenceId))
        {
            return referenceId;
        }

        var fresh = Guid.NewGuid();
        _claimedReferenceIds.Add(fresh);
        _warnings.Add($"The ReferenceID {referenceId} of a {what} was used by another item of the sheet; it was given {fresh}.");
        return fresh;
    }
}
