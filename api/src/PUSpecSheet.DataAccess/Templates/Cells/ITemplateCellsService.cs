using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Templates.Cells;

/// <summary>Changes to a row's cells. Each returns the whole updated template.</summary>
public interface ITemplateCellsService
{
    Task<TableTemplate> CreateAsync(CreateTemplateCellRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> UpdateAsync(int id, UpdateTemplateCellRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> DeleteAsync(int id, CancellationToken cancellationToken);
}
