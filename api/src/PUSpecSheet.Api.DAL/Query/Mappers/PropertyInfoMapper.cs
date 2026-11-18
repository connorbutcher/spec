using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Query.Models.Properties;

namespace PUSpecSheet.Api.DAL.Query.Mappers;

internal static class PropertyInfoMapper
{
    public static SlimPUSpecSheetPropertyInfo MapToSlimPUSpecSheetPropertyInfo(this SheetProperty property)
    {
        return new SlimPUSpecSheetPropertyInfo
        {
            Id = property.SpecSheetTypePropertyId,
            ReferenceId = property.SpecSheetTypeProperty!.ReferenceId,
            ValueKey = property.SpecSheetTypeProperty.ValueKey,
            DisplayOrder = property.SpecSheetTypeProperty.DisplayOrder,
            ValueName = property.SpecSheetTypeProperty.ValueName
        };
    }
}
