using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Templates.Sections;

/// <summary>Changes to a template's sections. Each returns the whole updated template.</summary>
public interface ITemplateSectionsService
{
    Task<TableTemplate> CreateAsync(CreateTemplateSectionRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> UpdateAsync(int id, UpdateTemplateSectionRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a section with its child sections, rows and cells.</summary>
    Task<TableTemplate> DeleteAsync(int id, CancellationToken cancellationToken);
}
