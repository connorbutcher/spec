namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// Who rows are checked out to. A row is checked out either because someone has changed it (its draft
/// revision in the database) or because someone is in it without having changed it yet (held in memory
/// by <see cref="LiveRowCheckoutTracker"/>). This puts the two behind one question, and is the seam the
/// takeover and checkout rules' tests replace.
/// </summary>
public interface IRowCheckouts
{
    /// <summary>The row's checkout of either kind, or null when nobody has it. A draft comes first.</summary>
    Task<RowCheckout?> FindAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>The row's checkout only if it is a draft: someone has saved something on it.</summary>
    Task<RowCheckout?> FindDraftAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>The sheet a row is on, or null when the row doesn't exist.</summary>
    Task<int?> SheetOfAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>
    /// Moves the row from one user to another. False when the first user no longer holds it, in which
    /// case nothing changes. Callers only move a row that has no changes in it.
    /// </summary>
    Task<bool> TransferAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken);

    /// <summary>The display names of the given users, by user id.</summary>
    Task<IReadOnlyDictionary<int, string>> DisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken);
}
