using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Templates;

/// <summary>Picks the cell type new cells get when none is chosen: the first Text type, else the first type.</summary>
internal static class DefaultCellType
{
    public static async Task<int> GetIdAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        var id = await db.CellTypes
            .OrderBy(cellType => cellType.Kind == CellKind.Text ? 0 : 1)
            .ThenBy(cellType => cellType.DisplayOrder)
            .Select(cellType => (int?)cellType.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (id is null)
        {
            throw new ConflictException("There are no cell types yet. Add a cell type first.");
        }

        return id.Value;
    }
}
