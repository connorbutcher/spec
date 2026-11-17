using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates;

public interface ITableTemplateDataService
{
    Task<List<TableTemplateSummary>> ListSummariesAsync(int? sheetTypeId, CancellationToken cancellationToken);

    Task<DbTableTemplate?> FindAsync(int id, CancellationToken cancellationToken);

    Task<DbTableTemplate?> FindForEditAsync(int id, CancellationToken cancellationToken);

    Task<List<DbTableTemplate>> ListForEditAsync(int sheetTypeId, CancellationToken cancellationToken);

    Task<bool> NameTakenAsync(int sheetTypeId, string name, int? exceptId, CancellationToken cancellationToken);

    Task<int?> GetLastDisplayOrderAsync(int sheetTypeId, CancellationToken cancellationToken);

    Task<bool> IsUsedAsync(int templateId, CancellationToken cancellationToken);

    Task<List<DbTableTemplateVersion>> ListVersionsAsync(int templateId, CancellationToken cancellationToken);

    Task<DbTableTemplateVersion> GetLatestVersionForEditAsync(int templateId, CancellationToken cancellationToken);

    Task<TemplateVersionKey?> FindVersionKeyAsync(int versionId, CancellationToken cancellationToken);

    Task<TemplateVersionState?> FindVersionStateAsync(int versionId, CancellationToken cancellationToken);

    Task<TemplateOrientation> GetOrientationAsync(int versionId, CancellationToken cancellationToken);

    Task<List<int>> GetVersionIdsInUseAsync(IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken);

    Task<DbTableTemplate?> FindForSheetTypeAsync(int id, int sheetTypeId, CancellationToken cancellationToken);

    Task<List<DbTableTemplate>> ListForSheetTypeAsync(int sheetTypeId, CancellationToken cancellationToken);

    Task<DbTableTemplateVersion> GetLatestVersionAsync(int templateId, CancellationToken cancellationToken);

    Task<Dictionary<int, int>> GetLatestVersionNumbersAsync(IReadOnlyCollection<int> templateIds, CancellationToken cancellationToken);
}
