using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Schema.Access;

public record DbSpecSheetTypePermission
{
    public required int Id { get; init; }

    public required int PermissionId { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public virtual DbPermission? Permission { get; init; }

    public virtual DbSpecSheetType? SpecSheetType { get; init; }
}
