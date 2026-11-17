namespace PUSpecSheet.Api.Database.Schema.Access;

public record DbUserPermission
{
    public required int Id { get; init; }

    public required int UserId { get; init; }

    public required int PermissionId { get; set; }

    public virtual DbPermission? Permission { get; set; }

    public virtual DbUser? User { get; set; }
}
