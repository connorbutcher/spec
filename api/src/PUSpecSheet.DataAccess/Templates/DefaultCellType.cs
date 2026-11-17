using PUSpecSheet.DataAccess.CellTypes.Queries;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Templates;

/// <summary>Picks the cell type new cells get when none is chosen: the first Text type, else the first type.</summary>
internal static class DefaultCellType
{
    public static async Task<int> GetIdAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        var id = await CellTypeQueries.FindDefaultIdAsync(db, cancellationToken);

        if (id is null)
        {
            throw new ConflictException("There are no cell types yet. Add a cell type first.");
        }

        return id.Value;
    }
}
