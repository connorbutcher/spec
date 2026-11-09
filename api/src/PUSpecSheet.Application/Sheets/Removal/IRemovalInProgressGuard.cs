namespace PUSpecSheet.Application.Sheets.Removal;

/// <summary>
/// Refuses changes inside a section or table that someone else is removing (see <see cref="PendingRemovals"/>).
/// It is the seam the row checkout rules' tests replace.
/// </summary>
public interface IRemovalInProgressGuard
{
    /// <summary>Refuses a change to a row in a section or table someone else is removing. A missing row passes.</summary>
    /// <exception cref="Common.ConflictException">The row's section, a section around it, or its table is being removed.</exception>
    Task EnsureRowNotBeingRemovedAsync(int rowId, int userId, CancellationToken cancellationToken);

    /// <summary>
    /// Refuses a change to something placed in a table, directly or inside <paramref name="sectionId"/>,
    /// when that section, a section around it, or the table is being removed.
    /// </summary>
    /// <exception cref="Common.ConflictException">The section, a section around it, or the table is being removed.</exception>
    Task EnsurePlaceNotBeingRemovedAsync(int tableId, int? sectionId, int userId, CancellationToken cancellationToken);
}
