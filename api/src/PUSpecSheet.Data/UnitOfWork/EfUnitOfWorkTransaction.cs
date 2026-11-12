using Microsoft.EntityFrameworkCore.Storage;
using PUSpecSheet.Application.Persistence;

namespace PUSpecSheet.Data.UnitOfWork;

/// <summary>An EF Core transaction behind <see cref="IUnitOfWorkTransaction"/>.</summary>
public sealed class EfUnitOfWorkTransaction : IUnitOfWorkTransaction
{
    private readonly IDbContextTransaction transaction;

    public EfUnitOfWorkTransaction(IDbContextTransaction transaction)
    {
        this.transaction = transaction;
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await transaction.CommitAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await transaction.DisposeAsync();
    }
}
