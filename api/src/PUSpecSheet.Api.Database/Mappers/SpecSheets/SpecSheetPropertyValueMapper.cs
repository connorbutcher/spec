using PUSpecSheet.Abstractions.Domain.SpecSheets.Properties;
using PUSpecSheet.Api.Database.Mappers.SpecSheets;
using PUSpecSheet.Api.Database.Mappers.Types;
using PUSpecSheet.Api.Database.Schema.SpecSheets;

namespace PUSpecSheet.Api.Database.Mappers.SpecSheets;

public static class SpecSheetPropertyValueMapper
{
    public static SpecSheetPropertyValue MapToDomain(this DbSpecSheetPropertyValue dbInstancePropertyValue)
    {
        return new SpecSheetPropertyValue
        {
            Id = dbInstancePropertyValue.Id,
            SpecSheetId = dbInstancePropertyValue.SpecSheetId,
            SpecSheetTypePropertyId = dbInstancePropertyValue.SpecSheetTypePropertyId,
            SpecSheetTypeProperty = dbInstancePropertyValue.SpecSheetTypeProperty!.MapToDomain(),
            PropertyValue = dbInstancePropertyValue.MapValue()
        };
    }

    public static SpecSheetPropertyValueSummary MapToSummaryDomain(this DbSpecSheetPropertyValue dbInstancePropertyValue)
    {
        return new SpecSheetPropertyValueSummary
        {
            ReferenceId = dbInstancePropertyValue.SpecSheetTypeProperty!.ReferenceId,
            PropertyKey = dbInstancePropertyValue.SpecSheetTypeProperty.ValueKey,
            PropertyName = dbInstancePropertyValue.SpecSheetTypeProperty.ValueName,
            ValueControlType = dbInstancePropertyValue.SpecSheetTypeProperty.ValueControlType,
            PropertyValue = dbInstancePropertyValue.MapValue()
        };
    }

    public static IEnumerable<SpecSheetPropertyValue> MapToDomain(this ICollection<DbSpecSheetPropertyValue> dbInstancePropertyValues)
    {
        return dbInstancePropertyValues.Select(MapToDomain);
    }

    public static IQueryable<SpecSheetPropertyValue> MapToDomain(this IQueryable<DbSpecSheetPropertyValue> dbInstancePropertyValues)
    {
        return dbInstancePropertyValues.Select(x => x.MapToDomain());
    }

    public static IEnumerable<SpecSheetPropertyValueSummary> MapToSummaryDomain(this ICollection<DbSpecSheetPropertyValue> dbInstancePropertyValues)
    {
        return dbInstancePropertyValues.Select(MapToSummaryDomain);
    }

    public static IQueryable<SpecSheetPropertyValueSummary> MapToSummaryDomain(this IQueryable<DbSpecSheetPropertyValue> dbInstancePropertyValues)
    {
        return dbInstancePropertyValues.Select(x => x.MapToSummaryDomain());
    }

    public static IPropertyValue? MapValue(this DbSpecSheetPropertyValue dbInstancePropertyValue)
    {
        var valueType = dbInstancePropertyValue.SpecSheetTypeProperty!.ValueType;
        return valueType switch
        {
            "string" => new StringPropertyValue { Value = dbInstancePropertyValue.PropertyValue },
            "int" => new IntPropertyValue { Value = string.IsNullOrWhiteSpace(dbInstancePropertyValue.PropertyValue) ? null : int.Parse(dbInstancePropertyValue.PropertyValue) },
            "double" => new DoublePropertyValue { Value = string.IsNullOrWhiteSpace(dbInstancePropertyValue.PropertyValue) ? null : double.Parse(dbInstancePropertyValue.PropertyValue) },
            "bool" => new BoolPropertyValue { Value = string.IsNullOrWhiteSpace(dbInstancePropertyValue.PropertyValue) ? null : bool.Parse(dbInstancePropertyValue.PropertyValue) },
            _ => throw new NotSupportedException($"Value type '{valueType}' is not supported."),
        };
    }
}
