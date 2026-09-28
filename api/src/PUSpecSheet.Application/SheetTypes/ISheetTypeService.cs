using PUSpecSheet.Contracts.SheetTypes;

namespace PUSpecSheet.Application.SheetTypes;

public interface ISheetTypeService
{
    /// <summary>Every sheet type, in display order.</summary>
    Task<IReadOnlyList<SheetTypeDto>> GetAllAsync(CancellationToken cancellationToken);
}
