namespace PUSpecSheet.Abstractions.Requests.Access.Tables;

public record ManageTablePermissionsRequest
{
    public required Guid TableReferenceId { get; init; }

    public required ICollection<CreateTablePermissionRequest> CreateTablePermissionRequests { get; init; }

    public required ICollection<int> DeleteTablePermissionRequests { get; init; }
}
