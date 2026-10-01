using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Sheets;

internal static class SheetDbContextExtensions
{
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
            throw new ConflictException("Someone else changed this at the same moment. Refresh the sheet and try again.");
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new ConflictException("Someone else changed this at the same moment. Refresh the sheet and try again.");
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        // SQL Server: 2601 (duplicate key in a unique index) and 2627 (unique constraint).
        return exception.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 };
    }
}
