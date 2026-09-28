using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Application.Templates;

public interface ITableTemplateService
{
    /// <summary>Templates without their sections, optionally for one sheet type, in display order.</summary>
    Task<IReadOnlyList<TableTemplateSummaryDto>> GetSummariesAsync(int? sheetTypeId, CancellationToken cancellationToken);

    Task<TableTemplateDto> GetAsync(int id, CancellationToken cancellationToken);

    Task<TableTemplateDto> CreateAsync(CreateTableTemplateRequest request, CancellationToken cancellationToken);

    Task<TableTemplateDto> UpdateAsync(int id, UpdateTableTemplateRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a template with all of its sections, rows and cells.</summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
