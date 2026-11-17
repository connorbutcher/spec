using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations;
using PUSpecSheet.Abstractions.Operations.Cells;
using PUSpecSheet.Abstractions.Operations.Rows;
using PUSpecSheet.Abstractions.Operations.Values.Linked;
using PUSpecSheet.Abstractions.Operations.Values.Number;
using PUSpecSheet.Abstractions.Operations.Values.Pair;
using PUSpecSheet.Abstractions.Operations.Values.Text;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Cells;
using PUSpecSheet.Api.DAL.Processors.Drafts.Rows;
using PUSpecSheet.Api.DAL.Processors.Drafts.Values.LinkedSpecSheetValue;
using PUSpecSheet.Api.DAL.Processors.Drafts.Values.Numeric;
using PUSpecSheet.Api.DAL.Processors.Drafts.Values.PairValue;
using PUSpecSheet.Api.DAL.Processors.Drafts.Values.Text;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts;

/// <summary>
/// Applies changes to a sheet's drafts in the revisions, for a sheet type edited on the revisions. Operations that aren't
/// applied to the revisions yet are refused, and nothing is changed.
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
            _ => throw new NotSupportedException($"Operation type {operation.GetType().Name} isn't applied to the revisions yet.")
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