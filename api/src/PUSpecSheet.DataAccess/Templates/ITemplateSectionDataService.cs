using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates;

public interface ITemplateSectionDataService
{
    Task<List<DbTemplateSection>> ListAsync(int versionId, CancellationToken cancellationToken);

    Task<List<DbTemplateSection>> ListWithRowsAsync(int versionId, CancellationToken cancellationToken);

    Task<List<DbTemplateSection>> ListForEditAsync(int versionId, CancellationToken cancellationToken);

    Task<List<DbTemplateSection>> ListSiblingsForEditAsync(int versionId, int? parentSectionId, CancellationToken cancellationToken);

    Task<DbTemplateSection?> FindForEditAsync(int id, CancellationToken cancellationToken);

    Task<int?> FindVersionIdAsync(int id, CancellationToken cancellationToken);

    Task<SectionRole?> FindRoleAsync(int versionId, int id, CancellationToken cancellationToken);

    Task<int?> GetLastDisplayOrderAsync(int versionId, int? parentSectionId, CancellationToken cancellationToken);

    Task<List<DbTemplateSection>> ListForLayoutAsync(int versionId, CancellationToken cancellationToken);

    Task<List<DbTemplateSection>> ListInVersionsAsync(IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken);
}
