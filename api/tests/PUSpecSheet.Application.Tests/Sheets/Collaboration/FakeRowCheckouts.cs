using PUSpecSheet.Application.Sheets.Collaboration;

namespace PUSpecSheet.Application.Tests.Sheets.Collaboration;

/// <summary>
/// Row checkouts held in a dictionary, standing in for the draft revisions in the database and the live
/// checkouts in memory. <see cref="CheckOut"/> makes a draft with no changes in it; <see cref="CheckOutLive"/>
/// a live checkout, which by definition has none.
/// </summary>
internal sealed class FakeRowCheckouts : IRowCheckouts
{
    private readonly Dictionary<int, RowCheckout> checkedOut = [];
    private readonly Dictionary<int, int> sheetByRow = [];
    private readonly HashSet<int> liveRows = [];

    public int? HolderOf(int rowId)
    {
        return checkedOut.GetValueOrDefault(rowId)?.HolderUserId;
    }

    public void AddRow(int rowId, int sheetId = 1)
    {
        sheetByRow[rowId] = sheetId;
    }

    public void CheckOut(int rowId, int sheetId, int holderUserId)
    {
        sheetByRow[rowId] = sheetId;
        checkedOut[rowId] = new RowCheckout(sheetId, holderUserId, HasChanges: false);
        liveRows.Remove(rowId);
    }

    /// <summary>The holder is in the row but hasn't saved anything, so it has no draft.</summary>
    public void CheckOutLive(int rowId, int sheetId, int holderUserId)
    {
        CheckOut(rowId, sheetId, holderUserId);
        liveRows.Add(rowId);
    }

    /// <summary>The holder types something into the row.</summary>
    public void Change(int rowId)
    {
        checkedOut[rowId] = checkedOut[rowId] with { HasChanges = true };
        liveRows.Remove(rowId);
    }

    public void Release(int rowId)
    {
        checkedOut.Remove(rowId);
        liveRows.Remove(rowId);
    }

    public Task<RowCheckout?> FindAsync(int rowId, CancellationToken cancellationToken)
    {
        return Task.FromResult(checkedOut.GetValueOrDefault(rowId));
    }

    public Task<int?> DraftHolderAsync(int rowId, CancellationToken cancellationToken)
    {
        return Task.FromResult(liveRows.Contains(rowId) ? null : HolderOf(rowId));
    }

    public Task<int?> SheetOfAsync(int rowId, CancellationToken cancellationToken)
    {
        return Task.FromResult(sheetByRow.TryGetValue(rowId, out var sheetId) ? sheetId : (int?)null);
    }

    public Task<RowTransfer> TransferAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken)
    {
        if (!checkedOut.TryGetValue(rowId, out var checkout) || checkout.HolderUserId != fromUserId)
        {
            return Task.FromResult(RowTransfer.Nothing);
        }

        checkedOut[rowId] = checkout with { HolderUserId = toUserId };
        return Task.FromResult(liveRows.Contains(rowId) ? RowTransfer.Live : RowTransfer.Draft);
    }

    public Task<IReadOnlyDictionary<int, string>> DisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<int, string> names = userIds.ToDictionary(userId => userId, userId => $"User {userId}");
        return Task.FromResult(names);
    }
}
