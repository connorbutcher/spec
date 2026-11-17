using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Database.Models.CellTypes;

namespace PUSpecSheet.Database.Mappers.CellTypes;

/// <summary>Cell types and their options as DTOs.</summary>
public static class CellTypeMapper
{
    /// <summary>Expects <see cref="DbCellType.Options"/> to be loaded.</summary>
    /// <param name="cellType">The cell type.</param>
    /// <param name="usageCount">How many template cells use it, which the database doesn't keep on the cell type.</param>
    public static CellTypeDto ToDto(this DbCellType cellType, int usageCount)
    {
        return new CellTypeDto(
            cellType.Id,
            cellType.Name,
            cellType.Kind,
            cellType.Description,
            cellType.DisplayOrder,
            cellType.Configuration,
            cellType.Style,
            cellType.Options
                .OrderBy(option => option.DisplayOrder)
                .Select(option => option.ToDto())
                .ToList(),
            usageCount);
    }

    public static CellTypeOptionDto ToDto(this DbCellTypeOption option)
    {
        return new CellTypeOptionDto(option.Id, option.Value, option.DisplayOrder);
    }
}
