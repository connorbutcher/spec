using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Templates.Cells;

public interface ITemplateCellsService
{
    Task<TableTemplate> CreateAsync(CreateTemplateCellRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> UpdateAsync(int id, UpdateTemplateCellRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> DeleteAsync(int id, CancellationToken cancellationToken);
}
