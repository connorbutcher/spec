using PUSpecSheet.Application.Sheets.Collaboration;

namespace PUSpecSheet.Application.Tests.Sheets.Collaboration;

/// <summary>Row checkouts held in a dictionary, standing in for the draft revisions in the database.</summary>
internal sealed class FakeRowCheckouts : IRowCheckouts
{
    private readonly Dictionary<int, RowCheckout> checkedOut = [];
    private readonly HashSet<int> rows = [];

    public int? HolderOf(int rowId)
    {
        return checkedOut.GetValueOrDefault(rowId)?.HolderUserId;
    }

    public void AddRow(int rowId)
    {
        rows.Add(rowId);
    }

    public void CheckOut(int rowId, int sheetId, int holderUserId)
    {
        rows.Add(rowId);
        checkedOut[rowId] = new RowCheckout(sheetId, holderUserId, HasChanges: false);
    }

    /// <summary>The holder types something into the row.</summary>
    public void Change(int rowId)
    {
        checkedOut[rowId] = checkedOut[rowId] with { HasChanges = true };
    }

    public void Release(int rowId)
    {
        checkedOut.Remove(rowId);
    }

    public Task<RowCheckout?> FindAsync(int rowId, CancellationToken cancellationToken)
    {
        return Task.FromResult(checkedOut.GetValueOrDefault(rowId));
    }

    public Task<bool> RowExistsAsync(int rowId, CancellationToken cancellationToken)
    {
        return Task.FromResult(rows.Contains(rowId));
    }

    public Task<bool> TransferAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken)
    {
        if (!checkedOut.TryGetValue(rowId, out var checkout) || checkout.HolderUserId != fromUserId)
        {
            return Task.FromResult(false);
        }

        checkedOut[rowId] = checkout with { HolderUserId = toUserId };
        return Task.FromResult(true);
    }

    public Task<IReadOnlyDictionary<int, string>> DisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<int, string> names = userIds.ToDictionary(userId => userId, userId => $"User {userId}");
        return Task.FromResult(names);
    }
}
