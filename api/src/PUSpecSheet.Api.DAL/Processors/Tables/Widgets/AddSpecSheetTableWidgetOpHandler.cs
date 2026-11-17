using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables.Widgets;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Values;

namespace PUSpecSheet.Api.DAL.Processors.Tables.Widgets;

internal class AddSpecSheetTableWidgetOpHandler : OperationHandlerBase, IOperationHandler<AddSpecSheetTableWidgetOp>
{
    public AddSpecSheetTableWidgetOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(AddSpecSheetTableWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableId = ResolveId(operation.SpecSheetTableId, tempIdMap);

        var newSpecSheetTableWidget = new DbSpecSheetTableWidget
        {
            Id = 0,
            SpecSheetTableId = specSheetTableId,
            TableTemplateWidgetId = operation.TableTemplateWidgetId,
            Value = new DbTableWidgetValue
            {
                Id = 0,
                SpecSheetTableWidgetId = 0,
                Value = operation.Value
            }
        };

        await DbContext.SpecSheetTableWidgets.AddAsync(newSpecSheetTableWidget, ct);
    }

    public (int TempId, int RealId)? GetGeneratedId(AddSpecSheetTableWidgetOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(AddSpecSheetTableWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return IsTempId(operation.SpecSheetTableId) && !tempIdMap.ContainsKey(operation.SpecSheetTableId);
    }
}