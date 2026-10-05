using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.SheetTypes;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.SheetTypes;

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

    public async Task<SheetTypeDto> CreateAsync(CreateSheetTypeRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await db.SheetTypes.AnyAsync(sheetType => sheetType.Name == name, cancellationToken))
        {
            throw new ConflictException($"A sheet type called \"{name}\" already exists.");
        }

        var lastOrder = await db.SheetTypes.MaxAsync(sheetType => (int?)sheetType.DisplayOrder, cancellationToken);
        var sheetType = new SheetType { Name = name, DisplayOrder = (lastOrder ?? 0) + 1 };

        db.SheetTypes.Add(sheetType);
        await db.SaveChangesAsync(cancellationToken);

        return new SheetTypeDto(sheetType.Id, sheetType.Name, sheetType.DisplayOrder);
    }
}
