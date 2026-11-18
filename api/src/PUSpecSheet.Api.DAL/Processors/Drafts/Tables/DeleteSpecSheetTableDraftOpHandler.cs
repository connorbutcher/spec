using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Tables;

/// <summary>
/// Removes a table in the draft. Its groups and rows go with it: nothing under a removed table is read.
/// </summary>
internal class DeleteSpecSheetTableDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<DeleteSpecSheetTableOp>
{
    public DeleteSpecSheetTableDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(DeleteSpecSheetTableOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        await Locks.EnsureNothingHeldInTableAsync(operation.Id, ct);

        var draft = await Tables.GetAsync(operation.Id, ct);
        draft.IsDeleted = true;
    }

    public (int TempId, int RealId)? GetGeneratedId(DeleteSpecSheetTableOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(DeleteSpecSheetTableOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}