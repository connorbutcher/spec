using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Rows;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Rows;

internal class AddSpecSheetTableRowDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<AddSpecSheetTableRowOp>
{
    private DbSheetTableRow? _addedEntity;

    public AddSpecSheetTableRowDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(AddSpecSheetTableRowOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var groupId = ResolveId(operation.SpecSheetTableGroupId, tempIdMap);
        await EnsureGroupOnSheetAsync(groupId, ct);

        var newRow = new DbSheetTableRow
        {
            Id = 0,
            SheetTableGroupId = groupId,
            TableRowTemplateId = operation.TableRowTemplateId,
            ReferenceId = operation.ReferenceId,
            CreatedDateTime = DateTime.UtcNow
        };

        Rows.StartNew(newRow, operation.RowNumber);
        await DbContext.SheetTableRows.AddAsync(newRow, ct);
        _addedEntity = newRow;
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