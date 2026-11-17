using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.TableGroups;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.SpecSheets;

namespace PUSpecSheet.Api.DAL.Processors.TableGroups;

internal class AddSpecSheetTableGroupOpHandler : OperationHandlerBase, IOperationHandler<AddSpecSheetTableGroupOp>
{
    private DbSpecSheetTableGroup? _addedEntity;

    public AddSpecSheetTableGroupOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(AddSpecSheetTableGroupOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableId = ResolveId(operation.SpecSheetTableId, tempIdMap);
        var linkedSpecSheetTableGroupId = operation.LinkedSpecSheetTableGroupId.HasValue
            ? ResolveId(operation.LinkedSpecSheetTableGroupId.Value, tempIdMap)
            : (int?)null;

        var newSpecSheetTableGroup = new DbSpecSheetTableGroup
        {
            Id = 0,
            SpecSheetTableId = specSheetTableId,
            ReferenceId = operation.ReferenceId,
            TableGroupTemplateId = operation.TableGroupTemplateId,
            LinkedSpecSheetTableGroupId = linkedSpecSheetTableGroupId,
            Order = operation.Order,
            CreatedDateTime = DateTime.UtcNow,
            CreatedByUserId = User.Id,
            IsDeleted = false,
        };

        await DbContext.SpecSheetTableGroups.AddAsync(newSpecSheetTableGroup, ct);
        _addedEntity = newSpecSheetTableGroup;
    }

    public (int TempId, int RealId)? GetGeneratedId(AddSpecSheetTableGroupOp operation)
    {
        return _addedEntity != null
            ? (operation.TempId, _addedEntity.Id)
            : null;
    }

    public bool HasUnresolvedDependencies(AddSpecSheetTableGroupOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return (IsTempId(operation.SpecSheetTableId) && !tempIdMap.ContainsKey(operation.SpecSheetTableId)) ||
               (operation.LinkedSpecSheetTableGroupId.HasValue && IsTempId(operation.LinkedSpecSheetTableGroupId.Value) && !tempIdMap.ContainsKey(operation.LinkedSpecSheetTableGroupId.Value));
    }
}