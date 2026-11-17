using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.SpecSheets;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// Reads every copy of a sheet (its SpecSheets rows and the tables under them) into <see cref="SheetSnapshot"/>s,
/// through the app's own Db records. Deleted tables, groups, rows and cells are left out: the copy doesn't show them.
/// Items keep their ReferenceIDs, which a copy made from another keeps, and that is how an item is followed from one
/// copy to the next.
/// </summary>
internal sealed class LegacySheetReader
{
    // Items are numbered by their place among their siblings, this far apart, as the app numbers new items.
    private const int OrderGap = 1024;

    private readonly PUSpecSheetDbContext _db;

    public LegacySheetReader(PUSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LegacySheet>> ListSheetsAsync(CancellationToken cancellationToken)
    {
        var copies = await _db.SpecSheets.AsNoTracking()
            .Select(copy => new { copy.SpecSheetPhaseId, copy.SpecSheetTypeId, copy.CreatedDateTime })
            .ToListAsync(cancellationToken);

        return copies
            .GroupBy(copy => (copy.SpecSheetPhaseId, copy.SpecSheetTypeId))
            .Select(sheet => new LegacySheet
            {
                PhaseId = sheet.Key.SpecSheetPhaseId,
                SheetTypeId = sheet.Key.SpecSheetTypeId,
                CreatedDateTime = sheet.Min(copy => copy.CreatedDateTime)
            })
            .OrderBy(sheet => sheet.PhaseId)
            .ThenBy(sheet => sheet.SheetTypeId)
            .ToList();
    }

    public async Task<LegacyCopies> LoadCopiesAsync(LegacySheet sheet, CancellationToken cancellationToken)
    {
        var warnings = new List<string>();

        // Each query narrows the one before, so every read below is of this sheet's copies only.
        var copies = _db.SpecSheets.AsNoTracking()
            .Where(copy => copy.SpecSheetPhaseId == sheet.PhaseId && copy.SpecSheetTypeId == sheet.SheetTypeId);
        var tables = _db.SpecSheetTables.AsNoTracking()
            .Where(table => !table.IsDeleted && copies.Any(copy => copy.Id == table.SpecSheetId));
        var groups = _db.SpecSheetTableGroups.AsNoTracking()
            .Where(group => !group.IsDeleted && tables.Any(table => table.Id == group.SpecSheetTableId));
        var rows = _db.SpecSheetTableRows.AsNoTracking()
            .Where(row => !row.IsDeleted && groups.Any(group => group.Id == row.SpecSheetTableGroupId));
        var cells = _db.SpecSheetTableCells.AsNoTracking()
            .Where(cell => !cell.IsDeleted && rows.Any(row => row.Id == cell.SpecSheetTableRowId));

        var copyRows = await copies
            .Select(copy => new
            {
                copy.Id,
                copy.IsPublished,
                copy.PublishedDateTime,
                copy.PublishedByUserId,
                copy.CreatedByUserId,
                copy.CreatedDateTime,
                copy.ChangeLog,
                copy.LegacySpecSheetId,
                copy.HasBeenMigrated
            })
            .ToListAsync(cancellationToken);

        var tableRows = await tables
            .Select(table => new
            {
                table.Id,
                table.SpecSheetId,
                table.TableTemplateId,
                table.Title,
                table.DisplayOrder,
                table.ReferenceId,
                table.ConfigurationOverride
            })
            .ToListAsync(cancellationToken);

        var groupRows = await groups
            .Select(group => new
            {
                group.Id,
                group.SpecSheetTableId,
                group.TableGroupTemplateId,
                group.Order,
                group.ReferenceId,
                LinkedReferenceId = group.LinkedSpecSheetTableGroup == null ? (Guid?)null : group.LinkedSpecSheetTableGroup.ReferenceId
            })
            .ToListAsync(cancellationToken);

        var rowRows = await rows
            .Select(row => new { row.Id, row.SpecSheetTableGroupId, row.TableRowTemplateId, row.RowNumber, row.ReferenceId })
            .ToListAsync(cancellationToken);

        var cellRows = await cells
            .Select(cell => new
            {
                cell.Id,
                cell.SpecSheetTableRowId,
                cell.ColumnNumber,
                cell.ReferenceId,
                cell.TableCellTypeId,
                cell.CellConfigurationId,
                cell.ConfigurationOverride
            })
            .ToListAsync(cancellationToken);

        // A copied cell doesn't name its template cell: it is the one in its row's template at the same column.
        var templateCells = (await _db.TableCellTemplates.AsNoTracking()
                .Where(templateCell => rows.Any(row => row.TableRowTemplateId == templateCell.TableRowTemplateId))
                .Select(templateCell => new
                {
                    templateCell.Id,
                    templateCell.TableRowTemplateId,
                    templateCell.ColumnNumber,
                    templateCell.TableCellTypeId,
                    templateCell.CellConfigurationId
                })
                .ToListAsync(cancellationToken))
            .ToLookup(templateCell => (templateCell.TableRowTemplateId, templateCell.ColumnNumber));
        var rowTemplates = rowRows.ToDictionary(row => row.Id, row => row.TableRowTemplateId);

        var contents = await ReadContentsAsync(cells, warnings, cancellationToken);

        var templateCellOfCell = new Dictionary<int, int>();
        var differing = 0;
        foreach (var cell in cellRows)
        {
            var candidates = templateCells[(rowTemplates[cell.SpecSheetTableRowId], cell.ColumnNumber)].ToList();
            if (candidates.Count == 0)
            {
                throw new MigrationException($"Cell {cell.Id} is at column {cell.ColumnNumber}, where its row template has no template cell.");
            }

            if (candidates.Count > 1)
            {
                throw new MigrationException($"Cell {cell.Id}'s row template has more than one template cell at column {cell.ColumnNumber}.");
            }

            var templateCell = candidates[0];
            templateCellOfCell[cell.Id] = templateCell.Id;
            if (cell.TableCellTypeId != templateCell.TableCellTypeId || cell.CellConfigurationId != templateCell.CellConfigurationId)
            {
                differing++;
            }

            if (cell.ConfigurationOverride is not null)
            {
                Get(contents, cell.Id).ConfigurationOverride = cell.ConfigurationOverride;
            }
        }

        if (differing > 0)
        {
            warnings.Add($"{differing} cell(s), across all copies, have a cell type or configuration other than their template cell's; the revisions take them from the template cell.");
        }

        var tableWidgets = await ReadTableWidgetsAsync(tables, cancellationToken);
        var groupWidgets = await ReadGroupWidgetsAsync(groups, cancellationToken);

        var notes = (await _db.SpecSheetTableNotes.AsNoTracking()
                .Where(note => tables.Any(table => table.Id == note.SpecSheetTableId))
                .OrderBy(note => note.CreatedDateTime)
                .ThenBy(note => note.Id)
                .Select(note => new { note.SpecSheetTableId, note.Note, note.SourceSystem, note.IsObsolete, note.CreatedByUserId, note.CreatedDateTime })
                .ToListAsync(cancellationToken))
            .ToLookup(
                note => note.SpecSheetTableId,
                note => new SheetTableNote
                {
                    Note = note.Note,
                    SourceSystem = note.SourceSystem,
                    IsObsolete = note.IsObsolete,
                    CreatedByUserId = note.CreatedByUserId,
                    CreatedDateTime = note.CreatedDateTime
                });

        var properties = new Dictionary<int, Dictionary<int, string?>>();
        foreach (var property in await _db.SpecSheetPropertyValues.AsNoTracking()
            .Where(property => copies.Any(copy => copy.Id == property.SpecSheetId))
            .OrderBy(property => property.Id)
            .Select(property => new { property.SpecSheetId, property.SpecSheetTypePropertyId, property.PropertyValue })
            .ToListAsync(cancellationToken))
        {
            Get(properties, property.SpecSheetId)[property.SpecSheetTypePropertyId] = property.PropertyValue;
        }

        var references = new Dictionary<int, Dictionary<Guid, SnapshotExternalReference>>();
        foreach (var reference in await _db.SpecSheetExternalReferences.AsNoTracking()
            .Where(reference => copies.Any(copy => copy.Id == reference.SpecSheetId))
            .OrderBy(reference => reference.Id)
            .Select(reference => new
            {
                reference.SpecSheetId,
                reference.ReferenceId,
                reference.ExternalId,
                reference.SystemName,
                reference.OwnerName,
                reference.Url,
                reference.Type,
                reference.Grouping,
                reference.SystemIconUrl,
                reference.Description
            })
            .ToListAsync(cancellationToken))
        {
            Get(references, reference.SpecSheetId)[reference.ReferenceId] = new SnapshotExternalReference
            {
                ReferenceId = reference.ReferenceId,
                ExternalId = reference.ExternalId,
                SystemName = reference.SystemName,
                OwnerName = reference.OwnerName,
                Url = reference.Url,
                Type = reference.Type,
                Grouping = reference.Grouping,
                SystemIconUrl = reference.SystemIconUrl,
                Description = reference.Description
            };
        }

        var linkedTables = new Dictionary<int, Dictionary<Guid, (int Id, SnapshotLinkedTable Link)>>();
        foreach (var link in await _db.SpecSheetLinkedTables.AsNoTracking()
            .Where(link => copies.Any(copy => copy.Id == link.SpecSheetId))
            .OrderBy(link => link.Id)
            .Select(link => new { link.Id, link.SpecSheetId, link.TableReferenceId, link.DisplayOrder, link.LinkedByUserId, link.LinkedDateTime })
            .ToListAsync(cancellationToken))
        {
            Get(linkedTables, link.SpecSheetId)[link.TableReferenceId] = (link.Id, new SnapshotLinkedTable
            {
                TableReferenceId = link.TableReferenceId,
                DisplayOrder = link.DisplayOrder,
                LinkedByUserId = link.LinkedByUserId,
                LinkedDateTime = link.LinkedDateTime
            });
        }

        var workflow = await ReadWorkflowAsync(copies, cancellationToken);

        var built = new List<LegacyCopy>();
        foreach (var copy in copyRows)
        {
            var publishedDateTime = copy.PublishedDateTime;
            if (copy.IsPublished && publishedDateTime is null)
            {
                warnings.Add($"Copy {copy.Id} is published but has no publish time; its creation time is used.");
                publishedDateTime = copy.CreatedDateTime;
            }

            var isPublished = copy.IsPublished;
            var copyTables = tableRows
                .Where(table => table.SpecSheetId == copy.Id)
                .Select(table => new SnapshotTable
                {
                    Id = table.Id,
                    ReferenceId = table.ReferenceId,
                    TableTemplateId = table.TableTemplateId,
                    Title = table.Title,
                    DisplayOrder = table.DisplayOrder,
                    ConfigurationOverride = table.ConfigurationOverride,
                    WidgetValues = tableWidgets.GetValueOrDefault(table.Id) ?? [],
                    Notes = notes[table.Id].ToList()
                })
                .ToList();
            var tableIds = copyTables.Select(table => table.Id).ToHashSet();

            var copyGroups = groupRows
                .Where(group => tableIds.Contains(group.SpecSheetTableId))
                .Select(group => new SnapshotGroup
                {
                    Id = group.Id,
                    ReferenceId = group.ReferenceId,
                    TableId = group.SpecSheetTableId,
                    TableGroupTemplateId = group.TableGroupTemplateId,
                    DisplayOrder = group.Order,
                    WidgetValues = groupWidgets.GetValueOrDefault(group.Id) ?? [],
                    LinkedGroupReferenceId = group.LinkedReferenceId
                })
                .ToList();
            var groupIds = copyGroups.Select(group => group.Id).ToHashSet();

            var copyRowsOfCopy = rowRows
                .Where(row => groupIds.Contains(row.SpecSheetTableGroupId))
                .Select(row => new SnapshotRow
                {
                    Id = row.Id,
                    ReferenceId = row.ReferenceId,
                    GroupId = row.SpecSheetTableGroupId,
                    TableRowTemplateId = row.TableRowTemplateId,
                    DisplayOrder = row.RowNumber
                })
                .ToList();
            var rowIds = copyRowsOfCopy.Select(row => row.Id).ToHashSet();

            var copyCells = cellRows
                .Where(cell => rowIds.Contains(cell.SpecSheetTableRowId))
                .Select(cell => new SnapshotCell
                {
                    Id = cell.Id,
                    ReferenceId = cell.ReferenceId,
                    RowId = cell.SpecSheetTableRowId,
                    TableCellTemplateId = templateCellOfCell[cell.Id],
                    Content = contents.TryGetValue(cell.Id, out var content) ? content.Build() : CellContent.Empty
                })
                .ToList();

            built.Add(new LegacyCopy
            {
                Id = copy.Id,
                PublishedDateTime = isPublished ? publishedDateTime : null,
                PublishedByUserId = isPublished ? copy.PublishedByUserId : null,
                CreatedByUserId = copy.CreatedByUserId,
                CreatedDateTime = copy.CreatedDateTime,
                ChangeLog = copy.ChangeLog,
                LegacySpecSheetId = copy.LegacySpecSheetId,
                HasBeenMigrated = copy.HasBeenMigrated,
                Snapshot = new SheetSnapshot
                {
                    // Orders become places among siblings, so the copies' own numbering doesn't matter.
                    Tables = Ordered(copyTables, _ => 0, table => table.DisplayOrder, table => table.Id)
                        .Select(item => item.Item with { DisplayOrder = item.Order })
                        .ToList(),
                    Groups = Ordered(copyGroups, group => group.TableId, group => group.DisplayOrder, group => group.Id)
                        .Select(item => item.Item with { DisplayOrder = item.Order })
                        .ToList(),
                    Rows = Ordered(copyRowsOfCopy, row => row.GroupId, row => row.DisplayOrder, row => row.Id)
                        .Select(item => item.Item with { DisplayOrder = item.Order })
                        .ToList(),
                    Cells = copyCells,
                    Properties = properties.GetValueOrDefault(copy.Id) ?? new Dictionary<int, string?>(),
                    LinkedTables = Ordered(linkedTables.GetValueOrDefault(copy.Id)?.Values.ToList() ?? [], _ => 0, link => link.Link.DisplayOrder, link => link.Id)
                        .Select(item => item.Item.Link with { DisplayOrder = item.Order })
                        .ToList(),
                    ExternalReferences = isPublished ? references.GetValueOrDefault(copy.Id)?.Values.ToList() ?? [] : null
                },
                Workflow = workflow[copy.Id].ToList()
            });
        }

        return new LegacyCopies { Copies = built, Warnings = warnings };
    }

    // Among the items of one parent, by their copy's order and then by id, spaced OrderGap apart.
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

    // A cell has at most one value of each kind; where a copy has more, the newest (highest id) is kept.
    private async Task<Dictionary<int, CellContentParts>> ReadContentsAsync(
        IQueryable<DbSpecSheetTableCell> cells,
        List<string> warnings,
        CancellationToken cancellationToken)
    {
        var parts = new Dictionary<int, CellContentParts>();
        var repeated = 0;

        foreach (var value in await _db.TextValues.AsNoTracking()
            .Where(value => value.Value != null && cells.Any(cell => cell.Id == value.SpecSheetTableCellId))
            .OrderBy(value => value.Id)
            .Select(value => new { value.SpecSheetTableCellId, value.Value })
            .ToListAsync(cancellationToken))
        {
            var cell = Get(parts, value.SpecSheetTableCellId);
            repeated += cell.Text is null ? 0 : 1;
            cell.Text = value.Value;
        }

        foreach (var value in await _db.NumericValues.AsNoTracking()
            .Where(value => value.Value != null && cells.Any(cell => cell.Id == value.SpecSheetTableCellId))
            .OrderBy(value => value.Id)
            .Select(value => new { value.SpecSheetTableCellId, value.Value })
            .ToListAsync(cancellationToken))
        {
            var cell = Get(parts, value.SpecSheetTableCellId);
            repeated += cell.Number is null ? 0 : 1;
            cell.Number = value.Value;
        }

        foreach (var value in await _db.PairValues.AsNoTracking()
            .Where(value => (value.PairValueX != null || value.PairValueY != null) && cells.Any(cell => cell.Id == value.SpecSheetTableCellId))
            .OrderBy(value => value.Id)
            .Select(value => new { value.SpecSheetTableCellId, value.PairValueX, value.PairValueY })
            .ToListAsync(cancellationToken))
        {
            var cell = Get(parts, value.SpecSheetTableCellId);
            repeated += cell.PairX is null && cell.PairY is null ? 0 : 1;
            cell.PairX = value.PairValueX;
            cell.PairY = value.PairValueY;
        }

        foreach (var value in await _db.LinkedSpecSheetValues.AsNoTracking()
            .Where(value => cells.Any(cell => cell.Id == value.SpecSheetTableCellId))
            .OrderBy(value => value.Id)
            .Select(value => new
            {
                value.SpecSheetTableCellId,
                value.LinkedSpecSheetTypeId,
                value.LinkedSpecSheetPhaseId,
                value.TableReferenceId,
                value.LinkedReferenceId
            })
            .ToListAsync(cancellationToken))
        {
            var cell = Get(parts, value.SpecSheetTableCellId);
            repeated += cell.Linked is null ? 0 : 1;
            cell.Linked = new LinkedSheetTarget
            {
                SheetTypeId = value.LinkedSpecSheetTypeId,
                PhaseId = value.LinkedSpecSheetPhaseId,
                TableReferenceId = value.TableReferenceId,
                ItemReferenceId = value.LinkedReferenceId
            };
        }

        if (repeated > 0)
        {
            warnings.Add($"{repeated} value(s) were one of several of the same kind on one cell; the newest of each was kept.");
        }

        return parts;
    }

    // A table's widget values, one per template widget; where a copy has more than one, the newest is kept.
    private async Task<Dictionary<int, IReadOnlyList<SheetWidgetValue>>> ReadTableWidgetsAsync(
        IQueryable<DbSpecSheetTable> tables,
        CancellationToken cancellationToken)
    {
        var widgets = _db.SpecSheetTableWidgets.AsNoTracking()
            .Where(widget => tables.Any(table => table.Id == widget.SpecSheetTableId));
        var owners = await widgets
            .OrderBy(widget => widget.Id)
            .Select(widget => new { widget.Id, OwnerId = widget.SpecSheetTableId, TemplateWidgetId = widget.TableTemplateWidgetId })
            .ToListAsync(cancellationToken);
        var values = await _db.TableWidgetValues.AsNoTracking()
            .Where(value => widgets.Any(widget => widget.Id == value.SpecSheetTableWidgetId))
            .OrderBy(value => value.Id)
            .Select(value => new { WidgetId = value.SpecSheetTableWidgetId, value.Value })
            .ToListAsync(cancellationToken);

        return Widgets(
            owners.Select(owner => (owner.Id, owner.OwnerId, owner.TemplateWidgetId)),
            values.Select(value => (value.WidgetId, value.Value)));
    }

    private async Task<Dictionary<int, IReadOnlyList<SheetWidgetValue>>> ReadGroupWidgetsAsync(
        IQueryable<DbSpecSheetTableGroup> groups,
        CancellationToken cancellationToken)
    {
        var widgets = _db.SpecSheetTableGroupWidgets.AsNoTracking()
            .Where(widget => groups.Any(group => group.Id == widget.SpecSheetTableGroupId));
        var owners = await widgets
            .OrderBy(widget => widget.Id)
            .Select(widget => new { widget.Id, OwnerId = widget.SpecSheetTableGroupId, TemplateWidgetId = widget.TableGroupTemplateWidgetId })
            .ToListAsync(cancellationToken);
        var values = await _db.TableGroupWidgetValues.AsNoTracking()
            .Where(value => widgets.Any(widget => widget.Id == value.SpecSheetTableGroupWidgetId))
            .OrderBy(value => value.Id)
            .Select(value => new { WidgetId = value.SpecSheetTableGroupWidgetId, value.Value })
            .ToListAsync(cancellationToken);

        return Widgets(
            owners.Select(owner => (owner.Id, owner.OwnerId, owner.TemplateWidgetId)),
            values.Select(value => (value.WidgetId, value.Value)));
    }

    private static Dictionary<int, IReadOnlyList<SheetWidgetValue>> Widgets(
        IEnumerable<(int Id, int OwnerId, int TemplateWidgetId)> widgets,
        IEnumerable<(int WidgetId, WidgetValue? Value)> values)
    {
        var valueOf = new Dictionary<int, WidgetValue?>();
        foreach (var (widgetId, value) in values)
        {
            valueOf[widgetId] = value;
        }

        var byOwner = new Dictionary<int, Dictionary<int, WidgetValue?>>();
        foreach (var widget in widgets)
        {
            Get(byOwner, widget.OwnerId)[widget.TemplateWidgetId] = valueOf.GetValueOrDefault(widget.Id);
        }

        return byOwner.ToDictionary(
            owner => owner.Key,
            owner => (IReadOnlyList<SheetWidgetValue>)owner.Value
                .OrderBy(widget => widget.Key)
                .Select(widget => new SheetWidgetValue { TemplateWidgetId = widget.Key, Value = widget.Value })
                .ToList());
    }

    private async Task<ILookup<int, LegacyWorkflowStep>> ReadWorkflowAsync(
        IQueryable<DbSpecSheet> copies,
        CancellationToken cancellationToken)
    {
        var steps = _db.WorkflowSteps.AsNoTracking()
            .Where(step => copies.Any(copy => copy.Id == step.SpecSheetId));

        var actors = (await _db.WorkflowStepActors.AsNoTracking()
                .Where(actor => steps.Any(step => step.Id == actor.WorkflowStepId))
                .OrderBy(actor => actor.Id)
                .Select(actor => new { actor.Id, actor.WorkflowStepId, actor.ActorUserId, actor.Comment, actor.State, actor.ActionedDateTime })
                .ToListAsync(cancellationToken))
            .ToLookup(
                actor => actor.WorkflowStepId,
                actor => new LegacyWorkflowActor
                {
                    Id = actor.Id,
                    UserId = actor.ActorUserId,
                    Comment = actor.Comment,
                    State = (int)actor.State,
                    ActionedDateTime = actor.ActionedDateTime
                });

        return (await steps
                .OrderBy(step => step.StartedDateTime)
                .ThenBy(step => step.Id)
                .Select(step => new { step.Id, step.SpecSheetId, step.StartedDateTime, step.State })
                .ToListAsync(cancellationToken))
            .ToLookup(
                step => step.SpecSheetId,
                step => new LegacyWorkflowStep
                {
                    Id = step.Id,
                    StartedDateTime = step.StartedDateTime,
                    State = (int)step.State,
                    Actors = actors[step.Id].ToList()
                });
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
}
