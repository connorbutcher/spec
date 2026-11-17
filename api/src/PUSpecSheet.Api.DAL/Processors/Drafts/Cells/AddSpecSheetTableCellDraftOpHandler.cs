using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Cells;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Cells;

/// <summary>
/// Adds a cell to a row. The revisions keep a cell as one of its row template's cells, found by its column; its type,
/// configuration and place come from that template cell.
/// </summary>
internal class AddSpecSheetTableCellDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<AddSpecSheetTableCellOp>
{
    private DbSheetTableCell? _addedEntity;

    public AddSpecSheetTableCellDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(AddSpecSheetTableCellOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var rowId = ResolveId(operation.SpecSheetTableRowId, tempIdMap);
        var draft = await Rows.GetShownAsync(rowId, ct);

        var templateCellId = await DbContext.SheetTableRows
            .Where(r => r.Id == rowId)
            .SelectMany(r => r.TableRowTemplate!.CellTemplates)
            .Where(c => c.ColumnNumber == operation.ColumnNumber)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException($"SheetTableRow with Id {rowId} has no template cell in column {operation.ColumnNumber}.");

        var newCell = new DbSheetTableCell
        {
            Id = 0,
            SheetTableRowId = rowId,
            TableCellTemplateId = templateCellId,
            ReferenceId = operation.ReferenceId,
            CreatedDateTime = DateTime.UtcNow
        };

        await DbContext.SheetTableCells.AddAsync(newCell, ct);
        if (operation.ConfigurationOverride is not null)
        {
            Values.AddConfigurationOverride(draft, newCell, operation.ConfigurationOverride);
        }

        _addedEntity = newCell;
    }

    public (int TempId, int RealId)? GetGeneratedId(AddSpecSheetTableCellOp operation)
    {
        return _addedEntity != null
            ? (operation.TempId, _addedEntity.Id)
            : null;
    }

    public bool HasUnresolvedDependencies(AddSpecSheetTableCellOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return IsTempId(operation.SpecSheetTableRowId) && !tempIdMap.ContainsKey(operation.SpecSheetTableRowId);
    }
}