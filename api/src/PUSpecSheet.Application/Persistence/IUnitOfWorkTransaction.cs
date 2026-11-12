namespace PUSpecSheet.Application.Persistence;

/// <summary>An open transaction. Disposing it without committing rolls it back.</summary>
public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
