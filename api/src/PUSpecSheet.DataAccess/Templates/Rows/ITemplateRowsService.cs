using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Templates.Rows;

/// <summary>Changes to a section's rows. Each returns the whole updated template.</summary>
public interface ITemplateRowsService
{
    Task<TableTemplate> CreateAsync(CreateTemplateRowRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a row with its cells.</summary>
    Task<TableTemplate> DeleteAsync(int id, CancellationToken cancellationToken);
}
