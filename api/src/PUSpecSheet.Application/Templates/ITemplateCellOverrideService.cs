using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Application.Templates;

public interface ITemplateCellOverrideService
{
    /// <summary>Replaces what a cell changes from its cell type's defaults, and returns the updated template.</summary>
    Task<TableTemplateDto> UpdateAsync(
        int cellId,
        UpdateTemplateCellOverridesRequest request,
        CancellationToken cancellationToken);
}
