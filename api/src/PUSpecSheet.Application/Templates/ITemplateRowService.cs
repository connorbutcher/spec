using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Application.Templates;

/// <summary>Changes to a section's rows. Each returns the whole updated template.</summary>
public interface ITemplateRowService
{
    Task<TableTemplateDto> CreateAsync(CreateTemplateRowRequest request, CancellationToken cancellationToken);

    Task<TableTemplateDto> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a row with its cells.</summary>
    Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken);
}
