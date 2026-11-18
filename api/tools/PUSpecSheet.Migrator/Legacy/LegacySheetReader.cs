using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// Reads every copy of a sheet (its SpecSheets rows and the tables under them) into <see cref="SheetSnapshot"/>s,
/// through the app's own Db records. Deleted tables, groups, rows and cells are left out: the copy doesn't show them.
/// Items keep their ReferenceIDs, which a copy made from another keeps, and that is how an item is followed from one
/// copy to the next. Their orders are kept as the copy has them: a row's RowNumber is how the app matches it with the
/// row of the same number in its parent group, so it is data, not just a place.
/// </summary>
internal sealed class LegacySheetReader
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly LegacyCellValueReader _cellValues;
    private readonly LegacyWidgetReader _widgets;
    private readonly LegacyWorkflowReader _workflow;

    public LegacySheetReader(PUSpecSheetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
        _cellValues = new LegacyCellValueReader(dbContext);
        _widgets = new LegacyWidgetReader(dbContext);
        _workflow = new LegacyWorkflowReader(dbContext);
    }

    public async Task<IReadOnlyList<LegacySheet>> ListSheetsAsync(CancellationToken cancellationToken)
    {
        var copies = await _dbContext.SpecSheets.AsNoTracking()
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

        var (copies, tables, groups, rows, cells) = LegacySheetScope.Of(_dbContext, sheet);

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
                cell.RowNumber,
                cell.ColumnNumber,
                cell.ReferenceId,
                cell.TableCellTypeId,
                cell.CellConfigurationId,
                cell.ConfigurationOverride
            })
            .ToListAsync(cancellationToken);

        // A copied cell doesn't name its template cell: it is the one in its row's template at the same column.
        var templateCells = (await _dbContext.TableCellTemplates.AsNoTracking()
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
        var rowNumbers = rowRows.ToDictionary(row => row.Id, row => row.RowNumber);

        var contents = await _cellValues.ReadAsync(cells, warnings, cancellationToken);

        var templateCellOfCell = new Dictionary<int, int>();
        var differing = 0;
        var offRow = 0;
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

            if (cell.RowNumber != rowNumbers[cell.SpecSheetTableRowId])
            {
                offRow++;
            }

            if (cell.ConfigurationOverride is not null)
            {
                contents.GetOrNew(cell.Id).ConfigurationOverride = cell.ConfigurationOverride;
            }
        }

        if (offRow > 0)
        {
            warnings.Add($"{offRow} cell(s), across all copies, have a RowNumber other than their row's; the revisions give a cell its row's.");
        }

        if (differing > 0)
        {
            warnings.Add($"{differing} cell(s), across all copies, have a cell type or configuration other than their template cell's; the revisions take them from the template cell.");
        }

        var tableWidgets = await _widgets.ReadTableWidgetsAsync(tables, cancellationToken);
        var groupWidgets = await _widgets.ReadGroupWidgetsAsync(groups, cancellationToken);

        var notes = await ReadNotesAsync(tables, cancellationToken);
        var properties = await ReadPropertiesAsync(copies, cancellationToken);
        var references = await ReadExternalReferencesAsync(copies, cancellationToken);
        var linkedTables = await ReadLinkedTablesAsync(copies, cancellationToken);
        var workflow = await _workflow.ReadAsync(copies, cancellationToken);

        // Each table, group, row and cell belongs to one copy. Grouping them by it once, in the order they were read,
        // saves going through every copy's items for each copy. One whose parent wasn't read is under copy 0, which
        // no copy is, so it is left out as before.
        var copyOfTable = tableRows.ToDictionary(table => table.Id, table => table.SpecSheetId);
        var copyOfGroup = groupRows.ToDictionary(group => group.Id, group => copyOfTable.GetValueOrDefault(group.SpecSheetTableId));
        var copyOfRow = rowRows.ToDictionary(row => row.Id, row => copyOfGroup.GetValueOrDefault(row.SpecSheetTableGroupId));
        var tablesByCopy = tableRows.ToLookup(table => table.SpecSheetId);
        var groupsByCopy = groupRows.ToLookup(group => copyOfGroup[group.Id]);
        var rowsByCopy = rowRows.ToLookup(row => copyOfRow[row.Id]);
        var cellsByCopy = cellRows.ToLookup(cell => copyOfRow.GetValueOrDefault(cell.SpecSheetTableRowId));

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
            var copyTables = tablesByCopy[copy.Id]
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

            var copyGroups = groupsByCopy[copy.Id]
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

            var copyRowsOfCopy = rowsByCopy[copy.Id]
                .Select(row => new SnapshotRow
                {
                    Id = row.Id,
                    ReferenceId = row.ReferenceId,
                    GroupId = row.SpecSheetTableGroupId,
                    TableRowTemplateId = row.TableRowTemplateId,
                    DisplayOrder = row.RowNumber
                })
                .ToList();

            var copyCells = cellsByCopy[copy.Id]
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
                    Tables = copyTables,
                    Groups = copyGroups,
                    Rows = copyRowsOfCopy,
                    Cells = copyCells,
                    Properties = properties.GetValueOrDefault(copy.Id) ?? new Dictionary<int, string?>(),
                    LinkedTables = linkedTables.GetValueOrDefault(copy.Id)?.Values.ToList() ?? [],
                    ExternalReferences = isPublished ? references.GetValueOrDefault(copy.Id)?.Values.ToList() ?? [] : null
                },
                Workflow = workflow[copy.Id].ToList()
            });
        }

        return new LegacyCopies { Copies = built, Warnings = warnings };
    }

    // A table's notes, oldest first.
    private async Task<ILookup<int, SheetTableNote>> ReadNotesAsync(IQueryable<DbSpecSheetTable> tables, CancellationToken cancellationToken)
    {
        return (await _dbContext.SpecSheetTableNotes.AsNoTracking()
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
    }

    // Each copy's property values, by SpecSheetTypePropertyID; where a copy has more than one, the newest is kept.
    private async Task<Dictionary<int, Dictionary<int, string?>>> ReadPropertiesAsync(IQueryable<DbSpecSheet> copies, CancellationToken cancellationToken)
    {
        var properties = new Dictionary<int, Dictionary<int, string?>>();
        foreach (var property in await _dbContext.SpecSheetPropertyValues.AsNoTracking()
            .Where(property => copies.Any(copy => copy.Id == property.SpecSheetId))
            .OrderBy(property => property.Id)
            .Select(property => new { property.SpecSheetId, property.SpecSheetTypePropertyId, property.PropertyValue })
            .ToListAsync(cancellationToken))
        {
            properties.GetOrNew(property.SpecSheetId)[property.SpecSheetTypePropertyId] = property.PropertyValue;
        }

        return properties;
    }

    // Each copy's external references, by ReferenceID; where a copy has more than one, the newest is kept.
    private async Task<Dictionary<int, Dictionary<Guid, SnapshotExternalReference>>> ReadExternalReferencesAsync(
        IQueryable<DbSpecSheet> copies,
        CancellationToken cancellationToken)
    {
        var references = new Dictionary<int, Dictionary<Guid, SnapshotExternalReference>>();
        foreach (var reference in await _dbContext.SpecSheetExternalReferences.AsNoTracking()
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
            references.GetOrNew(reference.SpecSheetId)[reference.ReferenceId] = new SnapshotExternalReference
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

        return references;
    }

    // Each copy's linked tables, by the linked table's ReferenceID; where a copy has more than one, the newest is kept.
    private async Task<Dictionary<int, Dictionary<Guid, SnapshotLinkedTable>>> ReadLinkedTablesAsync(
        IQueryable<DbSpecSheet> copies,
        CancellationToken cancellationToken)
    {
        var linkedTables = new Dictionary<int, Dictionary<Guid, SnapshotLinkedTable>>();
        foreach (var link in await _dbContext.SpecSheetLinkedTables.AsNoTracking()
            .Where(link => copies.Any(copy => copy.Id == link.SpecSheetId))
            .OrderBy(link => link.Id)
            .Select(link => new { link.Id, link.SpecSheetId, link.TableReferenceId, link.DisplayOrder, link.LinkedByUserId, link.LinkedDateTime })
            .ToListAsync(cancellationToken))
        {
            linkedTables.GetOrNew(link.SpecSheetId)[link.TableReferenceId] = new SnapshotLinkedTable
            {
                TableReferenceId = link.TableReferenceId,
                DisplayOrder = link.DisplayOrder,
                LinkedByUserId = link.LinkedByUserId,
                LinkedDateTime = link.LinkedDateTime
            };
        }

        return linkedTables;
    }
}
