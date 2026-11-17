using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Templates.Cells;

public interface ITemplateCellOverridesService
{
    /// <summary>Replaces what a cell changes from its cell type's defaults, and returns the updated template.</summary>
    Task<TableTemplate> UpdateAsync(
        int cellId,
        UpdateTemplateCellOverridesRequest request,
        CancellationToken cancellationToken);
}
