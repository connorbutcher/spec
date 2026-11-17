using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Templates.Sections;

public interface ITemplateSectionsService
{
    Task<TableTemplate> CreateAsync(CreateTemplateSectionRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> UpdateAsync(int id, UpdateTemplateSectionRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> MoveAsync(int id, MoveRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> DeleteAsync(int id, CancellationToken cancellationToken);
}
