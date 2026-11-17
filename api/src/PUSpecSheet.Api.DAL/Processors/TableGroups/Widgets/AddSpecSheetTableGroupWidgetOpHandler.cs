using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.TableGroups.Widgets;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Values;

namespace PUSpecSheet.Api.DAL.Processors.TableGroups.Widgets;

internal class AddSpecSheetTableGroupWidgetOpHandler : OperationHandlerBase, IOperationHandler<AddSpecSheetTableGroupWidgetOp>
{
    public AddSpecSheetTableGroupWidgetOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(AddSpecSheetTableGroupWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableGroupId = ResolveId(operation.SpecSheetTableGroupId, tempIdMap);

        var newSpecSheetTableGroupWidget = new DbSpecSheetTableGroupWidget
        {
            Id = 0,
            SpecSheetTableGroupId = specSheetTableGroupId,
            TableGroupTemplateWidgetId = operation.TableGroupTemplateWidgetId,
            Value = new DbTableGroupWidgetValue
            {
                Id = 0,
                SpecSheetTableGroupWidgetId = 0,
                Value = operation.Value
            }
        };

        await DbContext.SpecSheetTableGroupWidgets.AddAsync(newSpecSheetTableGroupWidget, ct);
    }

    public (int TempId, int RealId)? GetGeneratedId(AddSpecSheetTableGroupWidgetOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(AddSpecSheetTableGroupWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return IsTempId(operation.SpecSheetTableGroupId) && !tempIdMap.ContainsKey(operation.SpecSheetTableGroupId);
    }
}