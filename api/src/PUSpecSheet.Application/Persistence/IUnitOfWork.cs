namespace PUSpecSheet.Application.Persistence;

/// <summary>
/// Saves what the repositories have loaded and changed, and runs several saves as one transaction. Entities
/// loaded for editing are tracked, so changing them and saving is enough to store the change.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Whether a transaction is already open, so a caller can join it rather than start its own.</summary>
    bool HasTransaction { get; }

    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Saves, turning a clash with someone else's change made at the same moment (a stale row version,
    /// or two people taking the same lock or revision number) into a conflict the screen can explain.
    /// </summary>
    Task SaveSheetChangesAsync(CancellationToken cancellationToken);

    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken);

    /// <summary>Marks a loaded entity as changed, so saving checks and moves its row version on.</summary>
    void MarkModified(object entity);

    /// <summary>Stops tracking an entity, so a later save leaves it alone.</summary>
    void Detach(object entity);

    /// <summary>Forgets every tracked entity, so the next read comes fresh from the database.</summary>
    void ClearTracking();
}
