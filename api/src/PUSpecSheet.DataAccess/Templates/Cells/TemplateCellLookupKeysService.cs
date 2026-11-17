using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Templates.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Templates.Cells;

/// <summary>
/// Saves a template cell's lookup key. The key changes nothing about a sheet's layout or values, so it
/// isn't held back by <see cref="TemplateVersionGuard"/>: sheets that already use the version can be
/// looked up as soon as the key is set.
/// </summary>
public sealed class TemplateCellLookupKeysService : ITemplateCellLookupKeysService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly TableTemplateReader _reader;

    public TemplateCellLookupKeysService(PuSpecSheetDbContext db, TableTemplateReader reader)
    {
        _db = db;
        _reader = reader;
    }

    public async Task<TableTemplate> UpdateAsync(
        int cellId,
        UpdateTemplateCellLookupKeyRequest request,
        CancellationToken cancellationToken)
    {
        var cell = await TemplateCellQueries.FindWithTypeForEditAsync(_db, cellId, cancellationToken);

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
