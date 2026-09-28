using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Application.Templates;

/// <summary>Changes to a template's sections. Each returns the whole updated template.</summary>
public interface ITemplateSectionService
{
    Task<TableTemplateDto> CreateAsync(CreateTemplateSectionRequest request, CancellationToken cancellationToken);

    Task<TableTemplateDto> UpdateAsync(int id, UpdateTemplateSectionRequest request, CancellationToken cancellationToken);

    Task<TableTemplateDto> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a section with its child sections, rows and cells.</summary>
    Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken);
}
