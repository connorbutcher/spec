using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Templates.Cells;

public interface ITemplateCellOverridesService
{
    Task<TableTemplate> UpdateAsync(
        int cellId,
        UpdateTemplateCellOverridesRequest request,
        CancellationToken cancellationToken);
}
