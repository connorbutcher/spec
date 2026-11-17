namespace PUSpecSheet.Abstractions.Domain.Access;

public sealed record User
{
    public required int Id { get; init; }

    public required Guid ObjectId { get; init; }

    public required string DisplayName { get; init; }

    public required string EmailAddress { get; init; }

    public required ICollection<UserPermission> Permissions { get; init; }

    public required ICollection<SpecSheetTablePermission> TablePermissions { get; init; }
}
