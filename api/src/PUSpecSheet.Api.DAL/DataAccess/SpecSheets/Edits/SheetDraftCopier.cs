using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.SpecSheets;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// Makes a sheet's drafts show what another spec sheet shows, as copying a sheet into a new copy did. Tables, groups, rows
/// and cells are matched by ReferenceID: one the sheet has is changed in its draft only where it differs, one it hasn't
/// is added, and one the other sheet hasn't is removed. Copying a sheet's own published version therefore changes
/// nothing.
/// </summary>
internal class SheetDraftCopier
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly int _sheetId;
    private readonly TableDrafts _tables;
    private readonly GroupDrafts _groups;
    private readonly RowDrafts _rows;
    private readonly RowDraftValues _values;
    private readonly SheetDraftItems _items;
    private readonly Dictionary<int, Dictionary<int, int>> _templateCellIds = [];

    public SheetDraftCopier(PUSpecSheetDbContext dbContext, User user, int sheetId)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        _dbContext = dbContext;
        _sheetId = sheetId;
        _tables = new TableDrafts(dbContext, user, sheetId);
        _groups = new GroupDrafts(dbContext, user, sheetId);
        _rows = new RowDrafts(dbContext, user, sheetId);
        _values = new RowDraftValues(dbContext);
        _items = new SheetDraftItems(dbContext, _tables, _groups);
    }

    /// <param name="target">The sheet as its drafts show it now, read from the revisions.</param>
    /// <param name="sourceTables">The tables of the spec sheet to copy.</param>
    public async Task CopyAsync(SpecSheet target, IEnumerable<SpecSheetTable> sourceTables, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(target, nameof(target));
        ArgumentNullException.ThrowIfNull(sourceTables, nameof(sourceTables));

        var targetTables = target.Tables.ToDictionary(table => table.ReferenceId);
        foreach (var sourceTable in sourceTables)
        {
            if (targetTables.Remove(sourceTable.ReferenceId, out var targetTable))
            {
                await CopyTableAsync(targetTable, sourceTable, ct);
            }
            else
            {
                await _dbContext.SheetTables.AddAsync(await NewTableAsync(sourceTable, ct), ct);
            }
        }

        foreach (var removed in targetTables.Values)
        {
            (await _tables.GetAsync(removed.Id, ct)).IsDeleted = true;
        }
    }

    private async Task CopyTableAsync(SpecSheetTable target, SpecSheetTable source, CancellationToken ct)
    {
        if (SheetCopySignatures.Of(target) != SheetCopySignatures.Of(source))
        {
            var draft = await _tables.GetShownAsync(target.Id, ct);
            draft.Title = source.Title;
            draft.DisplayOrder = source.DisplayOrder;
            draft.ConfigurationOverride = source.ConfigurationOverride;
            await _items.SetTableItemsAsync(draft, null, source, ct);
        }

        var targetLinks = LinkedReferenceIds(target);
        var targetGroups = target.Groups.ToDictionary(group => group.ReferenceId);
        var sourceLinks = LinkedReferenceIds(source);
        foreach (var sourceGroup in source.Groups)
        {
            if (targetGroups.Remove(sourceGroup.ReferenceId, out var targetGroup))
            {
                await CopyGroupAsync(targetGroup, targetLinks[targetGroup.Id], sourceGroup, sourceLinks[sourceGroup.Id], ct);
            }
            else
            {
                var newGroup = await NewGroupAsync(target.Id, sourceGroup, sourceLinks[sourceGroup.Id], ct);
                await _dbContext.SheetTableGroups.AddAsync(newGroup, ct);
            }
        }

        foreach (var removed in targetGroups.Values)
        {
            (await _groups.GetAsync(removed.Id, ct)).IsDeleted = true;
        }
    }

    private async Task CopyGroupAsync(SpecSheetTableGroup target, Guid? targetLink, SpecSheetTableGroup source, Guid? sourceLink, CancellationToken ct)
    {
        if (target.Order != source.Order || targetLink != sourceLink || SheetCopySignatures.Of(target) != SheetCopySignatures.Of(source))
        {
            var draft = await _groups.GetShownAsync(target.Id, ct);
            draft.DisplayOrder = source.Order;
            draft.LinkedSheetTableGroupReferenceId = sourceLink;
            await _items.SetGroupItemsAsync(draft, null, source, ct);
        }

        var targetRows = target.Rows.ToDictionary(row => row.ReferenceId);
        foreach (var sourceRow in source.Rows)
        {
            if (targetRows.Remove(sourceRow.ReferenceId, out var targetRow))
            {
                await CopyRowAsync(targetRow, sourceRow, ct);
            }
            else
            {
                await _dbContext.SheetTableRows.AddAsync(await NewRowAsync(target.Id, sourceRow, ct), ct);
            }
        }

        foreach (var removed in targetRows.Values)
        {
            (await _rows.GetAsync(removed.Id, ct)).IsDeleted = true;
        }
    }

    private async Task CopyRowAsync(SpecSheetTableRow target, SpecSheetTableRow source, CancellationToken ct)
    {
        if (SheetCopySignatures.Of(target) == SheetCopySignatures.Of(source))
        {
            return;
        }

        var draft = await _rows.GetShownAsync(target.Id, ct);
        draft.DisplayOrder = source.RowNumber;

        var targetCells = target.Cells.ToDictionary(cell => cell.ReferenceId);
        foreach (var sourceCell in source.Cells)
        {
            if (targetCells.Remove(sourceCell.ReferenceId, out var targetCell))
            {
                _values.ReplaceValue(draft, targetCell.Id, sourceCell.CellValue);
                _values.SetConfigurationOverride(draft, targetCell.Id, sourceCell.ConfigurationOverride);
            }
            else
            {
                var newCell = await NewCellAsync(target.Id, target.TableRowTemplateId, sourceCell, ct);
                await _dbContext.SheetTableCells.AddAsync(newCell, ct);
                AddCellContent(draft, newCell, sourceCell);
            }
        }

        foreach (var removed in targetCells.Values)
        {
            _values.Clear(draft, removed.Id);
        }
    }

    private async Task<DbSheetTable> NewTableAsync(SpecSheetTable source, CancellationToken ct)
    {
        var newTable = new DbSheetTable
        {
            Id = 0,
            SheetId = _sheetId,
            TableTemplateId = source.TableTemplateId,
            ReferenceId = source.ReferenceId,
            CreatedDateTime = DateTime.UtcNow
        };

        var draft = _tables.StartNew(newTable, source.Title, source.DisplayOrder, source.ConfigurationOverride);
        await _items.SetTableItemsAsync(draft, newTable, source, ct);

        var links = LinkedReferenceIds(source);
        foreach (var sourceGroup in source.Groups)
        {
            newTable.Groups.Add(await NewGroupAsync(0, sourceGroup, links[sourceGroup.Id], ct));
        }

        return newTable;
    }

    private async Task<DbSheetTableGroup> NewGroupAsync(int tableId, SpecSheetTableGroup source, Guid? linkedReferenceId, CancellationToken ct)
    {
        var newGroup = new DbSheetTableGroup
        {
            Id = 0,
            SheetTableId = tableId,
            TableGroupTemplateId = source.TableGroupTemplateId,
            ReferenceId = source.ReferenceId,
            CreatedDateTime = DateTime.UtcNow
        };

        var draft = _groups.StartNew(newGroup, source.Order, linkedReferenceId);
        await _items.SetGroupItemsAsync(draft, newGroup, source, ct);

        foreach (var sourceRow in source.Rows)
        {
            newGroup.Rows.Add(await NewRowAsync(0, sourceRow, ct));
        }

        return newGroup;
    }

    private async Task<DbSheetTableRow> NewRowAsync(int groupId, SpecSheetTableRow source, CancellationToken ct)
    {
        var newRow = new DbSheetTableRow
        {
            Id = 0,
            SheetTableGroupId = groupId,
            TableRowTemplateId = source.TableRowTemplateId,
            ReferenceId = source.ReferenceId,
            CreatedDateTime = DateTime.UtcNow
        };

        var draft = _rows.StartNew(newRow, source.RowNumber);
        foreach (var sourceCell in source.Cells)
        {
            var newCell = await NewCellAsync(0, source.TableRowTemplateId, sourceCell, ct);
            newRow.Cells.Add(newCell);
            AddCellContent(draft, newCell, sourceCell);
        }

        return newRow;
    }

    // A cell is kept as its row template's cell in the same column.
    private async Task<DbSheetTableCell> NewCellAsync(int rowId, int rowTemplateId, SpecSheetTableCell source, CancellationToken ct)
    {
        if (!_templateCellIds.TryGetValue(rowTemplateId, out var templateCellIds))
        {
            templateCellIds = await _dbContext.TableCellTemplates
                .Where(c => c.TableRowTemplateId == rowTemplateId)
                .ToDictionaryAsync(c => c.ColumnNumber, c => c.Id, ct);
            _templateCellIds[rowTemplateId] = templateCellIds;
        }

        if (!templateCellIds.TryGetValue(source.ColumnNumber, out var templateCellId))
        {
            throw new InvalidOperationException($"Row template {rowTemplateId} has no cell in column {source.ColumnNumber}, so the cell {source.ReferenceId} can't be copied.");
        }

        return new DbSheetTableCell
        {
            Id = 0,
            SheetTableRowId = rowId,
            TableCellTemplateId = templateCellId,
            ReferenceId = source.ReferenceId,
            CreatedDateTime = DateTime.UtcNow
        };
    }

    private void AddCellContent(DbSheetTableRowRevision draft, DbSheetTableCell newCell, SpecSheetTableCell source)
    {
        _values.AddValue(draft, newCell, source.CellValue);
        if (source.ConfigurationOverride is not null)
        {
            _values.AddConfigurationOverride(draft, newCell, source.ConfigurationOverride);
        }
    }

    // Each group's link, as the ReferenceID of the group it is linked to, which is how the revisions keep it.
    private static Dictionary<int, Guid?> LinkedReferenceIds(SpecSheetTable table)
    {
        var referenceIds = table.Groups.ToDictionary(group => group.Id, group => group.ReferenceId);
        return table.Groups.ToDictionary(
            group => group.Id,
            group => group.LinkedSpecSheetTableGroupId is { } linkedId && referenceIds.TryGetValue(linkedId, out var linked)
                ? linked
                : (Guid?)null);
    }
}