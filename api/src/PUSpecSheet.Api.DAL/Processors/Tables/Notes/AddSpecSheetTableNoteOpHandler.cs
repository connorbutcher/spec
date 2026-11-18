using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables.Notes;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.DAL.Processors.Tables.Notes;

internal class AddSpecSheetTableNoteOpHandler : OperationHandlerBase, IOperationHandler<AddSpecSheetTableNoteOp>
{
    public AddSpecSheetTableNoteOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(AddSpecSheetTableNoteOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableId = ResolveId(operation.SpecSheetTableId, tempIdMap);

        var newSpecSheetTableNote = new DbSpecSheetTableNote
        {
            Id = 0,
            SpecSheetTableId = specSheetTableId,
            Note = operation.Note,
            SourceSystem = operation.SourceSystem,
            IsObsolete = false,
            CreatedByUserId = User.Id,
            CreatedDateTime = DateTime.UtcNow,
        };

        await DbContext.SpecSheetTableNotes.AddAsync(newSpecSheetTableNote, ct);
    }

    public (int TempId, int RealId)? GetGeneratedId(AddSpecSheetTableNoteOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(AddSpecSheetTableNoteOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return IsTempId(operation.SpecSheetTableId) && !tempIdMap.ContainsKey(operation.SpecSheetTableId);
    }
}
