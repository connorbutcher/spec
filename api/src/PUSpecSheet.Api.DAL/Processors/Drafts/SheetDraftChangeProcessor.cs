using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations;
using PUSpecSheet.Abstractions.Operations.Cells;
using PUSpecSheet.Abstractions.Operations.Rows;
using PUSpecSheet.Abstractions.Operations.TableGroups;
using PUSpecSheet.Abstractions.Operations.TableGroups.Widgets;
using PUSpecSheet.Abstractions.Operations.Tables;
using PUSpecSheet.Abstractions.Operations.Tables.Notes;
using PUSpecSheet.Abstractions.Operations.Tables.Widgets;
using PUSpecSheet.Abstractions.Operations.Values.Linked;
using PUSpecSheet.Abstractions.Operations.Values.Number;
using PUSpecSheet.Abstractions.Operations.Values.Pair;
using PUSpecSheet.Abstractions.Operations.Values.Text;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Cells;
using PUSpecSheet.Api.DAL.Processors.Drafts.Rows;
using PUSpecSheet.Api.DAL.Processors.Drafts.TableGroups;
using PUSpecSheet.Api.DAL.Processors.Drafts.TableGroups.Widgets;
using PUSpecSheet.Api.DAL.Processors.Drafts.Tables;
using PUSpecSheet.Api.DAL.Processors.Drafts.Tables.Notes;
using PUSpecSheet.Api.DAL.Processors.Drafts.Tables.Widgets;
using PUSpecSheet.Api.DAL.Processors.Drafts.Values.LinkedSpecSheetValue;
using PUSpecSheet.Api.DAL.Processors.Drafts.Values.Numeric;
using PUSpecSheet.Api.DAL.Processors.Drafts.Values.PairValue;
using PUSpecSheet.Api.DAL.Processors.Drafts.Values.Text;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts;

/// <summary>
/// Applies changes to a sheet's drafts in the revisions.
/// </summary>
internal class SheetDraftChangeProcessor : ChangeProcessorBase
{
    private readonly int _sheetId;

    public SheetDraftChangeProcessor(PUSpecSheetDbContext dbContext, User user, int sheetId, ILogger logger)
        : base(dbContext, user, logger)
    {
        _sheetId = sheetId;
    }

    protected override IOperationHandler CreateHandler(IChangeOp operation)
    {
        return operation switch
        {
            AddSpecSheetTableOp => new AddSpecSheetTableDraftOpHandler(DbContext, User, _sheetId),
            UpdateSpecSheetTableOp => new UpdateSpecSheetTableDraftOpHandler(DbContext, User, _sheetId),
            DeleteSpecSheetTableOp => new DeleteSpecSheetTableDraftOpHandler(DbContext, User, _sheetId),
            AddSpecSheetTableWidgetOp => new AddSpecSheetTableWidgetDraftOpHandler(DbContext, User, _sheetId),
            UpdateSpecSheetTableWidgetOp => new UpdateSpecSheetTableWidgetDraftOpHandler(DbContext, User, _sheetId),
            AddSpecSheetTableNoteOp => new AddSpecSheetTableNoteDraftOpHandler(DbContext, User, _sheetId),
            UpdateSpecSheetTableNoteOp => new UpdateSpecSheetTableNoteDraftOpHandler(DbContext, User, _sheetId),

            AddSpecSheetTableGroupOp => new AddSpecSheetTableGroupDraftOpHandler(DbContext, User, _sheetId),
            UpdateSpecSheetTableGroupOp => new UpdateSpecSheetTableGroupDraftOpHandler(DbContext, User, _sheetId),
            DeleteSpecSheetTableGroupOp => new DeleteSpecSheetTableGroupDraftOpHandler(DbContext, User, _sheetId),
            AddSpecSheetTableGroupWidgetOp => new AddSpecSheetTableGroupWidgetDraftOpHandler(DbContext, User, _sheetId),
            UpdateSpecSheetTableGroupWidgetOp => new UpdateSpecSheetTableGroupWidgetDraftOpHandler(DbContext, User, _sheetId),

            AddSpecSheetTableRowOp => new AddSpecSheetTableRowDraftOpHandler(DbContext, User, _sheetId),
            UpdateSpecSheetTableRowOp => new UpdateSpecSheetTableRowDraftOpHandler(DbContext, User, _sheetId),
            DeleteSpecSheetTableRowOp => new DeleteSpecSheetTableRowDraftOpHandler(DbContext, User, _sheetId),

            AddSpecSheetTableCellOp => new AddSpecSheetTableCellDraftOpHandler(DbContext, User, _sheetId),
            UpdateSpecSheetTableCellOp => new UpdateSpecSheetTableCellDraftOpHandler(DbContext, User, _sheetId),
            DeleteSpecSheetTableCellOp => new DeleteSpecSheetTableCellDraftOpHandler(DbContext, User, _sheetId),

            AddTextValueOp => new AddTextValueDraftOpHandler(DbContext, User, _sheetId),
            UpdateTextValueOp => new UpdateTextValueDraftOpHandler(DbContext, User, _sheetId),

            AddNumberValueOp => new AddNumericValueDraftOpHandler(DbContext, User, _sheetId),
            UpdateNumberValueOp => new UpdateNumericValueDraftOpHandler(DbContext, User, _sheetId),

            AddPairValueOp => new AddPairValueDraftOpHandler(DbContext, User, _sheetId),
            UpdatePairValueOp => new UpdatePairValueDraftOpHandler(DbContext, User, _sheetId),

            AddLinkedSpecSheetValueOp => new AddLinkedSpecSheetValueDraftOpHandler(DbContext, User, _sheetId),
            UpdateLinkedSpecSheetValueOp => new UpdateLinkedSpecSheetValueDraftOpHandler(DbContext, User, _sheetId),
            _ => throw new NotSupportedException($"No handler registered for operation type {operation.GetType().Name}")
        };
    }

    /// <summary>
    /// The sheet is the app's from its first edit on the revisions, so the migrator no longer builds it from its copies.
    /// </summary>
    protected override async Task BeforeApplyAsync(CancellationToken cancellationToken)
    {
        await DbContext.Sheets
            .Where(s => s.Id == _sheetId && s.LegacySourceHash != null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.LegacySourceHash, (byte[]?)null), cancellationToken);
    }
}