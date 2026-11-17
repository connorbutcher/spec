using PUSpecSheet.Abstractions.SheetTypes;

namespace PUSpecSheet.DataAccess.SheetTypes;

public interface ISheetTypesService
{
    Task<IReadOnlyList<SheetTypeDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<SheetTypeDto> CreateAsync(CreateSheetTypeRequest request, CancellationToken cancellationToken);

    Task<SheetTypeDto> AddIdentifierAsync(int sheetTypeId, SaveSheetTypeIdentifierRequest request, CancellationToken cancellationToken);

    Task<SheetTypeDto> UpdateIdentifierAsync(int sheetTypeId, int identifierId, SaveSheetTypeIdentifierRequest request, CancellationToken cancellationToken);

    Task<SheetTypeDto> DeleteIdentifierAsync(int sheetTypeId, int identifierId, CancellationToken cancellationToken);
}
