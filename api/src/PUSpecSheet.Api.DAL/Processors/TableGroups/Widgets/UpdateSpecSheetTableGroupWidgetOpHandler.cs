using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.TableGroups.Widgets;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.TableGroups.Widgets;

internal class UpdateSpecSheetTableGroupWidgetOpHandler : OperationHandlerBase, IOperationHandler<UpdateSpecSheetTableGroupWidgetOp>
{
    public UpdateSpecSheetTableGroupWidgetOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableGroupWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableGroupWidget = await DbContext.TableGroupWidgetValues
            .FirstOrDefaultAsync(v => v.SpecSheetTableGroupWidgetId == operation.Id)
            ?? throw new InvalidOperationException($"SpecSheetTableGroupWidget with Id {operation.Id} not found.");

        if (operation.WidgetValue is not null && operation.WidgetValue.HasChanged)
        {
            specSheetTableGroupWidget.Value = operation.WidgetValue.Value;
        }
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateSpecSheetTableGroupWidgetOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateSpecSheetTableGroupWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}
