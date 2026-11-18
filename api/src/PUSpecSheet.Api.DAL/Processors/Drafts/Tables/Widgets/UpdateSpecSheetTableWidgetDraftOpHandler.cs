using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables.Widgets;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Tables.Widgets;

internal class UpdateSpecSheetTableWidgetDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<UpdateSpecSheetTableWidgetOp>
{
    public UpdateSpecSheetTableWidgetDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        if (operation.WidgetValue is null || !operation.WidgetValue.HasChanged)
        {
            return;
        }

        var tableId = await Owners.TableOfWidgetAsync(operation.Id, ct);
        var draft = await Tables.GetShownAsync(tableId, ct);

        var value = (await Tables.LoadWidgetValuesAsync(draft, ct)).FirstOrDefault(v => v.SheetTableWidgetId == operation.Id)
            ?? throw new InvalidOperationException($"SheetTableWidget with Id {operation.Id} has no value in the draft of SheetTable with Id {tableId}.");
        value.Value = operation.WidgetValue.Value;
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