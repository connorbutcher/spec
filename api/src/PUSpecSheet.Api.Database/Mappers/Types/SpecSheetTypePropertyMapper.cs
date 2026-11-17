using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Mappers.Types;

public static class SpecSheetTypePropertyMapper
{
    public static SpecSheetTypeProperty MapToDomain(this DbSpecSheetTypeProperty dbSpecSheetTypeProperty)
    {
        return new SpecSheetTypeProperty
        {
            Id = dbSpecSheetTypeProperty.Id,
            SpecSheetTypeId = dbSpecSheetTypeProperty.SpecSheetTypeId,
            ReferenceId = dbSpecSheetTypeProperty.ReferenceId,
            ValueName = dbSpecSheetTypeProperty.ValueName,
            ValueKey = dbSpecSheetTypeProperty.ValueKey,
            ValueType = dbSpecSheetTypeProperty.ValueType,
            ValueControlType = dbSpecSheetTypeProperty.ValueControlType,
            CanValueBeSetInForm = dbSpecSheetTypeProperty.CanValueBeSetInForm,
            DisplayOrder = dbSpecSheetTypeProperty.DisplayOrder,
            PlaceholderText = dbSpecSheetTypeProperty.PlaceholderText,
            Validators = dbSpecSheetTypeProperty.ValidatorConfiguration,
            SaveValueAsProperty = dbSpecSheetTypeProperty.SaveValueAsProperty

        };
    }

    public static IEnumerable<SpecSheetTypeProperty> MapToDomain(this ICollection<DbSpecSheetTypeProperty> dbSpecSheetTypeProperties)
    {
        return dbSpecSheetTypeProperties.Select(MapToDomain);
    }

    public static IQueryable<SpecSheetTypeProperty> MapToDomain(this IQueryable<DbSpecSheetTypeProperty> dbSpecSheetTypeProperties)
    {
        return dbSpecSheetTypeProperties
            .Select(x => x.MapToDomain());
    }
}
