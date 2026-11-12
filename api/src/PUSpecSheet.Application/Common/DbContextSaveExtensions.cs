using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Common;

/// <summary>
/// The ways the services save: plainly, refusing quietly, or turning a clash with someone else's change into a
/// conflict the screen can explain. Entities loaded for editing are tracked, so changing them and saving is
/// enough to store the change.
/// </summary>
public static class DbContextSaveExtensions
{
    private const string ClashMessage = "Someone else changed this at the same moment. Refresh the sheet and try again.";

    /// <summary>
    /// Saves, returning false instead of throwing when the database refuses the change (such as a row someone
    /// else inserted at the same moment). The refused entities stay tracked; detach them before going on.
    /// </summary>
    public static async Task<bool> TrySaveChangesAsync(this PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    /// <summary>
    /// Saves, turning a clash with someone else's change made at the same moment (a stale row version,
    /// or two people taking the same lock or revision number) into a conflict the screen can explain.
    /// </summary>
    public static async Task SaveSheetChangesAsync(this PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(ClashMessage);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new ConflictException(ClashMessage);
        }
    }

    /// <summary>Marks a loaded entity as changed, so saving checks and moves its row version on.</summary>
    public static void MarkModified(this PuSpecSheetDbContext db, object entity)
    {
        db.Entry(entity).State = EntityState.Modified;
    }

    /// <summary>Stops tracking an entity, so a later save leaves it alone.</summary>
    public static void Detach(this PuSpecSheetDbContext db, object entity)
    {
        db.Entry(entity).State = EntityState.Detached;
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        // SQL Server: 2601 (duplicate key in a unique index) and 2627 (unique constraint).
        return exception.InnerException is SqlException { Number: 2601 or 2627 };
    }
}
