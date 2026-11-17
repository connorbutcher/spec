using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.Template;

namespace PUSpecSheet.Abstractions.Domain.Types;

public record SpecSheetType
{
    public required int Id { get; init; }

    public required string Type { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool CanCopyFromMigrated { get; init; }

    public required bool RequiresApprovalWorkflow { get; init; }

    public required string TableOrientation { get; init; }

    public required ICollection<TableTemplate> TableTemplates { get; init; }

    public required ICollection<SpecSheetTypePermission> Permissions { get; init; }

    public required ICollection<SpecSheetTypeProperty> Properties { get; init; }

    public required ICollection<SpecSheetTypeStructureLevel> StructureLevels { get; init; }
}
