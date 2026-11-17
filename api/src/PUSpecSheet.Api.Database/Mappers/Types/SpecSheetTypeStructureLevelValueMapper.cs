using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Mappers.Types;

public static class SpecSheetTypeStructureLevelValueMapper
{
    public static SpecSheetTypeStructureLevelValue MapToDomain(this DbSpecSheetTypeStructureLevelValue dbSpecSheetTypeStructureLevelValue)
    {
        return new SpecSheetTypeStructureLevelValue
        {
            Id = dbSpecSheetTypeStructureLevelValue.Id,
            SpecSheetTypeStructureLevelId = dbSpecSheetTypeStructureLevelValue.SpecSheetTypeStructureLevelId,
            SpecSheetTypePropertyReferenceId = dbSpecSheetTypeStructureLevelValue.SpecSheetTypePropertyReferenceId,
            DisplayOrder = dbSpecSheetTypeStructureLevelValue.DisplayOrder
        };
    }

    public static IEnumerable<SpecSheetTypeStructureLevelValue> MapToDomain(this ICollection<DbSpecSheetTypeStructureLevelValue> dbSpecSheetTypeStructureLevelValues)
    {
        return dbSpecSheetTypeStructureLevelValues.Select(MapToDomain);
    }

    public static IQueryable<SpecSheetTypeStructureLevelValue> MapToDomain(this IQueryable<DbSpecSheetTypeStructureLevelValue> dbSpecSheetTypeStructureLevelValues)
    {
        return dbSpecSheetTypeStructureLevelValues
            .Select(x => x.MapToDomain())
            .OrderBy(x => x.DisplayOrder);
    }
}
