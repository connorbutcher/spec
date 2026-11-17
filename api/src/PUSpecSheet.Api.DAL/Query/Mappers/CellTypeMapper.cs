using PUSpecSheet.Abstractions.Domain.Template;
using PUSpecSheet.Query.Models.Table.Cell;

namespace PUSpecSheet.Api.DAL.Query.Mappers;

internal static class CellTypeMapper
{
    public static SlimPUSpecSheetTableCellType MapToSlimType(this TableCellType type)
    {
        return new SlimPUSpecSheetTableCellType
        {
            Id = type.Id,
            Key = type.Key,
            Title = type.Title,
            Type = type.Type,
        };
    }
}
