using System.Globalization;
using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Database.Models.Approvals;
using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Database.Models.Values;
using PUSpecSheet.Migrator.Legacy;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Building;

// Turns the copies of one legacy sheet into its history on the revision tables. Each published copy, oldest first,
// becomes a version, and each item gets a published revision only when it is new or differs from its last one; the
// revision replaces the one before at the version's moment. An item missing from a copy gets a removing revision,
// unless what it was in went too. The working copy, if there is one, becomes drafts by its editor. Workflow steps
// become submissions.
internal sealed class SheetHistoryBuilder
{
    private const string Removed = "removed";
    private const int LongestReason = 1000;
    private const string NoReason = "(no reason given)";

    private readonly DbSheet _sheet;
    private readonly DateTime _nowUtc;
    private readonly List<string> _warnings = [];
    private readonly KeyAssigner _assigner;
    private readonly List<BuiltVersion> _checks = [];
    private readonly List<PlannedSubmission> _submissions = [];
    private readonly List<IDbSheetRevision> _writtenForCopy = [];
    private readonly Dictionary<int, ItemTrack<DbSheetTable, DbSheetTableRevision>> _tables = [];
    private readonly Dictionary<int, ItemTrack<DbSheetColumnBlock, DbSheetColumnBlockRevision>> _blocks = [];
    private readonly Dictionary<int, ItemTrack<DbSheetSection, DbSheetSectionRevision>> _sections = [];
    private readonly Dictionary<int, ItemTrack<DbSheetRow, DbSheetRowRevision>> _rows = [];
    private readonly Dictionary<int, ItemTrack<DbSheetProperty, DbSheetPropertyRevision>> _properties = [];
    private readonly Dictionary<Guid, ItemTrack<DbSheetLinkedTable, DbSheetLinkedTableRevision>> _linkedTables = [];
    private readonly Dictionary<Guid, ExternalReferenceTrack> _references = [];
    private readonly Dictionary<CellKey, DbSheetCell> _cells = [];
    private readonly Dictionary<CellKey, Guid> _cellPublicIds = [];
    private SnapshotKeys _previousKeys = new();
    private HashSet<int> _previousProperties = [];
    private HashSet<Guid> _previousLinkedTables = [];
    private HashSet<Guid> _previousReferences = [];

    private SheetHistoryBuilder(DbSheet sheet, DateTime nowUtc)
    {
        _sheet = sheet;
        _nowUtc = nowUtc;
        _assigner = new KeyAssigner(_warnings);
    }

    public static BuiltSheet Build(DbSheet sheet, IReadOnlyList<LegacyCopy> copies, DateTime nowUtc)
    {
        var builder = new SheetHistoryBuilder(sheet, nowUtc);
        builder.BuildAll(copies);
        return new BuiltSheet
        {
            Sheet = sheet,
            Checks = builder._checks,
            Submissions = builder._submissions,
            Warnings = builder._warnings,
        };
    }

    private void BuildAll(IReadOnlyList<LegacyCopy> copies)
    {
        var published = copies.Where(copy => !copy.IsWorkingCopy).OrderBy(copy => copy.PublishedAtUtc).ThenBy(copy => copy.Id).ToList();
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
            var at = copy.PublishedAtUtc!.Value;
            if (last is { } previous && at <= previous)
            {
                // Two versions at one moment can't be told apart "as of" a time, so the later moves to just after.
                at = previous.AddTicks(1);
                _warnings.Add(string.Create(
                    CultureInfo.InvariantCulture,
                    $"Version {number} (copy {copy.Id}) was published at {copy.PublishedAtUtc:O}, the same moment as the version before it; it is moved to {at:O}."));
            }

            var version = new DbSheetVersion
            {
                VersionNumber = number,
                PublishedAtUtc = at,
                PublishedByUserId = copy.UserId,
                Note = string.IsNullOrWhiteSpace(copy.ChangeLog) ? null : copy.ChangeLog,
                SourceSpecSheetId = copy.Id,
                LegacySpecSheetId = copy.LegacySpecSheetId,
                HasBeenMigrated = copy.HasBeenMigrated,
            };
            _sheet.Versions.Add(version);

            Apply(copy, version, at);
            PlanSubmissions(copy, version, at);
            _checks.Add(new BuiltVersion { VersionNumber = number, MomentUtc = at, Expected = copy.Snapshot });
            last = at;
        }

