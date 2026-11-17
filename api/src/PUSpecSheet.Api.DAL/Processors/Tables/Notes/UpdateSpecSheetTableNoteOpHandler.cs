using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables.Notes;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Tables.Notes;

internal class UpdateSpecSheetTableNoteOpHandler : OperationHandlerBase, IOperationHandler<UpdateSpecSheetTableNoteOp>
{
    public UpdateSpecSheetTableNoteOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableNoteOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableNote = await DbContext.SpecSheetTableNotes.FindAsync([operation.Id], ct)
            ?? throw new InvalidOperationException($"SpecSheetTableNote with Id {operation.Id} not found.");

        if (operation.IsObsolete.HasValue)
        {
            specSheetTableNote.IsObsolete = operation.IsObsolete.Value;
        }
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateSpecSheetTableNoteOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateSpecSheetTableNoteOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}