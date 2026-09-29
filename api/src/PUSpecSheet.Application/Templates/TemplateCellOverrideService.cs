using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.CellTypes;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Application.Templates;

/// <summary>Saves the configuration and style a template cell changes from its cell type's defaults.</summary>
public sealed class TemplateCellOverrideService(
    PuSpecSheetDbContext db,
    TableTemplateReader reader,
    TemplateVersionGuard guard) : ITemplateCellOverrideService
{
    public async Task<TableTemplateDto> UpdateAsync(
        int cellId,
        UpdateTemplateCellOverridesRequest request,
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

        var versionId = cell.TemplateRow.TemplateSection.TableTemplateVersionId;
        await guard.EnsureEditableAsync(versionId, cancellationToken);

        cell.ConfigurationOverride = CleanConfiguration(cell.CellType.Kind, request.ConfigurationOverride);
        cell.StyleOverride = CleanStyle(request.StyleOverride);

        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadVersionAsync(versionId, cancellationToken);
    }

    private static CellConfiguration? CleanConfiguration(
        CellKind kind,
        CellConfiguration? configuration)
    {
        if (configuration is null)
        {
            return null;
        }

        if (configuration.Kind != kind)
        {
            throw new InvalidRequestException(
                $"This cell's type is a {kind} type, so it can't take {configuration.Kind} settings.");
        }

        var cleaned = CellTypeSettings.CleanConfiguration(kind, configuration);
        return cleaned.IsEmpty() ? null : cleaned;
    }

    private static CellStyle? CleanStyle(CellStyle? style)
    {
        if (style is null)
        {
            return null;
        }

        var cleaned = CellTypeSettings.CleanStyle(style);
        return cleaned.IsEmpty() ? null : cleaned;
    }
}
