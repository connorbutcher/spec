using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Mappers.Types;

public static class SpecSheetTypeStructureLevelMapper
{
    public static SpecSheetTypeStructureLevel MapToDomain(this DbSpecSheetTypeStructureLevel dbSpecSheetTypeStructureLevel)
    {
        return new SpecSheetTypeStructureLevel
        {
            Id = dbSpecSheetTypeStructureLevel.Id,
            SpecSheetTypeId = dbSpecSheetTypeStructureLevel.SpecSheetTypeId,
            StructureLevelType = dbSpecSheetTypeStructureLevel.StructureLevelType,
            Level = dbSpecSheetTypeStructureLevel.Level,
            SortOrientation = dbSpecSheetTypeStructureLevel.SortOrientation,
            LevelValues = [.. dbSpecSheetTypeStructureLevel.LevelValues.MapToDomain()]
        };
    }

    public static IEnumerable<SpecSheetTypeStructureLevel> MapToDomain(this ICollection<DbSpecSheetTypeStructureLevel> dbSpecSheetTypeStructureLevels)
    {
        return dbSpecSheetTypeStructureLevels.Select(MapToDomain);
    }

    public static IQueryable<SpecSheetTypeStructureLevel> MapToDomain(this IQueryable<DbSpecSheetTypeStructureLevel> dbSpecSheetTypeStructureLevels)
    {
        return dbSpecSheetTypeStructureLevels
            .Select(x => x.MapToDomain())
            .OrderBy(x => x.Level);
    }
}
