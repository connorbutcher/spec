using Microsoft.EntityFrameworkCore.Storage;
using PUSpecSheet.Application.Persistence;

namespace PUSpecSheet.Data.UnitOfWork;

/// <summary>An EF Core transaction behind <see cref="IUnitOfWorkTransaction"/>.</summary>
public sealed class EfUnitOfWorkTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
{
    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await transaction.CommitAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await transaction.DisposeAsync();
    }
}
