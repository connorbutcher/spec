using PUSpecSheet.Contracts.CellTypes;

namespace PUSpecSheet.Application.CellTypes;

public interface ICellTypeService
{
    /// <summary>Every cell type in display order, with its options and how many cells use it.</summary>
    Task<IReadOnlyList<CellTypeDto>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Adds a cell type to the end of the list.</summary>
    Task<CellTypeDto> CreateAsync(SaveCellTypeRequest request, CancellationToken cancellationToken);

    Task<CellTypeDto> UpdateAsync(int id, SaveCellTypeRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a cell type that no cell uses.</summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
