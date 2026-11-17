using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Templates.Cells;

// A lookup key changes nothing about a sheet's layout or values, so TemplateVersionGuard doesn't hold it back.
public sealed class TemplateCellLookupKeysService : ITemplateCellLookupKeysService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly TableTemplateReader _reader;
    private readonly ITemplateCellDataService _templateCellData;

    public TemplateCellLookupKeysService(
        PuSpecSheetDbContext db,
        TableTemplateReader reader,
        ITemplateCellDataService templateCellData)
    {
        _db = db;
        _reader = reader;
        _templateCellData = templateCellData;
    }

    public async Task<TableTemplate> UpdateAsync(
        int cellId,
        UpdateTemplateCellLookupKeyRequest request,
        CancellationToken cancellationToken)
    {
        var cell = await _templateCellData.FindWithTypeForEditAsync(cellId, cancellationToken);

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

        await _db.SaveChangesAsync(cancellationToken);
        return await _reader.ReadVersionAsync(cell.TemplateRow.TemplateSection.TableTemplateVersionId, cancellationToken);
    }
}
