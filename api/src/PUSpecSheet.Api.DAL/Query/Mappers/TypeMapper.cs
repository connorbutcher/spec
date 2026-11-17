using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Query.Models;

namespace PUSpecSheet.Api.DAL.Query.Mappers;

internal static class TypeMapper
{
    public static SlimPUSpecSheetType MapToSlimType(this SpecSheetType type)
    {
        return new SlimPUSpecSheetType
        {
            Id = type.Id,
            Type = type.Type,
            DisplayOrder = type.DisplayOrder,
        };
    }

    public static IEnumerable<SlimPUSpecSheetType> MapToSlimTypes(this IEnumerable<SpecSheetType> types)
    {
        return types.Select(MapToSlimType);
    }
}
