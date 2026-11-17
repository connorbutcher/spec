using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Drafts;

// Starts the current user's draft on a row, which is what locks it to them. Everything that changes a row's values,
// settings or place goes through here, so the rules for taking a row are in one place.
public sealed class RowDraftStarter
{
    private readonly PuSpecSheetDbContext _db;
    private readonly RowDrafts _drafts;
    private readonly LiveRowCheckoutGuard _liveCheckouts;
    private readonly IRemovalInProgressGuard _removals;
    private readonly ICurrentUser _currentUser;
    private readonly ICellValueDataService _cellValueData;

    public RowDraftStarter(
        PuSpecSheetDbContext db,
        RowDrafts drafts,
        LiveRowCheckoutGuard liveCheckouts,
        IRemovalInProgressGuard removals,
        ICurrentUser currentUser,
        ICellValueDataService cellValueData)
    {
        _db = db;
        _drafts = drafts;
        _liveCheckouts = liveCheckouts;
        _removals = removals;
        _currentUser = currentUser;
        _cellValueData = cellValueData;
    }

    // The user's draft on the row. A newly started draft is saved straight away and given a copy of the published
    // values and settings, so editing carries on from what's published.
    public async Task<DbSheetRowRevision> StartAsync(int rowId, CancellationToken cancellationToken)
    {
        // Someone who has only clicked into the row holds it too, though they have no draft yet.
        _liveCheckouts.EnsureNotHeldByOthers(rowId);

        var state = await _drafts.LoadAsync(rowId, cancellationToken);
        if (state.Draft is null)
        {
            // Holding a draft already means nobody could have started removing around it since, so only
            // a new draft needs the check, and saves into an open one cost nothing extra.
            await _removals.EnsureRowNotBeingRemovedAsync(rowId, _currentUser.UserId, cancellationToken);
        }

        var (draft, created) = await _drafts.EnsureMineAsync(rowId, state, cancellationToken);
        if (!created)
        {
            return draft;
        }

        await _db.SaveSheetChangesAsync(cancellationToken);
        if (state.Current is { } current)
        {
            await _cellValueData.CopyAsync(current.Id, draft.Id, cancellationToken);
        }

        return draft;
    }
}
