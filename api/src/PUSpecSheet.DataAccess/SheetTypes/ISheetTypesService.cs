using PUSpecSheet.Abstractions.SheetTypes;

namespace PUSpecSheet.DataAccess.SheetTypes;

public interface ISheetTypesService
{
    /// <summary>Every sheet type, in display order.</summary>
    Task<IReadOnlyList<SheetTypeDto>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Adds a sheet type to the end of the list. No phase has it until it is made available to one.</summary>
    Task<SheetTypeDto> CreateAsync(CreateSheetTypeRequest request, CancellationToken cancellationToken);

    /// <summary>Adds an identifier to the end of the sheet type's list and returns the sheet type.</summary>
    Task<SheetTypeDto> AddIdentifierAsync(int sheetTypeId, SaveSheetTypeIdentifierRequest request, CancellationToken cancellationToken);

    /// <summary>Changes an identifier's name, kind, options or whether it is required, and returns the sheet type.</summary>
    Task<SheetTypeDto> UpdateIdentifierAsync(int sheetTypeId, int identifierId, SaveSheetTypeIdentifierRequest request, CancellationToken cancellationToken);

    /// <summary>Removes an identifier no sheet has a value for, and returns the sheet type.</summary>
    Task<SheetTypeDto> DeleteIdentifierAsync(int sheetTypeId, int identifierId, CancellationToken cancellationToken);
}
