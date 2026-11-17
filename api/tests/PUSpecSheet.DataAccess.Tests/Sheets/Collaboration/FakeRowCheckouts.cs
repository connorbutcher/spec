using PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;

namespace PUSpecSheet.DataAccess.Tests.Sheets.Collaboration;

/// <summary>
/// Row checkouts held in a dictionary, standing in for the draft revisions in the database and the live
/// checkouts in memory. <see cref="CheckOut"/> makes a draft with no changes in it; <see cref="CheckOutLive"/>
/// a live checkout, which by definition has none.
/// </summary>
internal sealed class FakeRowCheckouts : IRowCheckouts
{
    private readonly Dictionary<int, RowCheckoutState> _checkedOut = [];
    private readonly Dictionary<int, int> _sheetByRow = [];
    private readonly HashSet<int> _liveRows = [];

    public int? HolderOf(int rowId)
    {
        return _checkedOut.GetValueOrDefault(rowId)?.HolderUserId;
    }

    public void AddRow(int rowId, int sheetId = 1)
    {
        _sheetByRow[rowId] = sheetId;
    }

    public void CheckOut(int rowId, int sheetId, int holderUserId)
    {
        _sheetByRow[rowId] = sheetId;
        _checkedOut[rowId] = new RowCheckoutState { SheetId = sheetId, HolderUserId = holderUserId, HasChanges = false };
        _liveRows.Remove(rowId);
    }

    /// <summary>The holder is in the row but hasn't saved anything, so it has no draft.</summary>
    public void CheckOutLive(int rowId, int sheetId, int holderUserId)
    {
        CheckOut(rowId, sheetId, holderUserId);
        _liveRows.Add(rowId);
    }

    /// <summary>The holder types something into the row.</summary>
    public void Change(int rowId)
    {
        _checkedOut[rowId] = _checkedOut[rowId] with { HasChanges = true };
        _liveRows.Remove(rowId);
    }

    public void Release(int rowId)
    {
        _checkedOut.Remove(rowId);
        _liveRows.Remove(rowId);
    }

    public Task<RowCheckoutState?> FindAsync(int rowId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_checkedOut.GetValueOrDefault(rowId));
    }

    public Task<int?> DraftHolderAsync(int rowId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_liveRows.Contains(rowId) ? null : HolderOf(rowId));
    }

    public Task<int?> SheetOfAsync(int rowId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_sheetByRow.TryGetValue(rowId, out var sheetId) ? sheetId : (int?)null);
    }

    public Task<RowTransfer> TransferAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken)
    {
        if (!_checkedOut.TryGetValue(rowId, out var checkout) || checkout.HolderUserId != fromUserId)
        {
            return Task.FromResult(RowTransfer.Nothing);
        }

        _checkedOut[rowId] = checkout with { HolderUserId = toUserId };
        return Task.FromResult(_liveRows.Contains(rowId) ? RowTransfer.Live : RowTransfer.Draft);
    }

    public Task<IReadOnlyDictionary<int, string>> DisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<int, string> names = userIds.ToDictionary(userId => userId, userId => $"User {userId}");
        return Task.FromResult(names);
    }
}
