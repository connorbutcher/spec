using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Abstractions.Domain.Types;

public record SpecSheetTypeStructureLevel
{
    public required int Id { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public required StructureLevelType StructureLevelType { get; init; }

    public required int Level { get; set; }

    public required int SortOrientation { get; set; }

    public required ICollection<SpecSheetTypeStructureLevelValue> LevelValues { get; set; }
}
