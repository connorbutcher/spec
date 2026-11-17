using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.DataAccess.Templates.Cells;

public interface ITemplateCellLookupKeysService
{
    Task<TableTemplate> UpdateAsync(
        int cellId,
        UpdateTemplateCellLookupKeyRequest request,
        CancellationToken cancellationToken);
}
