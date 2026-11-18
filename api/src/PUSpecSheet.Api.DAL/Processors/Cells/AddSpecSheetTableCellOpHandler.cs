using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Cells;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.DAL.Processors.Cells;

internal class AddSpecSheetTableCellOpHandler : OperationHandlerBase, IOperationHandler<AddSpecSheetTableCellOp>
{
    private DbSpecSheetTableCell? _addedEntity;

    public AddSpecSheetTableCellOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(AddSpecSheetTableCellOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableRowId = ResolveId(operation.SpecSheetTableRowId, tempIdMap);

        var newSpecSheetTableCell = new DbSpecSheetTableCell
        {
            Id = 0,
            SpecSheetTableRowId = specSheetTableRowId,
            TableCellTypeId = operation.TableCellTypeId,
            CellConfigurationId = operation.CellConfigurationId,
            ReferenceId = operation.ReferenceId,
            RowNumber = operation.RowNumber,
            ColumnNumber = operation.ColumnNumber,
            RowSpan = operation.RowSpan,
            ColSpan = operation.ColSpan,
            ConfigurationOverride = operation.ConfigurationOverride,
            CreatedDateTime = DateTime.UtcNow,
            CreatedByUserId = User.Id,
            IsDeleted = false,
        };

        await DbContext.SpecSheetTableCells.AddAsync(newSpecSheetTableCell, ct);
        _addedEntity = newSpecSheetTableCell;
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