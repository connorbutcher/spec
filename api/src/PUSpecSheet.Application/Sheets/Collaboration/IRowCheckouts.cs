namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// Who rows are checked out to, read from and written to the database. A row's checkout is its draft
/// revision, so this is the one place the takeover rules touch stored data, and the seam their tests
/// replace.
/// </summary>
public interface IRowCheckouts
{
    /// <summary>The row's checkout, or null when nobody has it checked out.</summary>
    Task<RowCheckout?> FindAsync(int rowId, CancellationToken cancellationToken);

    Task<bool> RowExistsAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>The holder of each of the given rows that is checked out, by row id.</summary>
    Task<IReadOnlyDictionary<int, int>> HoldersAsync(IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken);

    /// <summary>
    /// Moves the row's draft from one user to another as it stands, unpublished changes included. False
    /// when the first user no longer holds it, in which case nothing changes.
    /// </summary>
    Task<bool> TransferAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken);

    /// <summary>The display names of the given users, by user id.</summary>
    Task<IReadOnlyDictionary<int, string>> DisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken);
}
