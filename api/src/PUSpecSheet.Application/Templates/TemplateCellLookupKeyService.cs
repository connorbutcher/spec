using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

/// <summary>
/// Saves a template cell's lookup key. The key changes nothing about a sheet's layout or values, so it
/// isn't held back by <see cref="TemplateVersionGuard"/>: sheets that already use the version can be
/// looked up as soon as the key is set.
/// </summary>
public sealed class TemplateCellLookupKeyService(
    PuSpecSheetDbContext db,
    TableTemplateReader reader) : ITemplateCellLookupKeyService
{
    public async Task<TableTemplateDto> UpdateAsync(
        int cellId,
        UpdateTemplateCellLookupKeyRequest request,
        CancellationToken cancellationToken)
    {
        var cell = await db.TemplateCells
            .Include(candidate => candidate.CellType)
            .Include(candidate => candidate.TemplateRow)
            .ThenInclude(row => row.TemplateSection)
            .SingleOrDefaultAsync(candidate => candidate.Id == cellId, cancellationToken);

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

            // A lookup finds a cell by the text typed into it, so only a text cell can answer to a key.
            if (cell.CellType.Kind != CellKind.Text)
            {
                throw new InvalidRequestException("Only a text cell can have a lookup key.");
            }
        }

        cell.LookupKey = key;

        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadVersionAsync(cell.TemplateRow.TemplateSection.TableTemplateVersionId, cancellationToken);
    }
}
