using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Api.Database.Schema.Types;

public record DbSpecSheetTypeStructureLevel
{
    public required int Id { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public required StructureLevelType StructureLevelType { get; init; }

    public required int Level { get; set; }

    public required int SortOrientation { get; set; }

    public virtual ICollection<DbSpecSheetTypeStructureLevelValue> LevelValues { get; set; } = [];
}
