using PUSpecSheet.Abstractions.Operations;

namespace PUSpecSheet.Api.DAL.Processors.Base;

/// <summary>
/// Non-generic interface for operation handlers
/// </summary>
internal interface IOperationHandler
{
    Task ApplyAsync(IChangeOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct);
    (int TempId, int RealId)? GetGeneratedId(IChangeOp operation);
    bool HasUnresolvedDependencies(IChangeOp operation, IReadOnlyDictionary<int, int> tempIdMap);
}

/// <summary>
/// Generic interface for strongly-typed handler implementations.
/// </summary>
internal interface IOperationHandler<in TOperation> : IOperationHandler where TOperation : IChangeOp
{
    Task ApplyAsync(TOperation operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct);
    (int TempId, int RealId)? GetGeneratedId(TOperation operation);
    bool HasUnresolvedDependencies(TOperation operation, IReadOnlyDictionary<int, int> tempIdMap);

    // Explicit interface implementations for non-generic version
    Task IOperationHandler.ApplyAsync(IChangeOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        return ApplyAsync((TOperation)operation, tempIdMap, ct);
    }

    (int TempId, int RealId)? IOperationHandler.GetGeneratedId(IChangeOp operation)
    {
        return GetGeneratedId((TOperation)operation);
    }

    bool IOperationHandler.HasUnresolvedDependencies(IChangeOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return HasUnresolvedDependencies((TOperation)operation, tempIdMap);
    }
}
