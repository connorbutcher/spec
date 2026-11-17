using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Template;

namespace PUSpecSheet.Api.Database.Schema.Types;

public record DbSpecSheetType
{
    public required int Id { get; init; }

    public required string Type { get; set; }

    public required int DisplayOrder { get; set; }

    public required bool CanCopyFromMigrated { get; set; }

    public required bool RequiresApprovalWorkflow { get; set; }

    public required string TableOrientation { get; init; }

    public required virtual ICollection<DbTableTemplate> TableTemplates { get; init; } = [];

    public required virtual ICollection<DbSpecSheetPhase> Phases { get; init; } = [];

    public required virtual ICollection<DbSpecSheetTypePermission> Permissions { get; init; } = [];

    public required virtual ICollection<DbSpecSheetTypeProperty> Properties { get; init; } = [];

    public required virtual ICollection<DbSpecSheetTypeStructureLevel> StructureLevels { get; init; } = [];
}
