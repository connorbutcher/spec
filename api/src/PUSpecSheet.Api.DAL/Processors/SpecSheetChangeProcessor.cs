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
using PUSpecSheet.Api.DAL.Processors.Cells;
using PUSpecSheet.Api.DAL.Processors.Rows;
using PUSpecSheet.Api.DAL.Processors.TableGroups;
using PUSpecSheet.Api.DAL.Processors.TableGroups.Widgets;
using PUSpecSheet.Api.DAL.Processors.Tables;
using PUSpecSheet.Api.DAL.Processors.Tables.Notes;
using PUSpecSheet.Api.DAL.Processors.Tables.Widgets;
using PUSpecSheet.Api.DAL.Processors.Values.LinkedSpecSheetValue;
using PUSpecSheet.Api.DAL.Processors.Values.Numeric;
using PUSpecSheet.Api.DAL.Processors.Values.PairValue;
using PUSpecSheet.Api.DAL.Processors.Values.Text;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors;

/// <summary>
/// Processes and applies changes to process versions by handling operation dependencies and managing database transactions.
/// </summary>
internal class SpecSheetChangeProcessor : ChangeProcessorBase
{
    public SpecSheetChangeProcessor(PUSpecSheetDbContext dbContext, User user, ILogger logger)
        : base(dbContext, user, logger)
    {
    }

    protected override IOperationHandler CreateHandler(IChangeOp operation)
    {
        return operation switch
        {
            AddSpecSheetTableOp => new AddSpecSheetTableOpHandler(DbContext, User),
            UpdateSpecSheetTableOp => new UpdateSpecSheetTableOpHandler(DbContext, User),
            DeleteSpecSheetTableOp => new DeleteSpecSheetTableOpHandler(DbContext, User),
            AddSpecSheetTableWidgetOp => new AddSpecSheetTableWidgetOpHandler(DbContext, User),
            UpdateSpecSheetTableWidgetOp => new UpdateSpecSheetTableWidgetOpHandler(DbContext, User),
            AddSpecSheetTableNoteOp => new AddSpecSheetTableNoteOpHandler(DbContext, User),
            UpdateSpecSheetTableNoteOp => new UpdateSpecSheetTableNoteOpHandler(DbContext, User),

            AddSpecSheetTableGroupOp => new AddSpecSheetTableGroupOpHandler(DbContext, User),
            UpdateSpecSheetTableGroupOp => new UpdateSpecSheetTableGroupOpHandler(DbContext, User),
            DeleteSpecSheetTableGroupOp => new DeleteSpecSheetTableGroupOpHandler(DbContext, User),
            AddSpecSheetTableGroupWidgetOp => new AddSpecSheetTableGroupWidgetOpHandler(DbContext, User),
            UpdateSpecSheetTableGroupWidgetOp => new UpdateSpecSheetTableGroupWidgetOpHandler(DbContext, User),

            AddSpecSheetTableRowOp => new AddSpecSheetTableRowOpHandler(DbContext, User),
            UpdateSpecSheetTableRowOp => new UpdateSpecSheetTableRowOpHandler(DbContext, User),
            DeleteSpecSheetTableRowOp => new DeleteSpecSheetTableRowOpHandler(DbContext, User),

            AddSpecSheetTableCellOp => new AddSpecSheetTableCellOpHandler(DbContext, User),
            UpdateSpecSheetTableCellOp => new UpdateSpecSheetTableCellOpHandler(DbContext, User),
            DeleteSpecSheetTableCellOp => new DeleteSpecSheetTableCellOpHandler(DbContext, User),

            AddTextValueOp => new AddTextValueOpHandler(DbContext, User),
            UpdateTextValueOp => new UpdateTextValueOpHandler(DbContext, User),

            AddNumberValueOp => new AddNumericValueOpHandler(DbContext, User),
            UpdateNumberValueOp => new UpdateNumericValueOpHandler(DbContext, User),

            AddPairValueOp => new AddPairValueOpHandler(DbContext, User),
            UpdatePairValueOp => new UpdatePairValueOpHandler(DbContext, User),

            AddLinkedSpecSheetValueOp => new AddLinkedSpecSheetValueOpHandler(DbContext, User),
            UpdateLinkedSpecSheetValueOp => new UpdateLinkedSpecSheetValueOpHandler(DbContext, User),
            _ => throw new NotSupportedException($"No handler registered for operation type {operation.GetType().Name}")
        };
    }
}
