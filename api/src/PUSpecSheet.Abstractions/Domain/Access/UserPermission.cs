namespace PUSpecSheet.Abstractions.Domain.Access;

public sealed record UserPermission
{
    public required int Id { get; init; }

    public required int UserId { get; init; }

    public required int PermissionId { get; init; }

    public required Permission? Permission { get; init; }
}
