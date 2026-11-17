using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Templates.ColumnBlocks;

/// <summary>Changes to a horizontal template's column blocks. Each returns the whole updated template.</summary>
public interface ITemplateColumnBlocksService
{
    /// <summary>Adds a block after the others, with one cell in every row to start with.</summary>
    Task<TableTemplate> CreateAsync(CreateTemplateColumnBlockRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> UpdateAsync(int id, UpdateTemplateColumnBlockRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a block with its cells in every row.</summary>
    Task<TableTemplate> DeleteAsync(int id, CancellationToken cancellationToken);
}
