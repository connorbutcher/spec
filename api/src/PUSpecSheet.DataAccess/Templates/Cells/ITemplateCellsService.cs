using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Templates.Cells;

/// <summary>Changes to a row's cells. Each returns the whole updated template.</summary>
public interface ITemplateCellsService
{
    Task<TableTemplateDto> CreateAsync(CreateTemplateCellRequest request, CancellationToken cancellationToken);

    Task<TableTemplateDto> UpdateAsync(int id, UpdateTemplateCellRequest request, CancellationToken cancellationToken);

    Task<TableTemplateDto> DeleteAsync(int id, CancellationToken cancellationToken);
}
