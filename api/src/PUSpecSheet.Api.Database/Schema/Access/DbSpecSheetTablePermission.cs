namespace PUSpecSheet.Api.Database.Schema.Access;

public record DbSpecSheetTablePermission
{
    public required int Id { get; init; }

    public required int UserId { get; init; }

    public required Guid TableReferenceId { get; init; }

    public required bool IsAdmin { get; init; }

    public virtual DbUser? User { get; init; }
}
