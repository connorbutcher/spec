using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Legacy;

// Reads every copy of a legacy sheet into SheetSnapshots. All the SQL is in LegacySql. Items keep their legacy
// ReferenceIDs as public ids: a copy made from another keeps them, which is how an item is followed from one version
// to the next.
internal sealed class LegacySheetReader
{
    // A pair value becomes labelled text with these two slots, which the pair cell types must have in the new app.
    public const string PairSlotX = "x";
    public const string PairSlotY = "y";

    private const int OrderGap = 1024;
    private const int LongestText = 4000;

    private static readonly Dictionary<int, string?> _noWidgets = new();

    private readonly SqlConnection _connection;
    private readonly DesignLookup _design;

    public LegacySheetReader(SqlConnection connection, DesignLookup design)
    {
        _connection = connection;
        _design = design;
    }

    public async Task<IReadOnlyList<LegacySheet>> ListSheetsAsync(CancellationToken cancellationToken)
    {
        var sheets = new List<LegacySheet>();
        await ReadAllAsync(LegacySql.Sheets, null, reader => sheets.Add(new LegacySheet
        {
            PhaseId = reader.Int("PhaseId"),
            SheetTypeId = reader.Int("SheetTypeId"),
            CreatedAtUtc = reader.Utc("CreatedAtUtc"),
        }), cancellationToken);
        return sheets;
    }

