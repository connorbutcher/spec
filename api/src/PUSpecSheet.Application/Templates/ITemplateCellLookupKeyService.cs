using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Application.Templates;

public interface ITemplateCellLookupKeyService
{
    /// <summary>
    /// Sets or removes the name other applications look a cell up by, and returns the updated template.
    /// Unlike a cell's other settings this can change on a version that sheets already use.
    /// </summary>
    Task<TableTemplateDto> UpdateAsync(
        int cellId,
        UpdateTemplateCellLookupKeyRequest request,
        CancellationToken cancellationToken);
}
