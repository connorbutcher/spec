using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;

namespace PUSpecSheet.Data.UnitOfWork;

/// <summary>The <see cref="IUnitOfWork"/> over the request's <see cref="PuSpecSheetDbContext"/>.</summary>
public sealed class EfUnitOfWork : IUnitOfWork
{
    private const string ClashMessage = "Someone else changed this at the same moment. Refresh the sheet and try again.";

    private readonly PuSpecSheetDbContext db;

    public EfUnitOfWork(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public bool HasTransaction => db.Database.CurrentTransaction is not null;

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken)
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

    public async Task SaveSheetChangesAsync(CancellationToken cancellationToken)
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

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return new EfUnitOfWorkTransaction(await db.Database.BeginTransactionAsync(cancellationToken));
    }

    public void MarkModified(object entity)
    {
        db.Entry(entity).State = EntityState.Modified;
    }

    public void Detach(object entity)
    {
        db.Entry(entity).State = EntityState.Detached;
    }

    public void ClearTracking()
    {
        db.ChangeTracker.Clear();
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        // SQL Server: 2601 (duplicate key in a unique index) and 2627 (unique constraint).
        return exception.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 };
    }
}
