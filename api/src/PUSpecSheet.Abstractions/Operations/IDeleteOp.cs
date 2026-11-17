namespace PUSpecSheet.Abstractions.Operations;

/// <summary>
/// Marker interface to identify operations that delete entities.
/// Delete operations are always processed last to avoid conflicts with add/update operations.
/// </summary>
public interface IDeleteOp : IChangeOp
{
}
