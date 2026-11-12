using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence.CellTypes;

namespace PUSpecSheet.Application.Templates;

/// <summary>Picks the cell type new cells get when none is chosen: the first Text type, else the first type.</summary>
internal static class DefaultCellType
{
    public static async Task<int> GetIdAsync(ICellTypeRepository cellTypes, CancellationToken cancellationToken)
    {
        var id = await cellTypes.FindDefaultIdAsync(cancellationToken);

        if (id is null)
        {
            throw new ConflictException("There are no cell types yet. Add a cell type first.");
        }

        return id.Value;
    }
}
