using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Common;

// How the services save: turning a clash with someone else's change into a conflict the screen can explain, and
// marking an entity changed so its row version moves on. Entities loaded for editing are tracked, so changing them
// and saving is enough to store the change.
public static class DbContextSaveExtensions
{
    private const string ClashMessage = "Someone else changed this at the same moment. Refresh the sheet and try again.";

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

    // Marks a loaded entity as changed, so saving checks and moves its row version on.
    public static void MarkModified(this PuSpecSheetDbContext db, object entity)
    {
        db.Entry(entity).State = EntityState.Modified;
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        // SQL Server: 2601 (duplicate key in a unique index) and 2627 (unique constraint).
        return exception.InnerException is SqlException { Number: 2601 or 2627 };
    }
}
