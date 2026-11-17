namespace PUSpecSheet.Api.Database.Schema.Access;

public record DbPermission
{
    public required int Id { get; init; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required string Key { get; set; }

    public virtual ICollection<DbUserPermission> UserPermissions { get; set; } = [];
}
