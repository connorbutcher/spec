namespace PUSpecSheet.Api.Database.Schema.Access;

public record DbUser
{
    public required int Id { get; init; }

    public required Guid ObjectId { get; init; }

    public required string DisplayName { get; set; }

    public required string EmailAddress { get; set; }

    public virtual ICollection<DbUserPermission> Permissions { get; init; } = [];

    public virtual ICollection<DbSpecSheetTablePermission> TablePermissions { get; init; } = [];
}
