using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Contracts.SheetTypes;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.SheetTypes;

public sealed class SheetTypeService(PuSpecSheetDbContext db) : ISheetTypeService
{
    public async Task<IReadOnlyList<SheetTypeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await db.SheetTypes
            .AsNoTracking()
            .OrderBy(sheetType => sheetType.DisplayOrder)
            .ThenBy(sheetType => sheetType.Name)
            .Select(sheetType => new SheetTypeDto(sheetType.Id, sheetType.Name, sheetType.DisplayOrder))
            .ToListAsync(cancellationToken);
    }
}
