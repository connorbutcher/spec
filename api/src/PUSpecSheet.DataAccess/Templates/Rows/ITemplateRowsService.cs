using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Templates.Rows;

public interface ITemplateRowsService
{
    Task<TableTemplate> CreateAsync(CreateTemplateRowRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> DeleteAsync(int id, CancellationToken cancellationToken);
}
