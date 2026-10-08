using PUSpecSheet.Application.Sheets.Collaboration;

namespace PUSpecSheet.Application.Tests.Sheets.Collaboration;

/// <summary>
/// Row checkouts held in a dictionary, standing in for the draft revisions in the database. Every
/// checkout here counts as a draft; <see cref="CheckOut"/> makes one with no changes in it.
/// </summary>
internal sealed class FakeRowCheckouts : IRowCheckouts
{
    private readonly Dictionary<int, RowCheckout> checkedOut = [];
    private readonly Dictionary<int, int> sheetByRow = [];

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

    public Task<RowCheckout?> FindDraftAsync(int rowId, CancellationToken cancellationToken)
    {
        return FindAsync(rowId, cancellationToken);
    }

    public Task<int?> SheetOfAsync(int rowId, CancellationToken cancellationToken)
    {
        return Task.FromResult(sheetByRow.TryGetValue(rowId, out var sheetId) ? sheetId : (int?)null);
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