    public async Task<LegacyCopies> LoadCopiesAsync(LegacySheet sheet, CancellationToken cancellationToken)
    {
        var warnings = new List<string>();

        var copies = new List<LegacyCopy>();
        await ReadAllAsync(LegacySql.Copies, sheet, reader =>
        {
            var published = reader.Bool("IsPublished");
            var publishedAt = reader.NullableDateTime("PublishedAtUtc");
            var createdAt = reader.Utc("CreatedAtUtc");
            var id = reader.Int("Id");
            if (published && publishedAt is null)
            {
                warnings.Add($"Copy {id} is published but has no publish time; its creation time is used.");
                publishedAt = createdAt;
            }

            copies.Add(new LegacyCopy
            {
                Id = id,
                PublishedAtUtc = published ? publishedAt : null,
                PublishedByUserId = published ? reader.NullableInt("PublishedByUserId") : null,
                CreatedByUserId = reader.Int("CreatedByUserId"),
                CreatedAtUtc = createdAt,
                ChangeLog = reader.NullableString("ChangeLog"),
                LegacySpecSheetId = reader.NullableInt("LegacySpecSheetId"),
                HasBeenMigrated = reader.Bool("HasBeenMigrated"),
            });
        }, cancellationToken);

        var tables = new List<LegacyTableRow>();
        await ReadAllAsync(LegacySql.Tables, sheet, reader => tables.Add(new LegacyTableRow
        {
            Id = reader.Int("Id"),
            CopyId = reader.Int("CopyId"),
            TableTemplateId = reader.Int("TableTemplateId"),
            Title = reader.NullableString("Title"),
            DisplayOrder = reader.Int("DisplayOrder"),
            ReferenceId = reader.GetGuid("ReferenceId"),
            HasConfigurationOverride = reader.NullableString("ConfigurationOverride") is not null,
        }), cancellationToken);

        var groups = new List<LegacyGroupRow>();
        await ReadAllAsync(LegacySql.Groups, sheet, reader => groups.Add(new LegacyGroupRow
        {
            Id = reader.Int("Id"),
            TableId = reader.Int("TableId"),
            GroupTemplateId = reader.Int("GroupTemplateId"),
            Order = reader.Int("Order"),
            ReferenceId = reader.GetGuid("ReferenceId"),
            LinkedReferenceId = reader.NullableGuid("LinkedReferenceId"),
        }), cancellationToken);

        var rows = new List<LegacyRowRow>();
        await ReadAllAsync(LegacySql.Rows, sheet, reader => rows.Add(new LegacyRowRow
        {
            Id = reader.Int("Id"),
            GroupId = reader.Int("GroupId"),
            RowTemplateId = reader.Int("RowTemplateId"),
            RowNumber = reader.Int("RowNumber"),
            ReferenceId = reader.GetGuid("ReferenceId"),
        }), cancellationToken);

        var cells = new Dictionary<int, LegacyCellRow>();
        await ReadAllAsync(LegacySql.Cells, sheet, reader =>
        {
            var id = reader.Int("Id");
            var templateCellId = reader.NullableInt("TemplateCellId");
            if (cells.ContainsKey(id))
            {
                throw new MigrationException($"Cell {id}'s row template has more than one template cell at column {reader.Int("ColumnNumber")}.");
            }

            var differs = reader.NullableString("ConfigurationOverride") is not null
                || (templateCellId is not null
                    && (reader.Int("CellTypeId") != reader.NullableInt("TemplateCellTypeId")
                        || reader.Int("CellConfigurationId") != reader.NullableInt("TemplateCellConfigurationId")));
            cells[id] = new LegacyCellRow
            {
                Id = id,
                RowId = reader.Int("RowId"),
                ColumnNumber = reader.Int("ColumnNumber"),
                ReferenceId = reader.GetGuid("ReferenceId"),
                TemplateCellId = templateCellId,
                DiffersFromTemplate = differs,
            };
        }, cancellationToken);

        var contents = await ReadContentsAsync(sheet, warnings, cancellationToken);
        var tableWidgets = await ReadWidgetsAsync(LegacySql.TableWidgets, sheet, cancellationToken);
        var groupWidgets = await ReadWidgetsAsync(LegacySql.GroupWidgets, sheet, cancellationToken);

        var notes = new Dictionary<int, List<SheetTableNote>>();
        await ReadAllAsync(LegacySql.Notes, sheet, reader => Add(notes, reader.Int("TableId"), new SheetTableNote
        {
            Note = reader.GetString("Note"),
            SourceSystem = reader.GetString("SourceSystem"),
            IsObsolete = reader.Bool("IsObsolete"),
            CreatedByUserId = reader.Int("CreatedByUserId"),
            CreatedAtUtc = reader.Utc("CreatedAtUtc"),
        }), cancellationToken);

        var properties = new Dictionary<int, Dictionary<int, string?>>();
        await ReadAllAsync(LegacySql.Properties, sheet, reader =>
        {
            var copy = Get(properties, reader.Int("CopyId"));
            copy[reader.Int("PropertyId")] = reader.NullableString("Value");
        }, cancellationToken);

        var references = new Dictionary<int, Dictionary<Guid, SnapshotExternalReference>>();
        await ReadAllAsync(LegacySql.ExternalReferences, sheet, reader =>
        {
            var reference = new SnapshotExternalReference
            {
                ReferenceId = reader.GetGuid("ReferenceId"),
                ExternalId = reader.Int("ExternalId"),
                SystemName = reader.GetString("SystemName"),
                OwnerName = reader.GetString("OwnerName"),
                Url = reader.GetString("Url"),
                Type = reader.GetString("Type"),
                Grouping = reader.NullableString("Grouping"),
                SystemIconUrl = reader.NullableString("SystemIconUrl"),
                Description = reader.NullableString("Description"),
            };
            Get(references, reader.Int("CopyId"))[reference.ReferenceId] = reference;
        }, cancellationToken);

        var linkedTables = new Dictionary<int, Dictionary<Guid, (int Id, SnapshotLinkedTable Link)>>();
        await ReadAllAsync(LegacySql.LinkedTables, sheet, reader =>
        {
            var link = new SnapshotLinkedTable
            {
                LinkedTablePublicId = reader.GetGuid("TableReferenceId"),
                DisplayOrder = reader.Int("DisplayOrder"),
                LinkedByUserId = reader.Int("LinkedByUserId"),
                LinkedAtUtc = reader.Utc("LinkedAtUtc"),
            };
            Get(linkedTables, reader.Int("CopyId"))[link.LinkedTablePublicId] = (reader.Int("Id"), link);
        }, cancellationToken);

        var workflow = await ReadWorkflowAsync(sheet, cancellationToken);

        var mismatched = cells.Values.Count(cell => cell.DiffersFromTemplate);
        if (mismatched > 0)
        {
            warnings.Add($"{mismatched} cell(s), across all copies, had a cell type, configuration or configuration override of their own; the new layout takes them from the template cell.");
        }

        var overridden = tables.Count(table => table.HasConfigurationOverride);
        if (overridden > 0)
        {
            warnings.Add($"{overridden} table(s), across all copies, had a configuration override, which the new layout doesn't keep.");
        }

        var built = copies
            .Select(copy => copy with
            {
                Snapshot = BuildSnapshot(
                    copy,
                    tables.Where(table => table.CopyId == copy.Id).ToList(),
                    groups,
                    rows,
                    cells,
                    contents,
                    tableWidgets,
                    groupWidgets,
                    notes,
                    properties.GetValueOrDefault(copy.Id) ?? new Dictionary<int, string?>(),
                    references.GetValueOrDefault(copy.Id),
                    linkedTables.GetValueOrDefault(copy.Id)),
                Workflow = workflow.GetValueOrDefault(copy.Id) ?? [],
            })
            .ToList();

        return new LegacyCopies { Copies = built, Warnings = warnings };
    }

