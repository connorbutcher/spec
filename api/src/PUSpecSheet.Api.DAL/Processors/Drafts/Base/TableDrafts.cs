using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Cell;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.DAL.Processors.Drafts.Locks;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Base;

/// <summary>
/// Finds and starts the drafts of a sheet's tables. A table's first edit starts its draft from its published revision,
/// widgets and notes included; a table added in the draft has only a draft.
/// </summary>
internal class TableDrafts
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly User _user;
    private readonly int _sheetId;
    private readonly DraftLocks _locks;

    public TableDrafts(PUSpecSheetDbContext dbContext, User user, int sheetId)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        _dbContext = dbContext;
        _user = user;
        _sheetId = sheetId;
        _locks = new DraftLocks(dbContext, user);
    }

    /// <summary>
    /// The draft of the table, which mustn't be removed.
    /// </summary>
    public async Task<DbSheetTableRevision> GetShownAsync(int tableId, CancellationToken ct)
    {
        var draft = await GetAsync(tableId, ct);
        if (draft.IsDeleted)
        {
            throw new InvalidOperationException($"SheetTable with Id {tableId} is removed in the draft.");
        }

        return draft;
    }

    /// <summary>
    /// The draft of the table, removed or not.
    /// </summary>
    public async Task<DbSheetTableRevision> GetAsync(int tableId, CancellationToken ct)
    {
        var draft = _dbContext.SheetTableRevisions.Local
            .FirstOrDefault(r => r.SheetTableId == tableId && r.Status == RevisionStatus.Draft)
            ?? await FindOrStartAsync(tableId, ct);
        await _locks.EnsureEditableAsync(draft, "table", ct);

        draft.LastModifiedDateTime = DateTime.UtcNow;
        return draft;
    }

    /// <summary>
    /// The widget values of a table's draft, loaded if the draft is one already saved.
    /// </summary>
    public async Task<ICollection<DbSheetTableWidgetValue>> LoadWidgetValuesAsync(DbSheetTableRevision draft, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(draft, nameof(draft));

        if (draft.Id != 0)
        {
            var widgetValues = _dbContext.Entry(draft).Collection(r => r.WidgetValues);
            if (!widgetValues.IsLoaded)
            {
                await widgetValues.LoadAsync(ct);
            }
        }

        return draft.WidgetValues;
    }

    /// <summary>
    /// The note values of a table's draft, loaded if the draft is one already saved.
    /// </summary>
    public async Task<ICollection<DbSheetTableNoteValue>> LoadNoteValuesAsync(DbSheetTableRevision draft, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(draft, nameof(draft));

        if (draft.Id != 0)
        {
            var noteValues = _dbContext.Entry(draft).Collection(r => r.NoteValues);
            if (!noteValues.IsLoaded)
            {
                await noteValues.LoadAsync(ct);
            }
        }

        return draft.NoteValues;
    }

    /// <summary>
    /// Adds the first revision of a table added in the draft.
    /// </summary>
    public DbSheetTableRevision StartNew(DbSheetTable table, string title, int displayOrder, ITableProperties? configurationOverride)
    {
        ArgumentNullException.ThrowIfNull(table, nameof(table));

        var draft = NewDraft(0, 1, title, displayOrder);
        draft.ConfigurationOverride = configurationOverride;
        table.Revisions.Add(draft);
        return draft;
    }

    private async Task<DbSheetTableRevision> FindOrStartAsync(int tableId, CancellationToken ct)
    {
        var revisions = await _dbContext.SheetTableRevisions
            .Where(r => r.SheetTableId == tableId
                && r.SheetId == _sheetId
                && (r.Status == RevisionStatus.Draft || r.SupersededDateTime == null))
            .ToListAsync(ct);

        var draft = revisions.FirstOrDefault(r => r.Status == RevisionStatus.Draft);
        if (draft is not null)
        {
            return draft;
        }

        var published = revisions.FirstOrDefault()
            ?? throw new KeyNotFoundException($"SheetTable with Id {tableId} not found on Sheet with Id {_sheetId}.");
        if (published.IsDeleted)
        {
            throw new InvalidOperationException($"SheetTable with Id {tableId} has been removed.");
        }

        var started = NewDraft(published.SheetTableId, published.RevisionNumber + 1, published.Title, published.DisplayOrder);
        started.ConfigurationOverride = published.ConfigurationOverride;

        var widgetValues = await _dbContext.SheetTableWidgetValues
            .AsNoTracking()
            .Where(v => v.SheetTableRevisionId == published.Id)
            .ToListAsync(ct);
        foreach (var value in widgetValues)
        {
            started.WidgetValues.Add(value with { SheetTableRevisionId = 0 });
        }

        var noteValues = await _dbContext.SheetTableNoteValues
            .AsNoTracking()
            .Where(v => v.SheetTableRevisionId == published.Id)
            .ToListAsync(ct);
        foreach (var value in noteValues)
        {
            started.NoteValues.Add(value with { SheetTableRevisionId = 0 });
        }

        _dbContext.SheetTableRevisions.Add(started);
        return started;
    }

    private DbSheetTableRevision NewDraft(int tableId, int revisionNumber, string title, int displayOrder)
    {
        var now = DateTime.UtcNow;
        return new DbSheetTableRevision
        {
            Id = 0,
            SheetTableId = tableId,
            SheetId = _sheetId,
            RevisionNumber = revisionNumber,
            Status = RevisionStatus.Draft,
            Title = title,
            DisplayOrder = displayOrder,
            ConfigurationOverride = null,
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