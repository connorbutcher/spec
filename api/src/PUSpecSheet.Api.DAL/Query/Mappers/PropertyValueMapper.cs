using System.Globalization;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Query.Models.Properties;
using PUSpecSheet.Query.Models.Values;

namespace PUSpecSheet.Api.DAL.Query.Mappers;

internal static class PropertyValueMapper
{
    public static SlimPUSpecSheetPropertyValue MapToSlimPropertyValue(this SheetProperty property)
    {
        return new SlimPUSpecSheetPropertyValue
        {
            Id = property.Id,
            PropertyInfo = property.MapToSlimPUSpecSheetPropertyInfo(),
            Value = property.MapToSlimValue()
        };
    }

    public static IEnumerable<SlimPUSpecSheetPropertyValue> MapToSlimPropertyValues(this IEnumerable<SheetProperty> properties)
    {
        return properties.Select(MapToSlimPropertyValue);
    }

    public static SlimPUSpecSheetPropertyValueSummary MapToSlimPropertyValueSummary(this SheetProperty property)
    {
        var typeProperty = property.SpecSheetTypeProperty!;
        return new SlimPUSpecSheetPropertyValueSummary
        {
            ReferenceId = typeProperty.ReferenceId,
            PropertyKey = typeProperty.ValueKey,
            PropertyName = typeProperty.ValueName,
            ValueControlType = typeProperty.ValueControlType,
            Value = property.MapToSlimValue()
        };
    }

    public static IEnumerable<SlimPUSpecSheetPropertyValueSummary> MapToSlimPropertyValueSummaries(this IEnumerable<SheetProperty> properties)
    {
        return properties.Select(MapToSlimPropertyValueSummary);
    }

    // The value is stored as text, and read as the kind the property's type says it is.
    private static SlimValue MapToSlimValue(this SheetProperty property)
    {
        var valueType = property.SpecSheetTypeProperty!.ValueType;
        var text = property.Value;
        var isEmpty = string.IsNullOrWhiteSpace(text);
        return valueType switch
        {
            "string" => new SlimTextValue { Id = property.Id, Value = text },
            "int" => new SlimIntValue { Id = property.Id, Value = isEmpty ? null : int.Parse(text!, CultureInfo.InvariantCulture) },
            "double" => new SlimDoubleValue { Id = property.Id, Value = isEmpty ? null : double.Parse(text!, CultureInfo.InvariantCulture) },
            "bool" => new SlimBoolValue { Id = property.Id, Value = isEmpty ? null : bool.Parse(text!) },
            "datetime" => new SlimDateTimeValue { Id = property.Id, Value = isEmpty ? null : DateTime.Parse(text!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) },
            _ => throw new NotSupportedException($"Value type '{valueType}' is not supported."),
        };
    }
}
