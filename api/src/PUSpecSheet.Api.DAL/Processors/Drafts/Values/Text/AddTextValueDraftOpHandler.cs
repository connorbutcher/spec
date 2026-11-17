using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Values.Text;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Values.Text;

internal class AddTextValueDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<AddTextValueOp>
{
    public AddTextValueDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(AddTextValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var cellId = ResolveId(operation.SpecSheetTableCellId, tempIdMap);
        var draft = await GetRowDraftAsync(cellId, ct);

        Values.SetText(draft, cellId, operation.Value);
    }

    public (int TempId, int RealId)? GetGeneratedId(AddTextValueOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(AddTextValueOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return IsTempId(operation.SpecSheetTableCellId) && !tempIdMap.ContainsKey(operation.SpecSheetTableCellId);
    }
}