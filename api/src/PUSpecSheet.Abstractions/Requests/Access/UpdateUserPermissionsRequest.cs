namespace PUSpecSheet.Abstractions.Requests.Access;

public sealed record UpdateUserPermissionsRequest(int UserId, IReadOnlyList<int> PermissionIdsToAdd, IReadOnlyList<int> PermissionIdsToRemove);