        if (working.Count == 1)
        {
            var copy = working[0];
            if (last is { } lastPublished && copy.CreatedAtUtc < lastPublished)
            {
                _warnings.Add($"The working copy (copy {copy.Id}) was started before the last version was published; it is kept as drafts on top of that version.");
            }

            Apply(copy, null, _nowUtc);
            PlanSubmissions(copy, null, _nowUtc);
            _checks.Add(new BuiltVersion { VersionNumber = number + 1, Expected = copy.Snapshot });
        }

        KeepPublicIds();
    }

    private void Apply(LegacyCopy copy, DbSheetVersion? version, DateTime at)
    {
        _writtenForCopy.Clear();
        var snapshot = copy.Snapshot;
        var keys = _assigner.Assign(snapshot);
        var context = new WriteContext { Version = version, At = at, UserId = copy.UserId };

        // Orders already published are kept where they still fit, so a moved neighbour doesn't make an item look changed.
        var tableOrders = KeepOrders(snapshot.Tables, table => table.Id, table => table.DisplayOrder, _ => 0, table => ExistingOrder(_tables, keys.Tables[table.Id]));
        var sectionOrders = KeepOrders(
            snapshot.Sections,
            section => section.Id,
            section => section.DisplayOrder,
            section => (section.TableId, section.ParentSectionId),
            section => ExistingOrder(_sections, keys.Sections[section.Id]));
        var rowOrders = KeepOrders(snapshot.Rows, row => row.Id, row => row.DisplayOrder, row => row.SectionId, row => ExistingOrder(_rows, keys.Rows[row.Id]));

        foreach (var table in snapshot.Tables)
        {
            ApplyTable(context, keys, table with { DisplayOrder = tableOrders[table.Id] });
        }

        foreach (var block in snapshot.ColumnBlocks)
        {
            ApplyColumnBlock(context, keys, block);
        }

        foreach (var section in KeyAssigner.ParentsFirst(snapshot.Sections))
        {
            ApplySection(context, keys, section with { DisplayOrder = sectionOrders[section.Id] });
        }

        foreach (var row in snapshot.Rows)
        {
            ApplyRow(context, snapshot, keys, row with { DisplayOrder = rowOrders[row.Id] });
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

    private static Dictionary<int, int> KeepOrders<TItem, TParent>(
        IEnumerable<TItem> items,
        Func<TItem, int> id,
        Func<TItem, int> copyOrder,
        Func<TItem, TParent> parent,
        Func<TItem, int?> existingOrder)
    {
        var orders = new Dictionary<int, int>();
        foreach (var siblings in items.GroupBy(parent))
        {
            var sorted = siblings.OrderBy(copyOrder).ThenBy(id).ToList();
            var kept = OrderKeeper.Assign(sorted.Select(existingOrder).ToList());
            for (var index = 0; index < sorted.Count; index++)
            {
                orders[id(sorted[index])] = kept[index];
            }
        }

        return orders;
    }

    private static int? ExistingOrder<TKey, TIdentity, TRevision>(Dictionary<TKey, ItemTrack<TIdentity, TRevision>> tracks, TKey key)
        where TKey : notnull
        where TIdentity : class
        where TRevision : class, IDbSheetRevision
    {
        return tracks.TryGetValue(key, out var track) && track.LastPublished is { IsDeleted: false } last ? last.DisplayOrder : null;
    }

    private void ApplyTable(WriteContext context, SnapshotKeys keys, SnapshotTable table)
    {
        var key = keys.Tables[table.Id];
        if (!_tables.TryGetValue(key, out var track))
        {
            var identity = new DbSheetTable { TableTemplateVersionId = table.TableTemplateVersionId, CreatedAtUtc = context.At };
            _sheet.Tables.Add(identity);
            track = new ItemTrack<DbSheetTable, DbSheetTableRevision>(identity, null, null);
            _tables[key] = track;
        }

        track.PublicId = table.PublicId;
        Write(context, track, $"{table.DisplayOrder}|{table.Title}|{table.WidgetValues}|{table.Notes}", () => new DbSheetTableRevision
        {
            Title = table.Title,
            DisplayOrder = table.DisplayOrder,
            WidgetValues = table.WidgetValues,
            Notes = table.Notes,
        });
    }

    private void ApplyColumnBlock(WriteContext context, SnapshotKeys keys, SnapshotColumnBlock block)
    {
        var key = keys.ColumnBlocks[block.Id];
        var tableKey = keys.Tables[block.TableId];
        if (!_blocks.TryGetValue(key, out var track))
        {
            var identity = new DbSheetColumnBlock { TemplateColumnBlockId = block.TemplateColumnBlockId, CreatedAtUtc = context.At };
            _tables[tableKey].Identity.ColumnBlocks.Add(identity);
            track = new ItemTrack<DbSheetColumnBlock, DbSheetColumnBlockRevision>(identity, tableKey, null);
            _blocks[key] = track;
        }

        track.PublicId = block.PublicId;
        Write(context, track, $"{block.DisplayOrder}", () => new DbSheetColumnBlockRevision { DisplayOrder = block.DisplayOrder });
    }

    private void ApplySection(WriteContext context, SnapshotKeys keys, SnapshotSection section)
    {
        var key = keys.Sections[section.Id];
        var tableKey = keys.Tables[section.TableId];
        int? parentKey = section.ParentSectionId is { } parentId ? keys.Sections[parentId] : null;
        if (!_sections.TryGetValue(key, out var track))
        {
            var identity = new DbSheetSection
            {
                TemplateSectionId = section.TemplateSectionId,
                ParentSheetSection = parentKey is { } parent ? _sections[parent].Identity : null,
                CreatedAtUtc = context.At,
            };
            _tables[tableKey].Identity.Sections.Add(identity);
            track = new ItemTrack<DbSheetSection, DbSheetSectionRevision>(identity, tableKey, parentKey);
            _sections[key] = track;
        }

        track.PublicId = section.PublicId;
        Write(context, track, $"{section.DisplayOrder}|{section.WidgetValues}|{section.LinkedSectionPublicId}", () => new DbSheetSectionRevision
        {
            DisplayOrder = section.DisplayOrder,
            WidgetValues = section.WidgetValues,
            LinkedSectionPublicId = section.LinkedSectionPublicId,
        });
    }

    private void ApplyRow(WriteContext context, SheetSnapshot snapshot, SnapshotKeys keys, SnapshotRow row)
    {
        var key = keys.Rows[row.Id];
        var sectionKey = keys.Sections[row.SectionId];
        if (!_rows.TryGetValue(key, out var track))
        {
            var identity = new DbSheetRow { TemplateRowId = row.TemplateRowId, CreatedAtUtc = context.At };
            _sections[sectionKey].Identity.Rows.Add(identity);
            track = new ItemTrack<DbSheetRow, DbSheetRowRevision>(identity, null, sectionKey);
            _rows[key] = track;
        }

        track.PublicId = row.PublicId;

        var contents = new List<(DbSheetCell Cell, CellContent Content)>();
        foreach (var cell in snapshot.CellsOf(row.Id))
        {
            var sheetCell = CellFor(track.Identity, key, keys, cell);
            if (!cell.Content.IsEmpty)
            {
                contents.Add((sheetCell, cell.Content));
            }
        }

        var state = $"{row.DisplayOrder}|{RowSignature(snapshot, row.Id)}";
        var written = Write(context, track, state, () =>
        {
            var revision = new DbSheetRowRevision { DisplayOrder = row.DisplayOrder };
            AddValues(revision, contents);
            return revision;
        });

        if (written && context.Version is not null)
        {
            track.PublishedCells = contents;
        }
    }

    private static string RowSignature(SheetSnapshot snapshot, int rowId)
    {
        var cells = snapshot.CellsOf(rowId)
            .Where(cell => !cell.Content.IsEmpty)
            .Select(cell => $"{cell.TemplateCellId}={cell.Content.Signature}")
            .Order(StringComparer.Ordinal);
        return string.Join('\n', cells);
    }

    private DbSheetCell CellFor(DbSheetRow row, int rowKey, SnapshotKeys keys, SnapshotCell cell)
    {
        int? blockKey = cell.ColumnBlockId is { } blockId ? keys.ColumnBlocks[blockId] : null;
        var cellKey = new CellKey { RowKey = rowKey, TemplateCellId = cell.TemplateCellId, ColumnBlockKey = blockKey };
        if (!_cells.TryGetValue(cellKey, out var sheetCell))
        {
            sheetCell = new DbSheetCell
            {
                TemplateCellId = cell.TemplateCellId,
                SheetColumnBlock = blockKey is { } block ? _blocks[block].Identity : null,
            };
            row.Cells.Add(sheetCell);
            _cells[cellKey] = sheetCell;
        }

        _cellPublicIds[cellKey] = cell.PublicId;
        return sheetCell;
    }

    private void RemoveMissing(WriteContext context, SnapshotKeys keys)
    {
        var tablesNow = keys.Tables.Values.ToHashSet();
        var sectionsNow = keys.Sections.Values.ToHashSet();
        var blocksNow = keys.ColumnBlocks.Values.ToHashSet();
        var rowsNow = keys.Rows.Values.ToHashSet();

        foreach (var key in _previousKeys.Tables.Values.Where(key => !tablesNow.Contains(key)))
        {
            var track = _tables[key];
            var last = track.LastPublished!;
            Write(context, track, Removed, () => new DbSheetTableRevision
            {
                Title = last.Title,
                DisplayOrder = last.DisplayOrder,
                WidgetValues = last.WidgetValues,
                Notes = last.Notes,
                IsDeleted = true,
            });
        }

        foreach (var key in _previousKeys.ColumnBlocks.Values.Where(key => !blocksNow.Contains(key)))
        {
            var track = _blocks[key];
            if (tablesNow.Contains(track.TableKey!.Value))
            {
                var last = track.LastPublished!;
                Write(context, track, Removed, () => new DbSheetColumnBlockRevision { DisplayOrder = last.DisplayOrder, IsDeleted = true });
            }
        }

        foreach (var key in _previousKeys.Sections.Values.Where(key => !sectionsNow.Contains(key)))
        {
            var track = _sections[key];
            var containerKept = track.SectionKey is { } parent ? sectionsNow.Contains(parent) : tablesNow.Contains(track.TableKey!.Value);
            if (containerKept)
            {
                var last = track.LastPublished!;
                Write(context, track, Removed, () => new DbSheetSectionRevision
                {
                    DisplayOrder = last.DisplayOrder,
                    WidgetValues = last.WidgetValues,
                    LinkedSectionPublicId = last.LinkedSectionPublicId,
                    IsDeleted = true,
                });
            }
        }

        foreach (var key in _previousKeys.Rows.Values.Where(key => !rowsNow.Contains(key)))
        {
            var track = _rows[key];
            if (sectionsNow.Contains(track.SectionKey!.Value))
            {
                // A removed row keeps its last values, as the app's removal drafts do.
                var last = track.LastPublished!;
                var kept = track.PublishedCells;
                Write(context, track, Removed, () =>
                {
                    var revision = new DbSheetRowRevision { DisplayOrder = last.DisplayOrder, IsDeleted = true };
                    AddValues(revision, kept);
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
                var identity = new DbSheetProperty { SheetTypePropertyId = propertyId, CreatedAtUtc = context.At };
                _sheet.Properties.Add(identity);
                track = new ItemTrack<DbSheetProperty, DbSheetPropertyRevision>(identity, null, null);
                _properties[propertyId] = track;
            }

            Write(context, track, $"={value}", () => new DbSheetPropertyRevision { Value = value });
        }

        foreach (var propertyId in _previousProperties.Where(id => !snapshot.Properties.ContainsKey(id)))
        {
            var last = _properties[propertyId].LastPublished!;
            Write(context, _properties[propertyId], Removed, () => new DbSheetPropertyRevision { Value = last.Value, IsDeleted = true });
        }

        if (context.Version is not null)
        {
            _previousProperties = snapshot.Properties.Keys.ToHashSet();
        }
    }

    private void ApplyLinkedTables(WriteContext context, SheetSnapshot snapshot)
    {
        var orders = KeepOrders(
            snapshot.LinkedTables.Select((link, index) => (Index: index, Link: link)).ToList(),
            item => item.Index,
            item => item.Link.DisplayOrder,
            _ => 0,
            item => ExistingOrder(_linkedTables, item.Link.LinkedTablePublicId));

        for (var index = 0; index < snapshot.LinkedTables.Count; index++)
        {
            var link = snapshot.LinkedTables[index];
            if (!_linkedTables.TryGetValue(link.LinkedTablePublicId, out var track))
            {
                var identity = new DbSheetLinkedTable
                {
                    LinkedTablePublicId = link.LinkedTablePublicId,
                    LinkedByUserId = link.LinkedByUserId,
                    LinkedAtUtc = link.LinkedAtUtc,
                    CreatedAtUtc = context.At,
                };
                _sheet.LinkedTables.Add(identity);
                track = new ItemTrack<DbSheetLinkedTable, DbSheetLinkedTableRevision>(identity, null, null);
                _linkedTables[link.LinkedTablePublicId] = track;
            }

            var order = orders[index];
            Write(context, track, $"{order}", () => new DbSheetLinkedTableRevision { DisplayOrder = order });
        }

        var now = snapshot.LinkedTables.Select(link => link.LinkedTablePublicId).ToHashSet();
        foreach (var linkedId in _previousLinkedTables.Where(id => !now.Contains(id)))
        {
            var last = _linkedTables[linkedId].LastPublished!;
            Write(context, _linkedTables[linkedId], Removed, () => new DbSheetLinkedTableRevision { DisplayOrder = last.DisplayOrder, IsDeleted = true });
        }

        if (context.Version is not null)
        {
            _previousLinkedTables = now;
        }
    }

    // External references are pushed for a version, not edited, so they have no drafts or authors: each version
    // gets a revision for a reference that is new, changed or gone.
    private void ApplyExternalReferences(DbSheetVersion version, DateTime at, IReadOnlyList<SnapshotExternalReference> references)
    {
        foreach (var reference in references)
        {
            if (!_references.TryGetValue(reference.ReferenceId, out var track))
            {
                var identity = new DbSheetExternalReference { ReferenceId = reference.ReferenceId, CreatedAtUtc = at };
                _sheet.ExternalReferences.Add(identity);
                track = new ExternalReferenceTrack(identity);
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
                    Description = last.Description,
                },
                isDeleted: true);
            track.Signature = Removed;
        }

        _previousReferences = now;
    }

    private static void WriteReference(ExternalReferenceTrack track, DbSheetVersion version, DateTime at, SnapshotExternalReference reference, bool isDeleted)
    {
        var revision = new DbSheetExternalReferenceRevision
        {
            RevisionNumber = ++track.RevisionCount,
            IsDeleted = isDeleted,
            PublishedAtUtc = at,
            PushedAtUtc = at,
            ExternalId = reference.ExternalId,
            SystemName = reference.SystemName,
            OwnerName = reference.OwnerName,
            Url = reference.Url,
            Type = reference.Type,
            Grouping = reference.Grouping,
            SystemIconUrl = reference.SystemIconUrl,
            Description = reference.Description,
            SheetVersion = version,
        };
        track.Identity.Revisions.Add(revision);
        if (track.Last is { } replaced)
        {
            replaced.SupersededAtUtc = at;
        }

        track.Last = revision;
    }

    private bool Write<TIdentity, TRevision>(WriteContext context, ItemTrack<TIdentity, TRevision> track, string state, Func<TRevision> create)
        where TIdentity : class
        where TRevision : class, IDbSheetRevision
    {
        if (string.Equals(track.PublishedState, state, StringComparison.Ordinal))
        {
            return false;
        }

        var revision = create();
        revision.SheetId = _sheet.Id;
        revision.RevisionNumber = ++track.RevisionCount;
        revision.AuthorUserId = context.UserId;
        revision.CreatedAtUtc = context.At;
        revision.UpdatedAtUtc = context.At;
        Attach(track.Identity, revision, context.Version);
        _writtenForCopy.Add(revision);

        if (context.Version is null)
        {
            revision.Status = RevisionStatus.Draft;
            return true;
        }

        revision.Status = RevisionStatus.Published;
        revision.PublishedAtUtc = context.At;
        if (track.LastPublished is { } replaced)
        {
            replaced.SupersededAtUtc = context.At;
        }

        track.LastPublished = revision;
        track.PublishedState = state;
        return true;
    }

    private static void Attach(object identity, IDbSheetRevision revision, DbSheetVersion? version)
    {
        switch (identity, revision)
        {
            case (DbSheetTable table, DbSheetTableRevision tableRevision):
                table.Revisions.Add(tableRevision);
                tableRevision.SheetVersion = version;
                break;
            case (DbSheetColumnBlock block, DbSheetColumnBlockRevision blockRevision):
                block.Revisions.Add(blockRevision);
                blockRevision.SheetVersion = version;
                break;
            case (DbSheetSection section, DbSheetSectionRevision sectionRevision):
                section.Revisions.Add(sectionRevision);
                sectionRevision.SheetVersion = version;
                break;
            case (DbSheetRow row, DbSheetRowRevision rowRevision):
                row.Revisions.Add(rowRevision);
                rowRevision.SheetVersion = version;
                break;
            case (DbSheetProperty property, DbSheetPropertyRevision propertyRevision):
                property.Revisions.Add(propertyRevision);
                propertyRevision.SheetVersion = version;
                break;
            case (DbSheetLinkedTable linked, DbSheetLinkedTableRevision linkedRevision):
                linked.Revisions.Add(linkedRevision);
                linkedRevision.SheetVersion = version;
                break;
            default:
                throw new InvalidOperationException($"A {revision.GetType().Name} can't belong to a {identity.GetType().Name}.");
        }
    }

    private static void AddValues(DbSheetRowRevision revision, IReadOnlyList<(DbSheetCell Cell, CellContent Content)> contents)
    {
        foreach (var (cell, content) in contents)
        {
            if (content.Text is { } text)
            {
                revision.TextValues.Add(new DbTextValue { SheetCell = cell, Value = text });
            }

            if (content.Number is { } number)
            {
                revision.NumericValues.Add(new DbNumericValue { SheetCell = cell, Value = number });
            }

            if (content.Date is { } date)
            {
                revision.DateValues.Add(new DbDateValue { SheetCell = cell, Value = date });
            }

            if (content.Boolean is { } boolean)
            {
                revision.BooleanValues.Add(new DbBooleanValue { SheetCell = cell, Value = boolean });
            }

            if (content.OptionId is { } optionId)
            {
                revision.OptionValues.Add(new DbOptionValue { SheetCell = cell, CellTypeOptionId = optionId });
            }

            foreach (var slot in content.LabelledText)
            {
                revision.LabelledTextValues.Add(new DbLabelledTextValue
                {
                    SheetCell = cell,
                    SlotKey = slot.SlotKey,
                    Position = slot.Position,
                    Value = slot.Value,
                });
            }

            if (content.Settings is { } settings)
            {
                revision.CellSettings.Add(new DbCellSettingsValue { SheetCell = cell, Settings = settings });
            }

            if (content.Linked is { } linked)
            {
                revision.LinkedSheetValues.Add(new DbLinkedSheetValue
                {
                    SheetCell = cell,
                    LinkedSheetTypeId = linked.SheetTypeId,
                    LinkedPhaseId = linked.PhaseId,
                    LinkedTablePublicId = linked.TablePublicId,
                    LinkedItemPublicId = linked.ItemPublicId,
                });
            }
        }
    }

    // A copy's workflow steps become submissions. The step that approved a published copy is the submission that
    // published its version, and the revisions written for that version are its changes. A pending step on the
    // working copy is a submission awaiting approval, holding the drafts. Every other step is kept as decided.
    private void PlanSubmissions(LegacyCopy copy, DbSheetVersion? version, DateTime at)
    {
        var steps = copy.Workflow.OrderBy(step => step.StartedAtUtc).ThenBy(step => step.Id).ToList();
        var publishing = version is null ? null : steps.LastOrDefault(step => LegacyWorkflowStates.Step(step) == SubmissionStatus.Approved);
        var awaiting = version is null ? steps.LastOrDefault() : null;
        if (awaiting is not null && LegacyWorkflowStates.Step(awaiting) != SubmissionStatus.Pending)
        {
            awaiting = null;
        }

        for (var index = 0; index < steps.Count; index++)
        {
            var step = steps[index];
            var status = LegacyWorkflowStates.Step(step);
            var next = index + 1 < steps.Count ? steps[index + 1].StartedAtUtc : (DateTime?)null;
            DateTime? decidedAt = null;
            if (step == publishing || step == awaiting)
            {
                decidedAt = step == publishing ? LatestAction(step) ?? at : null;
            }
            else if (status == SubmissionStatus.Pending)
            {
                // Overtaken by a later step or by publishing, so withdrawn when that happened.
                status = SubmissionStatus.Withdrawn;
                decidedAt = next ?? at;
                _warnings.Add($"Workflow step {step.Id} (copy {copy.Id}) was never decided; it is kept as withdrawn.");
            }
            else
            {
                if (status == SubmissionStatus.Approved)
                {
                    _warnings.Add($"Workflow step {step.Id} (copy {copy.Id}) approved a copy that a later step decided again, or that wasn't published; it is kept as approved without a version.");
                }

                decidedAt = LatestAction(step) ?? next ?? at;
            }

            var submission = new DbSheetSubmission
            {
                Sheet = _sheet,
                SubmittedByUserId = copy.CreatedByUserId,
                SubmittedAtUtc = step.StartedAtUtc,
                Status = status,
                DecidedAtUtc = decidedAt,
            };
            AddApprovers(submission, step, copy);

            _submissions.Add(new PlannedSubmission
            {
                Submission = submission,
                Version = step == publishing ? version : null,
                Revisions = step == publishing || step == awaiting ? [.. _writtenForCopy] : [],
            });
        }
    }

    private void AddApprovers(DbSheetSubmission submission, LegacyWorkflowStep step, LegacyCopy copy)
    {
        foreach (var actors in step.Actors.Where(actor => actor.UserId is not null).GroupBy(actor => actor.UserId!.Value))
        {
            var ordered = actors.OrderBy(candidate => candidate.Id).ToList();
            if (ordered.Count > 1)
            {
                _warnings.Add($"User {actors.Key} is an actor of workflow step {step.Id} (copy {copy.Id}) more than once; the last is kept.");
            }

            var actor = ordered[^1];
            var decision = LegacyWorkflowStates.Actor(actor);
            var reason = string.IsNullOrWhiteSpace(actor.Comment) ? null : actor.Comment.Trim();
            if (reason is { Length: > LongestReason })
            {
                _warnings.Add($"The comment of workflow step actor {actor.Id} (copy {copy.Id}) is cut to {LongestReason} characters.");
                reason = reason[..LongestReason];
            }

            if (decision == ApprovalDecision.Rejected && reason is null)
            {
                reason = NoReason;
            }

            submission.Approvers.Add(new DbSheetSubmissionApprover
            {
                UserId = actors.Key,
                Decision = decision,
                DecidedAtUtc = decision == ApprovalDecision.Pending ? null : actor.ActionedAtUtc ?? submission.DecidedAtUtc ?? step.StartedAtUtc,
                Reason = reason,
            });
        }
    }

    private static DateTime? LatestAction(LegacyWorkflowStep step)
    {
        return step.Actors.Max(actor => actor.ActionedAtUtc);
    }

    private void KeepPublicIds()
    {
        var used = new HashSet<Guid>();

        Guid Claim(Guid publicId, string what)
        {
            if (publicId != Guid.Empty && used.Add(publicId))
            {
                return publicId;
            }

            var fresh = Guid.NewGuid();
            used.Add(fresh);
            if (publicId != Guid.Empty)
            {
                _warnings.Add($"The ReferenceID {publicId} of a {what} was used more than once; it was given {fresh}.");
            }

            return fresh;
        }

        foreach (var track in _tables.Values)
        {
            track.Identity.PublicId = Claim(track.PublicId, "table");
        }

        foreach (var track in _blocks.Values)
        {
            track.Identity.PublicId = Claim(track.PublicId, "column block");
        }

        foreach (var track in _sections.Values)
        {
            track.Identity.PublicId = Claim(track.PublicId, "section");
        }

        foreach (var track in _rows.Values)
        {
            track.Identity.PublicId = Claim(track.PublicId, "row");
        }

        foreach (var (key, cell) in _cells)
        {
            cell.PublicId = Claim(_cellPublicIds[key], "cell");
        }
    }
}
