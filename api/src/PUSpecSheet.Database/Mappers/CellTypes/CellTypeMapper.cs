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
        return new CellTypeDto
        {
            Id = cellType.Id,
            Name = cellType.Name,
            Kind = cellType.Kind,
            Description = cellType.Description,
            DisplayOrder = cellType.DisplayOrder,
            Configuration = cellType.Configuration,
            Style = cellType.Style,
            Options = cellType.Options
                .OrderBy(option => option.DisplayOrder)
                .Select(option => option.ToDto())
                .ToList(),
            UsageCount = usageCount,
        };
    }

    public static CellTypeOptionDto ToDto(this DbCellTypeOption option)
    {
        return new CellTypeOptionDto { Id = option.Id, Value = option.Value, DisplayOrder = option.DisplayOrder };
    }
}
