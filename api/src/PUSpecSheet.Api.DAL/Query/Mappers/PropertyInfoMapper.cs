using PUSpecSheet.Abstractions.Domain.SpecSheets.Properties;
using PUSpecSheet.Query.Models.Properties;

namespace PUSpecSheet.Api.DAL.Query.Mappers;

internal static class PropertyInfoMapper
{
    public static SlimPUSpecSheetPropertyInfo MapToSlimPUSpecSheetPropertyInfo(this SpecSheetPropertyValue specSheetPropertyValue)
    {
        return new SlimPUSpecSheetPropertyInfo
        {
            Id = specSheetPropertyValue.SpecSheetTypePropertyId,
            ReferenceId = specSheetPropertyValue.SpecSheetTypeProperty.ReferenceId,
            ValueKey = specSheetPropertyValue.SpecSheetTypeProperty.ValueKey,
            DisplayOrder = specSheetPropertyValue.SpecSheetTypeProperty.DisplayOrder,
            ValueName = specSheetPropertyValue.SpecSheetTypeProperty.ValueName
        };
    }
}
