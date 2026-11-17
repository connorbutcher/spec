using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Values.Number;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Values;

namespace PUSpecSheet.Api.DAL.Processors.Values.Numeric;

internal class AddNumericValueOpHandler : OperationHandlerBase, IOperationHandler<AddNumberValueOp>
{
    public AddNumericValueOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(AddNumberValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableCellId = ResolveId(operation.SpecSheetTableCellId, tempIdMap);

        var newNumericValue = new DbNumericValue
        {
            Id = 0,
            SpecSheetTableCellId = specSheetTableCellId,
            Value = operation.Value,
        };

        await DbContext.NumericValues.AddAsync(newNumericValue, ct);
    }

    public (int TempId, int RealId)? GetGeneratedId(AddNumberValueOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(AddNumberValueOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return IsTempId(operation.SpecSheetTableCellId) && !tempIdMap.ContainsKey(operation.SpecSheetTableCellId);
    }
}