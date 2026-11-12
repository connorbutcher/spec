using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.SheetTypes.Queries;
using PUSpecSheet.Contracts.SheetTypes;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Entities.SheetTypes;

namespace PUSpecSheet.Application.SheetTypes;

public sealed class SheetTypesService : ISheetTypesService
{
    private readonly PuSpecSheetDbContext db;

    public SheetTypesService(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task<IReadOnlyList<SheetTypeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await SheetTypeQueries.ListAsync(db, cancellationToken);
    }

    public async Task<SheetTypeDto> CreateAsync(CreateSheetTypeRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await SheetTypeQueries.NameExistsAsync(db, name, cancellationToken))
        {
            throw new ConflictException($"A sheet type called \"{name}\" already exists.");
        }

        var lastOrder = await SheetTypeQueries.GetLastDisplayOrderAsync(db, cancellationToken);
        var sheetType = new SheetType { Name = name, DisplayOrder = (lastOrder ?? 0) + 1 };

        db.SheetTypes.Add(sheetType);
        await db.SaveChangesAsync(cancellationToken);

        return new SheetTypeDto(sheetType.Id, sheetType.Name, sheetType.DisplayOrder);
    }
}
