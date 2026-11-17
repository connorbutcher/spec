using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Base;

/// <summary>
/// The base of the handlers that apply an operation to a sheet's drafts in the revisions, rather than to its copy. The ids
/// in the operations are those the draft is read with: its items' own, and a value's is its cell's.
/// </summary>
internal abstract class DraftOperationHandlerBase : OperationHandlerBase
{
    protected DraftOperationHandlerBase(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user)
    {
        SheetId = sheetId;
        Values = new RowDraftValues(dbContext);
    }

    protected int SheetId { get; }

    protected RowDraftValues Values { get; }

    /// <summary>
    /// The draft of the row the cell is in. A row's first edit starts its draft from its published revision.
    /// </summary>
    protected async Task<DbSheetTableRowRevision> GetRowDraftAsync(int cellId, CancellationToken ct)
    {
        var rowId = await DbContext.SheetTableCells
            .Where(c => c.Id == cellId)
            .Select(c => (int?)c.SheetTableRowId)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"SheetTableCell with Id {cellId} not found.");

        var draft = DbContext.SheetTableRowRevisions.Local
            .FirstOrDefault(r => r.SheetTableRowId == rowId && r.Status == RevisionStatus.Draft)
            ?? await FindOrStartRowDraftAsync(rowId, ct);

        if (draft.IsDeleted)
        {
            throw new InvalidOperationException($"SheetTableRow with Id {rowId} is removed in the draft.");
        }

        draft.LastModifiedDateTime = DateTime.UtcNow;
        return draft;
    }

    private async Task<DbSheetTableRowRevision> FindOrStartRowDraftAsync(int rowId, CancellationToken ct)
    {
        var revisions = await DbContext.SheetTableRowRevisions
            .Where(r => r.SheetTableRowId == rowId
                && r.SheetId == SheetId
                && (r.Status == RevisionStatus.Draft || r.SupersededDateTime == null))
            .Include(r => r.TextValues)
            .Include(r => r.NumericValues)
            .Include(r => r.PairValues)
            .Include(r => r.LinkedSheetValues)
            .Include(r => r.CellConfigurationOverrides)
            .AsSplitQuery()
            .ToListAsync(ct);

        var draft = revisions.FirstOrDefault(r => r.Status == RevisionStatus.Draft);
        if (draft is not null)
        {
            return draft;
        }

        var published = revisions.FirstOrDefault()
            ?? throw new KeyNotFoundException($"SheetTableRow with Id {rowId} not found on Sheet with Id {SheetId}.");
        if (published.IsDeleted)
        {
            throw new InvalidOperationException($"SheetTableRow with Id {rowId} has been removed.");
        }

        return StartRowDraft(published);
    }

    private DbSheetTableRowRevision StartRowDraft(DbSheetTableRowRevision published)
    {
        var now = DateTime.UtcNow;
        var draft = new DbSheetTableRowRevision
        {
            Id = 0,
            SheetTableRowId = published.SheetTableRowId,
            SheetId = SheetId,
            RevisionNumber = published.RevisionNumber + 1,
            Status = RevisionStatus.Draft,
            DisplayOrder = published.DisplayOrder,
            IsDeleted = false,
            CreatedByUserId = User.Id,
            CreatedDateTime = now,
            LastModifiedDateTime = now,
            PublishedDateTime = null,
            SupersededDateTime = null,
            SheetVersionId = null,
            SheetSubmissionId = null
        };

        foreach (var value in published.TextValues)
        {
            draft.TextValues.Add(value with { SheetTableRowRevisionId = 0 });
        }

        foreach (var value in published.NumericValues)
        {
            draft.NumericValues.Add(value with { SheetTableRowRevisionId = 0 });
        }

        foreach (var value in published.PairValues)
        {
            draft.PairValues.Add(value with { SheetTableRowRevisionId = 0 });
        }

        foreach (var value in published.LinkedSheetValues)
        {
            draft.LinkedSheetValues.Add(value with { SheetTableRowRevisionId = 0 });
        }

        foreach (var value in published.CellConfigurationOverrides)
        {
            draft.CellConfigurationOverrides.Add(value with { SheetTableRowRevisionId = 0 });
        }

        DbContext.SheetTableRowRevisions.Add(draft);
        return draft;
    }
}