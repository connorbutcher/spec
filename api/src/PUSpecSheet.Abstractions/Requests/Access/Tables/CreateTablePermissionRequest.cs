namespace PUSpecSheet.Abstractions.Requests.Access.Tables;

public record CreateTablePermissionRequest
{
    public required Guid TableReferenceId { get; init; }

    public required int UserId { get; init; }

    public required bool IsAdmin { get; init; }
}
