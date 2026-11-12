using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates.Cells;

/// <summary>
/// Saves a template cell's lookup key. The key changes nothing about a sheet's layout or values, so it
/// isn't held back by <see cref="TemplateVersionGuard"/>: sheets that already use the version can be
/// looked up as soon as the key is set.
/// </summary>
public sealed class TemplateCellLookupKeyService : ITemplateCellLookupKeyService
{
    private readonly ITemplateCellRepository cells;
    private readonly IUnitOfWork unitOfWork;
    private readonly TableTemplateReader reader;

    public TemplateCellLookupKeyService(ITemplateCellRepository cells, IUnitOfWork unitOfWork, TableTemplateReader reader)
    {
        this.cells = cells;
        this.unitOfWork = unitOfWork;
        this.reader = reader;
    }

    public async Task<TableTemplateDto> UpdateAsync(
        int cellId,
        UpdateTemplateCellLookupKeyRequest request,
        CancellationToken cancellationToken)
    {
        var cell = await cells.FindWithTypeForEditAsync(cellId, cancellationToken);

        if (cell is null)
        {
            throw new NotFoundException($"Cell {cellId} was not found.");
        }

        var key = string.IsNullOrWhiteSpace(request.LookupKey) ? null : request.LookupKey.Trim();
        if (key is not null)
        {
            if (!LookupKeyRules.IsValid(key))
            {
                throw new InvalidRequestException(LookupKeyRules.Description);
            }

            // A lookup finds a cell by the text typed into it, so only a text cell can answer to a key. A
            // labelled text cell answers to any of its values.
            if (cell.CellType.Kind is not (CellKind.Text or CellKind.LabelledText))
            {
                throw new InvalidRequestException("Only a text or labelled text cell can have a lookup key.");
            }
        }

        cell.LookupKey = key;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await reader.ReadVersionAsync(cell.TemplateRow.TemplateSection.TableTemplateVersionId, cancellationToken);
    }
}
