using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Templates.ColumnBlocks;

public interface ITemplateColumnBlocksService
{
    Task<TableTemplate> CreateAsync(CreateTemplateColumnBlockRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> UpdateAsync(int id, UpdateTemplateColumnBlockRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> DeleteAsync(int id, CancellationToken cancellationToken);
}
