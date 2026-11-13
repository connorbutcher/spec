using System.Globalization;
using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Database.Models.Values;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Migrator.Legacy;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// Rebuilds a legacy sheet, a whole copy per version, as identity rows and revisions. The copies are replayed
/// oldest first. Each becomes a <see cref="DbSheetVersion"/>, and each item in it is matched with itself in the
/// copy before (<see cref="KeyAssigner"/>). An item gets a published revision only when it is new or differs
/// from its last one: a table's title or order, a section's or column block's order, a row's order or any of
/// its cells' contents. An item missing from a copy gets a revision removing it, unless what it was in went too,
/// as removing a table or section takes what is in it with it. The working copy, if there is one, is replayed
/// last in the same way but as drafts held by its editor.
/// <para>
/// Nothing touches the database: the result is a graph of new entities under the given <see cref="DbSheet"/>,
/// with every <c>SupersededAtUtc</c> already set, so it saves as inserts only and the one-current-revision
/// indexes are never broken along the way.
/// </para>
/// </summary>
internal sealed class SheetHistoryBuilder
{
    private const string Removed = "removed";

    private readonly DbSheet sheet;
    private readonly DateTime nowUtc;
    private readonly KeyAssigner assigner = new();
    private readonly List<string> warnings = [];
    private readonly List<BuiltVersion> checks = [];
    private readonly Dictionary<int, ItemTrack<DbSheetTable, DbSheetTableRevision>> tables = [];
    private readonly Dictionary<int, ItemTrack<DbSheetColumnBlock, DbSheetColumnBlockRevision>> blocks = [];
    private readonly Dictionary<int, ItemTrack<DbSheetSection, DbSheetSectionRevision>> sections = [];
    private readonly Dictionary<int, ItemTrack<DbSheetRow, DbSheetRowRevision>> rows = [];
    private readonly Dictionary<CellKey, DbSheetCell> cells = [];
    private readonly Dictionary<CellKey, Guid> cellPublicIds = [];
    private SnapshotKeys previousKeys = new();

    private SheetHistoryBuilder(DbSheet sheet, DateTime nowUtc)
    {
        this.sheet = sheet;
        this.nowUtc = nowUtc;
    }

    /// <param name="sheet">The migrated sheet, already saved so that it has its <c>Id</c>. Its tables and versions are added to it.</param>
    /// <param name="copies">Every copy of the legacy sheet, in any order.</param>
    /// <param name="nowUtc">When the migration runs: the time on the working copy's drafts.</param>
    public static BuiltSheet Build(DbSheet sheet, IReadOnlyList<LegacyCopy> copies, DateTime nowUtc)
    {
        var builder = new SheetHistoryBuilder(sheet, nowUtc);
        builder.BuildAll(copies);
        return new BuiltSheet(sheet, builder.checks, builder.warnings);
    }

    private void BuildAll(IReadOnlyList<LegacyCopy> copies)
    {
        var published = copies.Where(copy => !copy.IsWorkingCopy).OrderBy(copy => copy.VersionNumber).ToList();
        var working = copies.Where(copy => copy.IsWorkingCopy).ToList();
        Validate(published, working);

        DateTime? last = null;
        foreach (var copy in published)
        {
            var at = copy.PublishedAtUtc!.Value;
            if (last is { } previous && at <= previous)
            {
                // Two versions at one moment can't be told apart "as of" a time, and one older than the version
                // before it would break the timeline, so the later version moves to just after the earlier one.
                at = previous.AddTicks(1);
                warnings.Add(string.Create(
                    CultureInfo.InvariantCulture,
                    $"Version {copy.VersionNumber} was published at {copy.PublishedAtUtc:O}, not after the version before it; it is moved to {at:O}."));
            }

            var version = new DbSheetVersion
            {
                VersionNumber = copy.VersionNumber,
                PublishedAtUtc = at,
                PublishedByUserId = copy.UserId,
                Note = copy.Note,
            };
            sheet.Versions.Add(version);

            Apply(copy, version, at);
            checks.Add(new BuiltVersion(copy.VersionNumber, at, copy.Snapshot));
            last = at;
        }

        if (working.Count == 1)
        {
            Apply(working[0], null, nowUtc);
            checks.Add(new BuiltVersion(working[0].VersionNumber, null, working[0].Snapshot));
        }

        KeepPublicIds();
    }

    private static void Validate(List<LegacyCopy> published, List<LegacyCopy> working)
    {
        if (working.Count > 1)
        {
            throw new MigrationException($"It has {working.Count} unpublished copies; a sheet can only have one working copy.");
        }

        var duplicate = published.GroupBy(copy => copy.VersionNumber).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new MigrationException($"It has more than one version {duplicate.Key}.");
        }

