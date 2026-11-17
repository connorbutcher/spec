namespace PUSpecSheet.Abstractions.Domain.Access;

public record SpecSheetTablePermission
{
    public required int Id { get; init; }

    public required int UserId { get; init; }

    public required Guid TableReferenceId { get; init; }

    public required bool IsAdmin { get; init; }
}
