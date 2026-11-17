using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Query.Models.Structure;

namespace PUSpecSheet.Api.DAL.Query.Mappers;

internal static class StructureLevelValueMapper
{
    public static SlimStructureLevelValue MapToSlimStructureLevelValue(this SpecSheetTypeStructureLevelValue structureLevelValue)
    {
        return new SlimStructureLevelValue
        {
            Id = structureLevelValue.Id,
            PropertyReferenceId = structureLevelValue.SpecSheetTypePropertyReferenceId,
            DisplayOrder = structureLevelValue.DisplayOrder
        };
    }

    public static IEnumerable<SlimStructureLevelValue> MapToSlimStructureLevelValues(this IEnumerable<SpecSheetTypeStructureLevelValue> structureLevelValues)
    {
        return structureLevelValues.Select(MapToSlimStructureLevelValue);
    }
}
