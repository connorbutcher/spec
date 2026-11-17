using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.TableGroups;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.TableGroups;

internal class UpdateSpecSheetTableGroupOpHandler : OperationHandlerBase, IOperationHandler<UpdateSpecSheetTableGroupOp>
{
    public UpdateSpecSheetTableGroupOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableGroupOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableGroup = await DbContext.SpecSheetTableGroups.FindAsync([operation.Id], ct)
            ?? throw new InvalidOperationException($"SpecSheetTableGroup with Id {operation.Id} not found.");

        if (operation.Order.HasValue)
        {
            specSheetTableGroup.Order = operation.Order.Value;
        }

        if (operation.LinkedSpecSheetTableGroupId is not null && operation.LinkedSpecSheetTableGroupId.HasChanged)
        {
            specSheetTableGroup.LinkedSpecSheetTableGroupId = operation.LinkedSpecSheetTableGroupId.Value;
        }

        specSheetTableGroup.LastModifiedDateTime = DateTime.UtcNow;
        specSheetTableGroup.LastModifiedByUserId = User.Id;
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateSpecSheetTableGroupOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateSpecSheetTableGroupOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}