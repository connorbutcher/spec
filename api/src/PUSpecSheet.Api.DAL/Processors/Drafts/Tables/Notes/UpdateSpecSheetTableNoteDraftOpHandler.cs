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

        var note = await Numbers.FindNoteAsync(operation.Id, ct);
        var draft = await Tables.GetShownAsync(note.ItemId, ct);

        var notes = (draft.Notes ?? []).ToList();
        if (note.Index >= notes.Count)
        {
            throw new InvalidOperationException($"SheetTable with Id {note.ItemId} holds no note at position {note.Index + 1}.");
        }

        notes[note.Index] = notes[note.Index] with { IsObsolete = operation.IsObsolete.Value };
        draft.Notes = notes;
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