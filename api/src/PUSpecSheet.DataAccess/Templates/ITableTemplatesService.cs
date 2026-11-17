using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Templates;

public interface ITableTemplatesService
{
    Task<IReadOnlyList<TableTemplateSummary>> GetSummariesAsync(int? sheetTypeId, CancellationToken cancellationToken);

    Task<TableTemplate> GetAsync(int id, int? versionNumber, CancellationToken cancellationToken);

    Task<TableTemplate> CreateAsync(CreateTableTemplateRequest request, CancellationToken cancellationToken);

    // Changing orientation needs the latest version to be editable; renaming doesn't.
    Task<TableTemplate> UpdateAsync(int id, UpdateTableTemplateRequest request, CancellationToken cancellationToken);

    Task<TableTemplate> CreateVersionAsync(int id, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
