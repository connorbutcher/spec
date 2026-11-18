using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables.Notes;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Tables.Notes;

internal class UpdateSpecSheetTableNoteDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<UpdateSpecSheetTableNoteOp>
{
    public UpdateSpecSheetTableNoteDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableNoteOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        if (!operation.IsObsolete.HasValue)
        {
            return;
        }

        var tableId = await Owners.TableOfNoteAsync(operation.Id, ct);
        var draft = await Tables.GetShownAsync(tableId, ct);

        var note = (await Tables.LoadNoteValuesAsync(draft, ct)).FirstOrDefault(v => v.SheetTableNoteId == operation.Id)
            ?? throw new InvalidOperationException($"SheetTableNote with Id {operation.Id} isn't in the draft of SheetTable with Id {tableId}.");
        note.IsObsolete = operation.IsObsolete.Value;
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