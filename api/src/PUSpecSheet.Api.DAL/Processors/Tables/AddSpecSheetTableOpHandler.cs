using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.SpecSheets;

namespace PUSpecSheet.Api.DAL.Processors.Tables;

internal class AddSpecSheetTableOpHandler : OperationHandlerBase, IOperationHandler<AddSpecSheetTableOp>
{
    private DbSpecSheetTable? _addedEntity;

    public AddSpecSheetTableOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(AddSpecSheetTableOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetId = ResolveId(operation.SpecSheetId, tempIdMap);

        var newSpecSheetTable = new DbSpecSheetTable
        {
            Id = 0,
            SpecSheetId = specSheetId,
            ReferenceId = operation.ReferenceId,
            TableTemplateId = operation.TableTemplateId,
            Title = operation.Title,
            CreatedDateTime = DateTime.UtcNow,
            CreatedByUserId = User.Id,
            DisplayOrder = operation.DisplayOrder,
            ConfigurationOverride = operation.ConfigurationOverride,
            IsDeleted = false,
        };

        await DbContext.SpecSheetTables.AddAsync(newSpecSheetTable, ct);
        _addedEntity = newSpecSheetTable;
    }

    public (int TempId, int RealId)? GetGeneratedId(AddSpecSheetTableOp operation)
    {
        return _addedEntity != null
            ? (operation.TempId, _addedEntity.Id)
            : null;
    }

    public bool HasUnresolvedDependencies(AddSpecSheetTableOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return IsTempId(operation.SpecSheetId) && !tempIdMap.ContainsKey(operation.SpecSheetId);
    }
}