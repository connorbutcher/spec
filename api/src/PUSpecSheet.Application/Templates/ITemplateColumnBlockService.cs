using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Application.Templates;

/// <summary>Changes to a horizontal template's column blocks. Each returns the whole updated template.</summary>
public interface ITemplateColumnBlockService
{
    /// <summary>Adds a block after the others, with one cell in every row to start with.</summary>
    Task<TableTemplateDto> CreateAsync(CreateTemplateColumnBlockRequest request, CancellationToken cancellationToken);

    Task<TableTemplateDto> UpdateAsync(int id, UpdateTemplateColumnBlockRequest request, CancellationToken cancellationToken);

    Task<TableTemplateDto> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a block with its cells in every row.</summary>
    Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken);
}