    private SheetSnapshot BuildSnapshot(
        LegacyCopy copy,
        List<LegacyTableRow> copyTables,
        List<LegacyGroupRow> allGroups,
        List<LegacyRowRow> allRows,
        Dictionary<int, LegacyCellRow> allCells,
        Dictionary<int, CellContentParts> contents,
        Dictionary<int, Dictionary<int, string?>> tableWidgets,
        Dictionary<int, Dictionary<int, string?>> groupWidgets,
        Dictionary<int, List<SheetTableNote>> notes,
        Dictionary<int, string?> properties,
        Dictionary<Guid, SnapshotExternalReference>? references,
        Dictionary<Guid, (int Id, SnapshotLinkedTable Link)>? linkedTables)
    {
        var tables = Ordered(copyTables, _ => 0, table => table.DisplayOrder, table => table.Id)
            .Select(item => new SnapshotTable
            {
                Id = item.Item.Id,
                PublicId = item.Item.ReferenceId,
                TableTemplateVersionId = _design.VersionOfTemplate(item.Item.TableTemplateId),
                Title = item.Item.Title,
                DisplayOrder = item.Order,
                WidgetValues = SnapshotJson.WidgetValues(tableWidgets.GetValueOrDefault(item.Item.Id) ?? _noWidgets),
                Notes = SnapshotJson.Notes(notes.GetValueOrDefault(item.Item.Id) ?? []),
            })
            .ToList();
        var tableIds = tables.Select(table => table.Id).ToHashSet();

        var groups = allGroups.Where(group => tableIds.Contains(group.TableId)).ToList();
        foreach (var group in groups.Where(group => !_design.HasSection(group.GroupTemplateId)))
        {
            throw new MigrationException($"Group {group.Id} (copy {copy.Id}) uses group template {group.GroupTemplateId}, which isn't a template section in the target database.");
        }

        var parents = ParentGroups(groups, copy);
        var sections = Ordered(groups, group => (group.TableId, parents[group.Id]), group => group.Order, group => group.Id)
            .Select(item => new SnapshotSection
            {
                Id = item.Item.Id,
                PublicId = item.Item.ReferenceId,
                TableId = item.Item.TableId,
                ParentSectionId = parents[item.Item.Id],
                TemplateSectionId = item.Item.GroupTemplateId,
                DisplayOrder = item.Order,
                WidgetValues = SnapshotJson.WidgetValues(groupWidgets.GetValueOrDefault(item.Item.Id) ?? _noWidgets),
                LinkedSectionPublicId = item.Item.LinkedReferenceId,
            })
            .ToList();
        var sectionIds = sections.Select(section => section.Id).ToHashSet();

        var copyRows = allRows.Where(row => sectionIds.Contains(row.GroupId)).ToList();
        foreach (var row in copyRows.Where(row => !_design.HasRow(row.RowTemplateId)))
        {
            throw new MigrationException($"Row {row.Id} (copy {copy.Id}) uses row template {row.RowTemplateId}, which isn't a template row in the target database.");
        }

        var rows = Ordered(copyRows, row => row.GroupId, row => row.RowNumber, row => row.Id)
            .Select(item => new SnapshotRow
            {
                Id = item.Item.Id,
                PublicId = item.Item.ReferenceId,
                SectionId = item.Item.GroupId,
                TemplateRowId = item.Item.RowTemplateId,
                DisplayOrder = item.Order,
            })
            .ToList();
        var rowIds = rows.Select(row => row.Id).ToHashSet();

        var cells = new List<SnapshotCell>();
        foreach (var cell in allCells.Values.Where(cell => rowIds.Contains(cell.RowId)))
        {
            if (cell.TemplateCellId is not { } templateCellId)
            {
                throw new MigrationException($"Cell {cell.Id} (copy {copy.Id}) is at column {cell.ColumnNumber}, where its row template has no template cell.");
            }

            if (!_design.HasCell(templateCellId))
            {
                throw new MigrationException($"Cell {cell.Id} (copy {copy.Id}) is template cell {templateCellId}, which isn't in the target database.");
            }

            cells.Add(new SnapshotCell
            {
                Id = cell.Id,
                PublicId = cell.ReferenceId,
                RowId = cell.RowId,
                TemplateCellId = templateCellId,
                Content = contents.TryGetValue(cell.Id, out var content) ? content.Build() : CellContent.Empty,
            });
        }

        return new SheetSnapshot
        {
            Tables = tables,
            Sections = sections,
            Rows = rows,
            Cells = cells,
            Properties = properties,
            LinkedTables = Ordered(linkedTables?.Values.ToList() ?? [], _ => 0, link => link.Link.DisplayOrder, link => link.Id)
                .Select(item => item.Item.Link with { DisplayOrder = item.Order })
                .ToList(),
            ExternalReferences = copy.IsWorkingCopy ? null : references?.Values.ToList() ?? [],
        };
    }

