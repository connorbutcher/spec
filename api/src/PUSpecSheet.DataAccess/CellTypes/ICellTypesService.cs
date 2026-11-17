using PUSpecSheet.Abstractions.CellTypes;

namespace PUSpecSheet.DataAccess.CellTypes;

public interface ICellTypesService
{
    Task<IReadOnlyList<CellType>> GetAllAsync(CancellationToken cancellationToken);

    Task<CellType> CreateAsync(SaveCellTypeRequest request, CancellationToken cancellationToken);

    Task<CellType> UpdateAsync(int id, SaveCellTypeRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
