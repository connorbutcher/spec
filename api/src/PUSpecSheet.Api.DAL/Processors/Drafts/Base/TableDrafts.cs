using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
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

    public TableDrafts(PUSpecSheetDbContext dbContext, User user, int sheetId)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        _dbContext = dbContext;
        _user = user;
        _sheetId = sheetId;
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

        draft.LastModifiedDateTime = DateTime.UtcNow;
        return draft;
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
        started.WidgetValues = published.WidgetValues is null ? null : [.. published.WidgetValues];
        started.Notes = published.Notes is null ? null : [.. published.Notes];

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
            WidgetValues = null,
            Notes = null,
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