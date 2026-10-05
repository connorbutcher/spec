using PUSpecSheet.Contracts.SheetTypes;

namespace PUSpecSheet.Application.SheetTypes;

public interface ISheetTypeService
{
    /// <summary>Every sheet type, in display order.</summary>
    Task<IReadOnlyList<SheetTypeDto>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Adds a sheet type to the end of the list. No phase has it until it is made available to one.</summary>
    Task<SheetTypeDto> CreateAsync(CreateSheetTypeRequest request, CancellationToken cancellationToken);
}
