using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Tables;

internal class UpdateSpecSheetTableOpHandler : OperationHandlerBase, IOperationHandler<UpdateSpecSheetTableOp>
{
    public UpdateSpecSheetTableOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTable = await DbContext.SpecSheetTables.FindAsync([operation.Id], ct)
            ?? throw new InvalidOperationException($"SpecSheetTable with Id {operation.Id} not found.");

        if (!string.IsNullOrWhiteSpace(operation.Title))
        {
            specSheetTable.Title = operation.Title;
        }

        if (operation.DisplayOrder.HasValue)
        {
            specSheetTable.DisplayOrder = operation.DisplayOrder.Value;
        }

        if (operation.ConfigurationOverride is not null && operation.ConfigurationOverride.HasChanged)
        {
            specSheetTable.ConfigurationOverride = operation.ConfigurationOverride.Value;
        }

        specSheetTable.LastModifiedDateTime = DateTime.UtcNow;
        specSheetTable.LastModifiedByUserId = User.Id;
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateSpecSheetTableOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateSpecSheetTableOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}