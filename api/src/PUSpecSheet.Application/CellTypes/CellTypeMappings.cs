using PUSpecSheet.Contracts.CellTypes;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.CellTypes;

internal static class CellTypeMappings
{
    /// <summary>Maps a cell type to its DTO. Expects <see cref="CellType.Options"/> to be loaded.</summary>
    public static CellTypeDto ToDto(this CellType cellType, int usageCount)
    {
        var options = cellType.Options
            .OrderBy(option => option.DisplayOrder)
            .Select(option => new CellTypeOptionDto(option.Id, option.Value, option.DisplayOrder))
            .ToList();

        return new CellTypeDto(
            cellType.Id,
            cellType.Name,
            cellType.Kind,
            cellType.Description,
            cellType.DisplayOrder,
            cellType.Configuration,
            cellType.Style,
            options,
            usageCount);
    }
}
