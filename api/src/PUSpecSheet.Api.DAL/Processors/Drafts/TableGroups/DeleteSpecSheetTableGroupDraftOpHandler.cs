using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.TableGroups;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.TableGroups;

/// <summary>
/// Removes a group in the draft, with the groups linked to it. Its rows go with it: nothing under a removed group is read.
/// </summary>
internal class DeleteSpecSheetTableGroupDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<DeleteSpecSheetTableGroupOp>
{
    public DeleteSpecSheetTableGroupDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(DeleteSpecSheetTableGroupOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var draft = await Groups.GetAsync(operation.Id, ct);
        if (draft.IsDeleted)
        {
            return;
        }

        draft.IsDeleted = true;

        var referenceId = await Groups.FindReferenceIdAsync(operation.Id, ct);
        foreach (var linkedGroupId in await FindLinkedGroupIdsAsync(referenceId, ct))
        {
            var linkedDraft = await Groups.GetAsync(linkedGroupId, ct);
            if (linkedDraft.LinkedSheetTableGroupReferenceId == referenceId)
            {
                await ApplyAsync(new DeleteSpecSheetTableGroupOp { Id = linkedGroupId }, tempIdMap, ct);
            }
        }
    }

    public (int TempId, int RealId)? GetGeneratedId(DeleteSpecSheetTableGroupOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(DeleteSpecSheetTableGroupOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }

    private async Task<List<int>> FindLinkedGroupIdsAsync(Guid referenceId, CancellationToken ct)
    {
        var linkedGroupIds = await DbContext.SheetTableGroupRevisions
            .Where(r => r.SheetId == SheetId
                && (r.Status == RevisionStatus.Draft || r.SupersededDateTime == null)
                && !r.IsDeleted
                && r.LinkedSheetTableGroupReferenceId == referenceId)
            .Select(r => r.SheetTableGroupId)
            .Distinct()
            .ToListAsync(ct);

        var linkedInThisChange = DbContext.SheetTableGroupRevisions.Local
            .Where(r => r.Status == RevisionStatus.Draft && !r.IsDeleted && r.LinkedSheetTableGroupReferenceId == referenceId)
            .Select(r => r.SheetTableGroupId);

        return [.. linkedGroupIds.Union(linkedInThisChange)];
    }
}