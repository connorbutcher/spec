using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Query.Models.Structure;

namespace PUSpecSheet.Api.DAL.Query.Mappers;

internal static class StructureLevelMapper
{
    public static SlimStructureLevel MapToSlimStructureLevel(this SpecSheetTypeStructureLevel structureLevel)
    {
        return new SlimStructureLevel
        {
            Id = structureLevel.Id,
            SortOrientation = structureLevel.SortOrientation,
            LevelValues = [.. structureLevel.LevelValues.MapToSlimStructureLevelValues()]
        };
    }
}
