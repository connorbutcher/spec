using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Cell;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Base;

/// <summary>
/// Finds and starts the drafts of a sheet's table groups. A group's first edit starts its draft from its published
/// revision, widgets included; a group added in the draft has only a draft.
/// </summary>
internal class GroupDrafts
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly User _user;
    private readonly int _sheetId;

    public GroupDrafts(PUSpecSheetDbContext dbContext, User user, int sheetId)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        _dbContext = dbContext;
        _user = user;
        _sheetId = sheetId;
    }

    /// <summary>
    /// The draft of the group, which mustn't be removed.
    /// </summary>
    public async Task<DbSheetTableGroupRevision> GetShownAsync(int groupId, CancellationToken ct)
    {
        var draft = await GetAsync(groupId, ct);
        if (draft.IsDeleted)
        {
            throw new InvalidOperationException($"SheetTableGroup with Id {groupId} is removed in the draft.");
        }

        return draft;
    }

    /// <summary>
    /// The draft of the group, removed or not.
    /// </summary>
    public async Task<DbSheetTableGroupRevision> GetAsync(int groupId, CancellationToken ct)
    {
        var draft = _dbContext.SheetTableGroupRevisions.Local
            .FirstOrDefault(r => r.SheetTableGroupId == groupId && r.Status == RevisionStatus.Draft)
            ?? await FindOrStartAsync(groupId, ct);

        draft.LastModifiedDateTime = DateTime.UtcNow;
        return draft;
    }

    /// <summary>
    /// The widget values of a group's draft, loaded if the draft is one already saved.
    /// </summary>
    public async Task<ICollection<DbSheetTableGroupWidgetValue>> LoadWidgetValuesAsync(DbSheetTableGroupRevision draft, CancellationToken ct)
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
    /// Adds the first revision of a group added in the draft.
    /// </summary>
    public DbSheetTableGroupRevision StartNew(DbSheetTableGroup group, int displayOrder, Guid? linkedGroupReferenceId)
    {
        ArgumentNullException.ThrowIfNull(group, nameof(group));

        var draft = NewDraft(0, 1, displayOrder, linkedGroupReferenceId);
        group.Revisions.Add(draft);
        return draft;
    }

    /// <summary>
    /// The ReferenceID of a group on the sheet, by which another group is linked to it.
    /// </summary>
    public async Task<Guid> FindReferenceIdAsync(int groupId, CancellationToken ct)
    {
        return await _dbContext.SheetTableGroups
            .Where(g => g.Id == groupId && _dbContext.SheetTables.Any(t => t.Id == g.SheetTableId && t.SheetId == _sheetId))
            .Select(g => (Guid?)g.ReferenceId)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"SheetTableGroup with Id {groupId} not found on Sheet with Id {_sheetId}.");
    }

    private async Task<DbSheetTableGroupRevision> FindOrStartAsync(int groupId, CancellationToken ct)
    {
        var revisions = await _dbContext.SheetTableGroupRevisions
            .Where(r => r.SheetTableGroupId == groupId
                && r.SheetId == _sheetId
                && (r.Status == RevisionStatus.Draft || r.SupersededDateTime == null))
            .ToListAsync(ct);

        var draft = revisions.FirstOrDefault(r => r.Status == RevisionStatus.Draft);
        if (draft is not null)
        {
            return draft;
        }

        var published = revisions.FirstOrDefault()
            ?? throw new KeyNotFoundException($"SheetTableGroup with Id {groupId} not found on Sheet with Id {_sheetId}.");
        if (published.IsDeleted)
        {
            throw new InvalidOperationException($"SheetTableGroup with Id {groupId} has been removed.");
        }

        var started = NewDraft(published.SheetTableGroupId, published.RevisionNumber + 1, published.DisplayOrder, published.LinkedSheetTableGroupReferenceId);

        var widgetValues = await _dbContext.SheetTableGroupWidgetValues
            .AsNoTracking()
            .Where(v => v.SheetTableGroupRevisionId == published.Id)
            .ToListAsync(ct);
        foreach (var value in widgetValues)
        {
            started.WidgetValues.Add(value with { SheetTableGroupRevisionId = 0 });
        }

        _dbContext.SheetTableGroupRevisions.Add(started);
        return started;
    }

    private DbSheetTableGroupRevision NewDraft(int groupId, int revisionNumber, int displayOrder, Guid? linkedGroupReferenceId)
    {
        var now = DateTime.UtcNow;
        return new DbSheetTableGroupRevision
        {
            Id = 0,
            SheetTableGroupId = groupId,
            SheetId = _sheetId,
            RevisionNumber = revisionNumber,
            Status = RevisionStatus.Draft,
            DisplayOrder = displayOrder,
            LinkedSheetTableGroupReferenceId = linkedGroupReferenceId,
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