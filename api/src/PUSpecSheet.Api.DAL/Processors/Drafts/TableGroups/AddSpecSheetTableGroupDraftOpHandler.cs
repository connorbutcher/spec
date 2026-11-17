using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.TableGroups;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.TableGroups;

/// <summary>
/// Adds a group to a table. The revisions keep the group it is linked to by that group's ReferenceID.
/// </summary>
internal class AddSpecSheetTableGroupDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<AddSpecSheetTableGroupOp>
{
    private DbSheetTableGroup? _addedEntity;

    public AddSpecSheetTableGroupDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(AddSpecSheetTableGroupOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var tableId = ResolveId(operation.SpecSheetTableId, tempIdMap);
        await EnsureTableOnSheetAsync(tableId, ct);

        Guid? linkedGroupReferenceId = operation.LinkedSpecSheetTableGroupId.HasValue
            ? await Groups.FindReferenceIdAsync(ResolveId(operation.LinkedSpecSheetTableGroupId.Value, tempIdMap), ct)
            : null;

        var newGroup = new DbSheetTableGroup
        {
            Id = 0,
            SheetTableId = tableId,
            TableGroupTemplateId = operation.TableGroupTemplateId,
            ReferenceId = operation.ReferenceId,
            CreatedDateTime = DateTime.UtcNow
        };

        Groups.StartNew(newGroup, operation.Order, linkedGroupReferenceId);
        await DbContext.SheetTableGroups.AddAsync(newGroup, ct);
        _addedEntity = newGroup;
    }

    public (int TempId, int RealId)? GetGeneratedId(AddSpecSheetTableGroupOp operation)
    {
        return _addedEntity != null
            ? (operation.TempId, _addedEntity.Id)
            : null;
    }

    public bool HasUnresolvedDependencies(AddSpecSheetTableGroupOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        var hasUnresolvedTable = IsTempId(operation.SpecSheetTableId) && !tempIdMap.ContainsKey(operation.SpecSheetTableId);
        var hasUnresolvedLinkedGroup = operation.LinkedSpecSheetTableGroupId.HasValue
            && IsTempId(operation.LinkedSpecSheetTableGroupId.Value)
            && !tempIdMap.ContainsKey(operation.LinkedSpecSheetTableGroupId.Value);

        return hasUnresolvedTable || hasUnresolvedLinkedGroup;
    }
}