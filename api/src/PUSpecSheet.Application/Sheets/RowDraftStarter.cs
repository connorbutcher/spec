using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Starts the current user's draft on a row, which is what locks it to them. Everything that changes a
/// row's values, settings or place goes through here, so the rules for taking a row are in one place.
/// </summary>
public sealed class RowDraftStarter(
    PuSpecSheetDbContext db,
    RowDrafts drafts,
    LiveRowCheckoutGuard liveCheckouts,
    RowValueStore valueStore)
{
    /// <summary>
    /// The user's draft on the row. A newly started draft is saved straight away and given a copy of the
    /// published values and settings, so editing carries on from what's published.
    /// </summary>
    public async Task<SheetRowRevision> StartAsync(int rowId, CancellationToken cancellationToken)
    {
        // Someone who has only clicked into the row holds it too, though they have no draft yet.
        liveCheckouts.EnsureNotHeldByOthers(rowId);

        var state = await drafts.LoadAsync(rowId, cancellationToken);
        var (draft, created) = await drafts.EnsureMineAsync(rowId, state, cancellationToken);
        if (!created)
        {
            return draft;
        }

        await db.SaveSheetChangesAsync(cancellationToken);
        if (state.Current is { } current)
        {
            await valueStore.CopyAsync(current.Id, draft.Id, cancellationToken);
            await db.SaveSheetChangesAsync(cancellationToken);
        }

        return draft;
    }
}
