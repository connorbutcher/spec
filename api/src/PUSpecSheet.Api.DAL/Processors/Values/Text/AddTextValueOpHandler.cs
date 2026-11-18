using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Values.Text;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Legacy.Values;

namespace PUSpecSheet.Api.DAL.Processors.Values.Text;

internal class AddTextValueOpHandler : OperationHandlerBase, IOperationHandler<AddTextValueOp>
{
    public AddTextValueOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(AddTextValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableCellId = ResolveId(operation.SpecSheetTableCellId, tempIdMap);

        var newTextValue = new DbTextValue
        {
            Id = 0,
            SpecSheetTableCellId = specSheetTableCellId,
            Value = operation.Value,
        };

        await DbContext.TextValues.AddAsync(newTextValue, ct);
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
