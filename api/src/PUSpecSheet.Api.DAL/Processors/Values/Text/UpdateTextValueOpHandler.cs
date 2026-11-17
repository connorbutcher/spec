using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Values.Text;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Values.Text;

internal class UpdateTextValueOpHandler : OperationHandlerBase, IOperationHandler<UpdateTextValueOp>
{
    public UpdateTextValueOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(UpdateTextValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var textValue = await DbContext.TextValues.FindAsync([operation.Id], ct)
            ?? throw new InvalidOperationException($"TextValue with Id {operation.Id} not found.");

        if (operation.Value is not null && operation.Value.HasChanged)
        {
            textValue.Value = operation.Value.Value;
        }
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateTextValueOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateTextValueOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}