        if (published.Count > 0 && published[0].VersionNumber < 1)
        {
            throw new MigrationException($"It has a version {published[0].VersionNumber}; version numbers start at 1.");
        }
    }

    /// <summary>Writes the revisions that take every item from how it was to how it is in <paramref name="copy"/>.</summary>
    /// <param name="version">The version the copy becomes, or null for the working copy, whose revisions are drafts.</param>
    private void Apply(LegacyCopy copy, DbSheetVersion? version, DateTime at)
    {
        var snapshot = copy.Snapshot;
        var keys = assigner.Assign(snapshot);
        var context = new WriteContext(version, at, copy.UserId);

        foreach (var table in snapshot.Tables)
        {
            ApplyTable(context, keys, table);
        }

        foreach (var block in snapshot.ColumnBlocks)
        {
            ApplyColumnBlock(context, keys, block);
        }

        foreach (var section in ParentsFirst(snapshot.Sections))
        {
            ApplySection(context, keys, section);
        }

        foreach (var row in snapshot.Rows)
        {
            ApplyRow(context, snapshot, keys, row);
        }

        RemoveMissing(context, keys);

        if (version is not null)
        {
            previousKeys = keys;
        }
    }

    private void ApplyTable(WriteContext context, SnapshotKeys keys, SnapshotTable table)
    {
        var key = keys.Tables[table.Id];
        if (!tables.TryGetValue(key, out var track))
        {
            var identity = new DbSheetTable { TableTemplateVersionId = table.TableTemplateVersionId, CreatedAtUtc = context.At };
            sheet.Tables.Add(identity);
            track = new ItemTrack<DbSheetTable, DbSheetTableRevision>(identity, null, null);
            tables[key] = track;
        }

        track.PublicId = table.PublicId;
        Write(context, track, $"{table.DisplayOrder}|{table.Title}", () => new DbSheetTableRevision
        {
            Title = table.Title,
            DisplayOrder = table.DisplayOrder,
        });
    }

    private void ApplyColumnBlock(WriteContext context, SnapshotKeys keys, SnapshotColumnBlock block)
    {
        var key = keys.ColumnBlocks[block.Id];
        var tableKey = keys.Tables[block.TableId];
        if (!blocks.TryGetValue(key, out var track))
        {
            var identity = new DbSheetColumnBlock { TemplateColumnBlockId = block.TemplateColumnBlockId, CreatedAtUtc = context.At };
            tables[tableKey].Identity.ColumnBlocks.Add(identity);
            track = new ItemTrack<DbSheetColumnBlock, DbSheetColumnBlockRevision>(identity, tableKey, null);
            blocks[key] = track;
        }

        track.PublicId = block.PublicId;
        Write(context, track, $"{block.DisplayOrder}", () => new DbSheetColumnBlockRevision { DisplayOrder = block.DisplayOrder });
    }

    private void ApplySection(WriteContext context, SnapshotKeys keys, SnapshotSection section)
    {
        var key = keys.Sections[section.Id];
        var tableKey = keys.Tables[section.TableId];
        int? parentKey = section.ParentSectionId is { } parentId ? keys.Sections[parentId] : null;
        if (!sections.TryGetValue(key, out var track))
        {
            var identity = new DbSheetSection
            {
                TemplateSectionId = section.TemplateSectionId,
                ParentSheetSection = parentKey is { } parent ? sections[parent].Identity : null,
                CreatedAtUtc = context.At,
            };
            tables[tableKey].Identity.Sections.Add(identity);
            track = new ItemTrack<DbSheetSection, DbSheetSectionRevision>(identity, tableKey, parentKey);
            sections[key] = track;
        }

        track.PublicId = section.PublicId;
        Write(context, track, $"{section.DisplayOrder}", () => new DbSheetSectionRevision { DisplayOrder = section.DisplayOrder });
    }

    private void ApplyRow(WriteContext context, SheetSnapshot snapshot, SnapshotKeys keys, SnapshotRow row)
    {
        var key = keys.Rows[row.Id];
        var sectionKey = keys.Sections[row.SectionId];
        if (!rows.TryGetValue(key, out var track))
        {
            var identity = new DbSheetRow { TemplateRowId = row.TemplateRowId, CreatedAtUtc = context.At };
            sections[sectionKey].Identity.Rows.Add(identity);
            track = new ItemTrack<DbSheetRow, DbSheetRowRevision>(identity, null, sectionKey);
            rows[key] = track;
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

        var state = $"{row.DisplayOrder}|{KeyAssigner.RowSignature(snapshot, keys, row.Id)}";
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

    /// <summary>The identity cell of a row's cell, made the first time the row has it.</summary>
    private DbSheetCell CellFor(DbSheetRow row, int rowKey, SnapshotKeys keys, SnapshotCell cell)
    {
        int? blockKey = cell.ColumnBlockId is { } blockId ? keys.ColumnBlocks[blockId] : null;
        var cellKey = new CellKey(rowKey, cell.TemplateCellId, blockKey);
        if (!cells.TryGetValue(cellKey, out var sheetCell))
        {
            sheetCell = new DbSheetCell
            {
                TemplateCellId = cell.TemplateCellId,
                SheetColumnBlock = blockKey is { } block ? blocks[block].Identity : null,
            };
            row.Cells.Add(sheetCell);
            cells[cellKey] = sheetCell;
        }

        cellPublicIds[cellKey] = cell.PublicId;
        return sheetCell;
    }

    /// <summary>
    /// Removes what was in the copy before and isn't in this one. Only the outermost thing that went is removed,
    /// as the app does: a row whose section went with it stays as it was, hidden by its section.
    /// </summary>
    private void RemoveMissing(WriteContext context, SnapshotKeys keys)
    {
        var tablesNow = keys.Tables.Values.ToHashSet();
        var sectionsNow = keys.Sections.Values.ToHashSet();
        var blocksNow = keys.ColumnBlocks.Values.ToHashSet();
        var rowsNow = keys.Rows.Values.ToHashSet();

        foreach (var key in previousKeys.Tables.Values.Where(key => !tablesNow.Contains(key)))
        {
            var track = tables[key];
            var last = track.LastPublished!;
            Write(context, track, Removed, () => new DbSheetTableRevision { Title = last.Title, DisplayOrder = last.DisplayOrder, IsDeleted = true });
        }

        foreach (var key in previousKeys.ColumnBlocks.Values.Where(key => !blocksNow.Contains(key)))
        {
            var track = blocks[key];
            if (tablesNow.Contains(track.TableKey!.Value))
            {
                var last = track.LastPublished!;
                Write(context, track, Removed, () => new DbSheetColumnBlockRevision { DisplayOrder = last.DisplayOrder, IsDeleted = true });
            }
        }

        foreach (var key in previousKeys.Sections.Values.Where(key => !sectionsNow.Contains(key)))
        {
            var track = sections[key];
            var containerKept = track.SectionKey is { } parent ? sectionsNow.Contains(parent) : tablesNow.Contains(track.TableKey!.Value);
            if (containerKept)
            {
                var last = track.LastPublished!;
                Write(context, track, Removed, () => new DbSheetSectionRevision { DisplayOrder = last.DisplayOrder, IsDeleted = true });
            }
        }

        foreach (var key in previousKeys.Rows.Values.Where(key => !rowsNow.Contains(key)))
        {
            var track = rows[key];
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

    /// <summary>
    /// Adds a revision recording <paramref name="state"/>, unless the item's last published revision already
    /// does. A published revision replaces the one before it at the same moment.
    /// </summary>
    /// <returns>Whether a revision was added.</returns>
    private bool Write<TIdentity, TRevision>(WriteContext context, ItemTrack<TIdentity, TRevision> track, string state, Func<TRevision> create)
        where TIdentity : class
        where TRevision : class, IDbSheetRevision
    {
        if (string.Equals(track.PublishedState, state, StringComparison.Ordinal))
        {
            return false;
        }

        var revision = create();
        revision.SheetId = sheet.Id;
        revision.RevisionNumber = ++track.RevisionCount;
        revision.AuthorUserId = context.UserId;
        revision.CreatedAtUtc = context.At;
        revision.UpdatedAtUtc = context.At;
        Attach(track.Identity, revision, context.Version);

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

    /// <summary>Hangs a revision off its item, and off its version when published, so it is saved with the sheet.</summary>
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
        }
    }

    /// <summary>Sections ordered so each comes after the section it is in.</summary>
    private static List<SnapshotSection> ParentsFirst(IReadOnlyList<SnapshotSection> all)
    {
        var byId = all.ToDictionary(section => section.Id);
        var depths = new Dictionary<int, int>();

        int Depth(SnapshotSection section)
        {
            if (!depths.TryGetValue(section.Id, out var depth))
            {
                depth = section.ParentSectionId is { } parentId ? Depth(byId[parentId]) + 1 : 0;
                depths[section.Id] = depth;
            }

            return depth;
        }

        return all.OrderBy(Depth).ToList();
    }

    /// <summary>
    /// Gives each identity row the public id its item had in the latest copy it was in, so links into the
    /// current sheet keep working. A public id already taken by another item gets a new one instead.
    /// </summary>
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
            warnings.Add($"The public id {publicId} of a {what} was used more than once; it was given {fresh}.");
            return fresh;
        }

        foreach (var track in tables.Values)
        {
            track.Identity.PublicId = Claim(track.PublicId, "table");
        }

        foreach (var track in blocks.Values)
        {
            track.Identity.PublicId = Claim(track.PublicId, "column block");
        }

        foreach (var track in sections.Values)
        {
            track.Identity.PublicId = Claim(track.PublicId, "section");
        }

        foreach (var track in rows.Values)
        {
            track.Identity.PublicId = Claim(track.PublicId, "row");
        }

        foreach (var (key, cell) in cells)
        {
            cell.PublicId = Claim(cellPublicIds[key], "cell");
        }
    }
}
