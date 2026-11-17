using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates;

public interface ITemplateRowDataService
{
    Task<List<DbTemplateRow>> ListWithCellsAsync(int versionId, CancellationToken cancellationToken);

    Task<List<int>> ListIdsAsync(int versionId, CancellationToken cancellationToken);

    Task<List<DbTemplateRow>> ListInSectionWithCellsForEditAsync(int sectionId, CancellationToken cancellationToken);

    Task<List<DbTemplateRow>> ListInSectionForEditAsync(int sectionId, CancellationToken cancellationToken);

    Task<DbTemplateRow?> FindForEditAsync(int id, CancellationToken cancellationToken);

    Task<TemplateCellTarget?> FindCellTargetAsync(int rowId, int? columnBlockId, CancellationToken cancellationToken);

    Task<List<DbTemplateRow>> ListWithCellsInVersionsAsync(IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken);
}
