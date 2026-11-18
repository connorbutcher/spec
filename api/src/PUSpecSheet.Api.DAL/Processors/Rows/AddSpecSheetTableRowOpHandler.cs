using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Rows;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.DAL.Processors.Rows;

internal class AddSpecSheetTableRowOpHandler : OperationHandlerBase, IOperationHandler<AddSpecSheetTableRowOp>
{
    private DbSpecSheetTableRow? _addedEntity;

    public AddSpecSheetTableRowOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(AddSpecSheetTableRowOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableGroupId = ResolveId(operation.SpecSheetTableGroupId, tempIdMap);

        var newSpecSheetTableRow = new DbSpecSheetTableRow
        {
            Id = 0,
            SpecSheetTableGroupId = specSheetTableGroupId,
            TableRowTemplateId = operation.TableRowTemplateId,
            ReferenceId = operation.ReferenceId,
            RowNumber = operation.RowNumber,
            CreatedDateTime = DateTime.UtcNow,
            CreatedByUserId = User.Id,
            IsDeleted = false,
        };

        await DbContext.SpecSheetTableRows.AddAsync(newSpecSheetTableRow, ct);
        _addedEntity = newSpecSheetTableRow;
    }

    public (int TempId, int RealId)? GetGeneratedId(AddSpecSheetTableRowOp operation)
    {
        return _addedEntity != null
            ? (operation.TempId, _addedEntity.Id)
            : null;
    }

    public bool HasUnresolvedDependencies(AddSpecSheetTableRowOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return IsTempId(operation.SpecSheetTableGroupId) && !tempIdMap.ContainsKey(operation.SpecSheetTableGroupId);
    }
}