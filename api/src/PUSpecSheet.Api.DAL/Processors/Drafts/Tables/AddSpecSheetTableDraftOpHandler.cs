using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Tables;

internal class AddSpecSheetTableDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<AddSpecSheetTableOp>
{
    private DbSheetTable? _addedEntity;

    public AddSpecSheetTableDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(AddSpecSheetTableOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var newTable = new DbSheetTable
        {
            Id = 0,
            SheetId = SheetId,
            TableTemplateId = operation.TableTemplateId,
            ReferenceId = operation.ReferenceId,
            CreatedDateTime = DateTime.UtcNow
        };

        Tables.StartNew(newTable, operation.Title, operation.DisplayOrder, operation.ConfigurationOverride);
        await DbContext.SheetTables.AddAsync(newTable, ct);
        _addedEntity = newTable;
    }

    public (int TempId, int RealId)? GetGeneratedId(AddSpecSheetTableOp operation)
    {
        return _addedEntity != null
            ? (operation.TempId, _addedEntity.Id)
            : null;
    }

    public bool HasUnresolvedDependencies(AddSpecSheetTableOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}