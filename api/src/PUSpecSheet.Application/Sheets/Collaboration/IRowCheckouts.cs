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

    /// <summary>
    /// Who has saved something on the row, which makes its draft theirs, or null when nobody has. Cheaper
    /// than <see cref="FindAsync"/>: it doesn't work out whether the draft differs from what is published.
    /// </summary>
    Task<int?> DraftHolderAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>The sheet a row is on, or null when the row doesn't exist.</summary>
    Task<int?> SheetOfAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>
    /// Moves the row from one user to another and says what moved: nothing when the first user no longer
    /// holds it. Callers only move a row that has no changes in it.
    /// </summary>
    Task<RowTransfer> TransferAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<int, string>> DisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken);
}