    // Legacy groups aren't nested; their templates are. A group whose template has a parent goes in the nearest group
    // before it, in the same table, of that parent template, or the first one if none is before it.
    private Dictionary<int, int?> ParentGroups(List<LegacyGroupRow> groups, LegacyCopy copy)
    {
        var parents = new Dictionary<int, int?>();
        foreach (var group in groups)
        {
            if (_design.ParentOf(group.GroupTemplateId) is not { } parentTemplateId)
            {
                parents[group.Id] = null;
                continue;
            }

            var candidates = groups
                .Where(candidate => candidate.TableId == group.TableId && candidate.GroupTemplateId == parentTemplateId)
                .OrderBy(candidate => candidate.Order)
                .ThenBy(candidate => candidate.Id)
                .ToList();
            if (candidates.Count == 0)
            {
                throw new MigrationException($"Group {group.Id} (copy {copy.Id}) belongs in a group of template {parentTemplateId}, but its table has none.");
            }

            var before = candidates.LastOrDefault(candidate => (candidate.Order, candidate.Id).CompareTo((group.Order, group.Id)) < 0);
            parents[group.Id] = (before ?? candidates[0]).Id;
        }

        return parents;
    }

    // Among the items of one parent, by their legacy order and then by id, spaced OrderGap apart.
    private static List<(TItem Item, int Order)> Ordered<TItem, TParent>(
        List<TItem> items,
        Func<TItem, TParent> parent,
        Func<TItem, int> order,
        Func<TItem, int> id)
    {
        return items
            .GroupBy(parent)
            .SelectMany(siblings => siblings
                .OrderBy(order)
                .ThenBy(id)
                .Select((item, index) => (item, (index + 1) * OrderGap)))
            .ToList();
    }

    private async Task<Dictionary<int, CellContentParts>> ReadContentsAsync(LegacySheet sheet, List<string> warnings, CancellationToken cancellationToken)
    {
        var parts = new Dictionary<int, CellContentParts>();
        var repeated = 0;

        CellContentParts For(SqlDataReader reader)
        {
            return Get(parts, reader.Int("CellId"));
        }

        await ReadAllAsync(LegacySql.TextValues, sheet, reader =>
        {
            var cell = For(reader);
            repeated += cell.Text is null ? 0 : 1;
            cell.Text = Checked(reader.GetString("Value"), reader.Int("CellId"));
        }, cancellationToken);

        await ReadAllAsync(LegacySql.NumericValues, sheet, reader =>
        {
            var cell = For(reader);
            repeated += cell.Number is null ? 0 : 1;
            cell.Number = reader.GetDecimal("Value");
        }, cancellationToken);

        await ReadAllAsync(LegacySql.PairValues, sheet, reader =>
        {
            var cell = For(reader);
            repeated += cell.LabelledText.Count == 0 ? 0 : 1;
            cell.LabelledText.Clear();
            var cellId = reader.Int("CellId");
            if (reader.NullableString("X") is { } x)
            {
                cell.LabelledText.Add(new LabelledSlot { SlotKey = PairSlotX, Position = 0, Value = Checked(x, cellId) });
            }

            if (reader.NullableString("Y") is { } y)
            {
                cell.LabelledText.Add(new LabelledSlot { SlotKey = PairSlotY, Position = 1, Value = Checked(y, cellId) });
            }
        }, cancellationToken);

        await ReadAllAsync(LegacySql.LinkedValues, sheet, reader =>
        {
            var cell = For(reader);
            repeated += cell.Linked is null ? 0 : 1;
            cell.Linked = new LinkedSheetTarget
            {
                SheetTypeId = reader.NullableInt("SheetTypeId"),
                PhaseId = reader.NullableInt("PhaseId"),
                TablePublicId = reader.NullableGuid("TableReferenceId"),
                ItemPublicId = reader.NullableGuid("ItemReferenceId"),
            };
        }, cancellationToken);

        if (repeated > 0)
        {
            warnings.Add($"{repeated} value(s) were one of several of the same kind on one cell; the newest of each was kept.");
        }

        return parts;
    }

