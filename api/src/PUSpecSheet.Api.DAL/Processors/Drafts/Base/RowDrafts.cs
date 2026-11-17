using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Base;

/// <summary>
/// Finds and starts the drafts of a sheet's rows. A row's first edit starts its draft from its published revision, values
/// included; a row added in the draft has only a draft.
/// </summary>
internal class RowDrafts
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly User _user;
    private readonly int _sheetId;

    public RowDrafts(PUSpecSheetDbContext dbContext, User user, int sheetId)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        _dbContext = dbContext;
        _user = user;
        _sheetId = sheetId;
    }

    public async Task<int> FindRowIdOfCellAsync(int cellId, CancellationToken ct)
    {
        return await _dbContext.SheetTableCells
            .Where(c => c.Id == cellId)
            .Select(c => (int?)c.SheetTableRowId)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"SheetTableCell with Id {cellId} not found.");
    }

    /// <summary>
    /// The draft of the row the cell is in, which mustn't be removed.
    /// </summary>
    public async Task<DbSheetTableRowRevision> GetOfCellAsync(int cellId, CancellationToken ct)
    {
        var rowId = await FindRowIdOfCellAsync(cellId, ct);
        return await GetShownAsync(rowId, ct);
    }

    /// <summary>
    /// The draft of the row, which mustn't be removed.
    /// </summary>
    public async Task<DbSheetTableRowRevision> GetShownAsync(int rowId, CancellationToken ct)
    {
        var draft = await GetAsync(rowId, ct);
        if (draft.IsDeleted)
        {
            throw new InvalidOperationException($"SheetTableRow with Id {rowId} is removed in the draft.");
        }

        return draft;
    }

    /// <summary>
    /// The draft of the row, removed or not.
    /// </summary>
    public async Task<DbSheetTableRowRevision> GetAsync(int rowId, CancellationToken ct)
    {
        var draft = _dbContext.SheetTableRowRevisions.Local
            .FirstOrDefault(r => r.SheetTableRowId == rowId && r.Status == RevisionStatus.Draft)
            ?? await FindOrStartAsync(rowId, ct);

        draft.LastModifiedDateTime = DateTime.UtcNow;
        return draft;
    }

    /// <summary>
    /// Adds the first revision of a row added in the draft.
    /// </summary>
    public DbSheetTableRowRevision StartNew(DbSheetTableRow row, int displayOrder)
    {
        ArgumentNullException.ThrowIfNull(row, nameof(row));

        var draft = NewDraft(0, 1, displayOrder);
        row.Revisions.Add(draft);
        return draft;
    }

    private async Task<DbSheetTableRowRevision> FindOrStartAsync(int rowId, CancellationToken ct)
    {
        var revisions = await _dbContext.SheetTableRowRevisions
            .Where(r => r.SheetTableRowId == rowId
                && r.SheetId == _sheetId
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
            ?? throw new KeyNotFoundException($"SheetTableRow with Id {rowId} not found on Sheet with Id {_sheetId}.");
        if (published.IsDeleted)
        {
            throw new InvalidOperationException($"SheetTableRow with Id {rowId} has been removed.");
        }

        return StartFrom(published);
    }

    private DbSheetTableRowRevision StartFrom(DbSheetTableRowRevision published)
    {
        var draft = NewDraft(published.SheetTableRowId, published.RevisionNumber + 1, published.DisplayOrder);

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

        _dbContext.SheetTableRowRevisions.Add(draft);
        return draft;
    }

    private DbSheetTableRowRevision NewDraft(int rowId, int revisionNumber, int displayOrder)
    {
        var now = DateTime.UtcNow;
        return new DbSheetTableRowRevision
        {
            Id = 0,
            SheetTableRowId = rowId,
            SheetId = _sheetId,
            RevisionNumber = revisionNumber,
            Status = RevisionStatus.Draft,
            DisplayOrder = displayOrder,
            IsDeleted = false,
            CreatedByUserId = _user.Id,
            CreatedDateTime = now,
            LastModifiedDateTime = now,
            PublishedDateTime = null,
            SupersededDateTime = null,
            SheetVersionId = null,
            SheetSubmissionId = null
        };
    }
}