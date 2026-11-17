namespace PUSpecSheet.Abstractions.Domain.Access;

public record SpecSheetTypePermission
{
    public required int Id { get; init; }

    public required int PermissionId { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public Permission? Permission { get; init; }
}