    private static string Checked(string value, int cellId)
    {
        return value.Length <= LongestText
            ? value
            : throw new MigrationException($"Cell {cellId} holds {value.Length.ToString(CultureInfo.InvariantCulture)} characters; the new layout holds at most {LongestText.ToString(CultureInfo.InvariantCulture)}.");
    }

    private async Task<Dictionary<int, Dictionary<int, string?>>> ReadWidgetsAsync(string sql, LegacySheet sheet, CancellationToken cancellationToken)
    {
        var widgets = new Dictionary<int, Dictionary<int, string?>>();
        await ReadAllAsync(sql, sheet, reader => Get(widgets, reader.Int("OwnerId"))[reader.Int("TemplateWidgetId")] = reader.NullableString("Value"), cancellationToken);
        return widgets;
    }

    private async Task<Dictionary<int, List<LegacyWorkflowStep>>> ReadWorkflowAsync(LegacySheet sheet, CancellationToken cancellationToken)
    {
        var actors = new Dictionary<int, List<LegacyWorkflowActor>>();
        await ReadAllAsync(LegacySql.WorkflowActors, sheet, reader => Add(actors, reader.Int("StepId"), new LegacyWorkflowActor
        {
            Id = reader.Int("Id"),
            UserId = reader.NullableInt("UserId"),
            Comment = reader.NullableString("Comment"),
            State = reader.Int("State"),
            ActionedAtUtc = reader.NullableDateTime("ActionedAtUtc"),
        }), cancellationToken);

        var steps = new Dictionary<int, List<LegacyWorkflowStep>>();
        await ReadAllAsync(LegacySql.WorkflowSteps, sheet, reader =>
        {
            var id = reader.Int("Id");
            Add(steps, reader.Int("CopyId"), new LegacyWorkflowStep
            {
                Id = id,
                StartedAtUtc = reader.Utc("StartedAtUtc"),
                State = reader.Int("State"),
                Actors = actors.GetValueOrDefault(id) ?? [],
            });
        }, cancellationToken);

        return steps;
    }

    private async Task ReadAllAsync(string sql, LegacySheet? sheet, Action<SqlDataReader> read, CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(sql, _connection);
        if (sheet is not null)
        {
            command.Parameters.Add(new SqlParameter("@phaseId", SqlDbType.Int) { Value = sheet.PhaseId });
            command.Parameters.Add(new SqlParameter("@sheetTypeId", SqlDbType.Int) { Value = sheet.SheetTypeId });
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            read(reader);
        }
    }

    private static TValue Get<TValue>(Dictionary<int, TValue> map, int key)
        where TValue : new()
    {
        if (!map.TryGetValue(key, out var value))
        {
            value = new TValue();
            map[key] = value;
        }

        return value;
    }

    private static void Add<TValue>(Dictionary<int, List<TValue>> map, int key, TValue value)
    {
        Get(map, key).Add(value);
    }

    private sealed record LegacyTableRow
    {
        public required int Id { get; init; }

        public required int CopyId { get; init; }

        public required int TableTemplateId { get; init; }

        public string? Title { get; init; }

        public required int DisplayOrder { get; init; }

        public required Guid ReferenceId { get; init; }

        public required bool HasConfigurationOverride { get; init; }
    }

    private sealed record LegacyGroupRow
    {
        public required int Id { get; init; }

        public required int TableId { get; init; }

        public required int GroupTemplateId { get; init; }

        public required int Order { get; init; }

        public required Guid ReferenceId { get; init; }

        public Guid? LinkedReferenceId { get; init; }
    }

    private sealed record LegacyRowRow
    {
        public required int Id { get; init; }

        public required int GroupId { get; init; }

        public required int RowTemplateId { get; init; }

        public required int RowNumber { get; init; }

        public required Guid ReferenceId { get; init; }
    }

    private sealed record LegacyCellRow
    {
        public required int Id { get; init; }

        public required int RowId { get; init; }

        public required int ColumnNumber { get; init; }

        public required Guid ReferenceId { get; init; }

        public int? TemplateCellId { get; init; }

        public required bool DiffersFromTemplate { get; init; }
    }
}
