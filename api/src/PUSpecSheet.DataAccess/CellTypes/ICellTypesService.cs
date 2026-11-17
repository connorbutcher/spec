using PUSpecSheet.Abstractions.CellTypes;

namespace PUSpecSheet.DataAccess.CellTypes;

public interface ICellTypesService
{
    Task<IReadOnlyList<CellTypeDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<CellTypeDto> CreateAsync(SaveCellTypeRequest request, CancellationToken cancellationToken);

    Task<CellTypeDto> UpdateAsync(int id, SaveCellTypeRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
