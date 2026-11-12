using PUSpecSheet.Application.CellTypes;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Application.Templates.Cells;

/// <summary>Saves the configuration and style a template cell changes from its cell type's defaults.</summary>
public sealed class TemplateCellOverrideService : ITemplateCellOverrideService
{
    private readonly ITemplateCellRepository cells;
    private readonly IUnitOfWork unitOfWork;
    private readonly TableTemplateReader reader;
    private readonly TemplateVersionGuard guard;

    public TemplateCellOverrideService(
        ITemplateCellRepository cells,
        IUnitOfWork unitOfWork,
        TableTemplateReader reader,
        TemplateVersionGuard guard)
    {
        this.cells = cells;
        this.unitOfWork = unitOfWork;
        this.reader = reader;
        this.guard = guard;
    }

    public async Task<TableTemplateDto> UpdateAsync(
        int cellId,
        UpdateTemplateCellOverridesRequest request,
        CancellationToken cancellationToken)
    {
        var cell = await cells.FindWithTypeForEditAsync(cellId, cancellationToken);

        if (cell is null)
        {
            throw new NotFoundException($"Cell {cellId} was not found.");
        }

        var versionId = cell.TemplateRow.TemplateSection.TableTemplateVersionId;
        await guard.EnsureEditableAsync(versionId, cancellationToken);

        cell.ConfigurationOverride = CleanConfiguration(cell.CellType.Kind, request.ConfigurationOverride);
        cell.StyleOverride = CleanStyle(request.StyleOverride);

        await unitOfWork.SaveChangesAsync(cancellationToken);
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
