using PUSpecSheet.Abstractions.SheetTypes;

namespace PUSpecSheet.DataAccess.SheetTypes;

public interface ISheetTypesService
{
    Task<IReadOnlyList<SheetType>> GetAllAsync(CancellationToken cancellationToken);

    Task<SheetType> CreateAsync(CreateSheetTypeRequest request, CancellationToken cancellationToken);

    Task<SheetType> AddIdentifierAsync(int sheetTypeId, SaveSheetTypeIdentifierRequest request, CancellationToken cancellationToken);

    Task<SheetType> UpdateIdentifierAsync(int sheetTypeId, int identifierId, SaveSheetTypeIdentifierRequest request, CancellationToken cancellationToken);

    Task<SheetType> DeleteIdentifierAsync(int sheetTypeId, int identifierId, CancellationToken cancellationToken);
}
