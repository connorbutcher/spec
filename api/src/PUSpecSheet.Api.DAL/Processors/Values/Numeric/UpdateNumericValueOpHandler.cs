using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Values.Number;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Values.Numeric;

internal class UpdateNumericValueOpHandler : OperationHandlerBase, IOperationHandler<UpdateNumberValueOp>
{
    public UpdateNumericValueOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(UpdateNumberValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var numberValue = await DbContext.NumericValues.FindAsync([operation.Id], ct)
            ?? throw new InvalidOperationException($"NumericValue with Id {operation.Id} not found.");

        if (operation.Value is not null && operation.Value.HasChanged)
        {
            numberValue.Value = operation.Value.Value;
        }
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateNumberValueOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateNumberValueOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}
