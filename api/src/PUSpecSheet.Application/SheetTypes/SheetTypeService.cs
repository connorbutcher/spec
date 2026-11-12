using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.SheetTypes;
using PUSpecSheet.Contracts.SheetTypes;
using PUSpecSheet.Domain.SheetTypes;

namespace PUSpecSheet.Application.SheetTypes;

public sealed class SheetTypeService(ISheetTypeRepository sheetTypes, IUnitOfWork unitOfWork) : ISheetTypeService
{
    public async Task<IReadOnlyList<SheetTypeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await sheetTypes.ListAsync(cancellationToken);
    }

    public async Task<SheetTypeDto> CreateAsync(CreateSheetTypeRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await sheetTypes.NameExistsAsync(name, cancellationToken))
        {
            throw new ConflictException($"A sheet type called \"{name}\" already exists.");
        }

        var lastOrder = await sheetTypes.GetLastDisplayOrderAsync(cancellationToken);
        var sheetType = new SheetType { Name = name, DisplayOrder = (lastOrder ?? 0) + 1 };

        sheetTypes.Add(sheetType);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SheetTypeDto(sheetType.Id, sheetType.Name, sheetType.DisplayOrder);
    }
}
