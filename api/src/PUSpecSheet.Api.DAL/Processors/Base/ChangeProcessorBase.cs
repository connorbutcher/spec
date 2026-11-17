using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Base;

/// <summary>
/// Processes and applies changes to process versions by handling operation dependencies and managing database transactions.
/// </summary>
internal abstract class ChangeProcessorBase
{
    protected readonly PUSpecSheetDbContext DbContext;
    protected readonly User User;
    protected readonly ILogger Logger;
    private readonly Dictionary<IChangeOp, IOperationHandler> _operationHandlers = [];

    public ChangeProcessorBase(PUSpecSheetDbContext dbContext, User user, ILogger logger)
    {
        DbContext = dbContext;
        User = user;
        Logger = logger;
    }

    /// <summary>
    /// Applies a collection of process version change operations in waves based on dependency resolution.
    /// </summary>
    public async Task ApplyChangesAsync(IChangeOp[] operations, CancellationToken cancellationToken)
    {
        using var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);

        _operationHandlers.Clear();
        foreach (var operation in operations)
        {
            _operationHandlers[operation] = CreateHandler(operation);
        }

        var (nonDeleteOps, deleteOps) = PartitionByDeleteOperations(operations);
        var tempIdMap = new Dictionary<int, int>();

        // Process non-delete operations first
        if (nonDeleteOps.Count > 0)
        {
            await ProcessOperationsInWavesAsync(nonDeleteOps, tempIdMap, cancellationToken);
        }

        // Process delete operations last (no waves needed - they have no interdependencies)
        if (deleteOps.Count > 0)
        {
            foreach (var operation in deleteOps)
            {
                var handler = GetHandler(operation);
                await handler.ApplyAsync(operation, tempIdMap, cancellationToken);
            }

            await DbContext.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    protected abstract IOperationHandler CreateHandler(IChangeOp operation);

    /// <summary>
    /// Processes operations in dependency-resolved waves until all are applied.
    /// </summary>
    private async Task ProcessOperationsInWavesAsync(List<IChangeOp> operations, Dictionary<int, int> tempIdMap, CancellationToken cancellationToken)
    {
        var remaining = operations;
        var waveCounter = 0;

        while (remaining.Count > 0)
        {
            waveCounter++;
            var (readyOps, deferredOps) = PartitionByReadiness(remaining, tempIdMap);

            if (readyOps.Count == 0)
            {
                var unresolved = deferredOps.Select(op => op.GetType().Name);
                throw new InvalidOperationException(
                    $"Circular dependency or unresolvable temporary IDs detected in operations: {string.Join(", ", unresolved)}");
            }

            // Apply all ready operations
            foreach (var operation in readyOps)
            {
                var handler = GetHandler(operation);
                await handler.ApplyAsync(operation, tempIdMap, cancellationToken);
            }

            // Save to get database-generated IDs
            await DbContext.SaveChangesAsync(cancellationToken);

            // Update temp ID map with real IDs
            foreach (var operation in readyOps)
            {
                var handler = GetHandler(operation);
                var generatedId = handler.GetGeneratedId(operation);
                if (generatedId.HasValue)
                {
                    tempIdMap[generatedId.Value.TempId] = generatedId.Value.RealId;
                }
            }

            remaining = deferredOps;
        }

        Logger.LogInformation("Processed {OperationCount} operations in {WaveCount} waves.", operations.Count, waveCounter);
    }

    /// <summary>
    /// Separates delete operations from non-delete operations using the IDeleteOperation marker interface.
    /// </summary>
    private static (List<IChangeOp> NonDeletes, List<IChangeOp> Deletes) PartitionByDeleteOperations(
        IChangeOp[] operations)
    {
        var nonDeletes = new List<IChangeOp>();
        var deletes = new List<IChangeOp>();

        foreach (var operation in operations)
        {
            if (operation is IDeleteOp)
            {
                deletes.Add(operation);
            }
            else
            {
                nonDeletes.Add(operation);
            }
        }

        return (nonDeletes, deletes);
    }

    /// <summary>
    /// Partitions operations into ready and deferred groups based on dependency resolution.
    /// </summary>
    /// <param name="operations">The list of operations to partition.</param>
    /// <param name="tempIdMap">The mapping of temporary IDs to real database-generated IDs.</param>
    /// <returns>A tuple containing the list of ready operations and the list of deferred operations.</returns>
    private (List<IChangeOp> Ready, List<IChangeOp> Deferred) PartitionByReadiness(
        List<IChangeOp> operations,
        IReadOnlyDictionary<int, int> tempIdMap)
    {
        var ready = new List<IChangeOp>();
        var deferred = new List<IChangeOp>();

        foreach (var operation in operations)
        {
            var handler = GetHandler(operation);
            if (handler.HasUnresolvedDependencies(operation, tempIdMap))
            {
                deferred.Add(operation);
            }
            else
            {
                ready.Add(operation);
            }
        }

        return (ready, deferred);
    }

    private IOperationHandler GetHandler(IChangeOp operation)
    {
        return _operationHandlers[operation];
    }
}
