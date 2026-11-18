using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables.Notes;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database.Schema.Cell;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Tables.Notes;

internal class AddSpecSheetTableNoteDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<AddSpecSheetTableNoteOp>
{
    public AddSpecSheetTableNoteDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(AddSpecSheetTableNoteOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var tableId = ResolveId(operation.SpecSheetTableId, tempIdMap);
        var draft = await Tables.GetShownAsync(tableId, ct);

        var newNote = new DbSheetTableNote
        {
            Id = 0,
            SheetTableId = tableId,
            SourceSystem = operation.SourceSystem,
            CreatedByUserId = User.Id,
            CreatedDateTime = DateTime.UtcNow
        };
        await DbContext.SheetTableNotes.AddAsync(newNote, ct);

        draft.NoteValues.Add(new DbSheetTableNoteValue
        {
            SheetTableRevisionId = 0,
            SheetTableNoteId = 0,
            Note = operation.Note,
            IsObsolete = false,
            SheetTableNote = newNote
        });
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