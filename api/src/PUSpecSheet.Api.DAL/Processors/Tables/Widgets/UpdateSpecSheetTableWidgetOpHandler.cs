using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables.Widgets;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Tables.Widgets;

internal class UpdateSpecSheetTableWidgetOpHandler : OperationHandlerBase, IOperationHandler<UpdateSpecSheetTableWidgetOp>
{
    public UpdateSpecSheetTableWidgetOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableWidget = await DbContext.TableWidgetValues.FindAsync([operation.Id], ct)
            ?? throw new InvalidOperationException($"SpecSheetTableWidget with Id {operation.Id} not found.");

        if (operation.WidgetValue is not null && operation.WidgetValue.HasChanged)
        {
            specSheetTableWidget.Value = operation.WidgetValue.Value;
        }
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateSpecSheetTableWidgetOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateSpecSheetTableWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}
