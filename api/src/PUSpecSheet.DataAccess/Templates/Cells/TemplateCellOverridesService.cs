using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.Abstractions.CellTypes.Styles;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.CellTypes;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Templates.Cells;

public sealed class TemplateCellOverridesService : ITemplateCellOverridesService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly TableTemplateReader _reader;
    private readonly TemplateVersionGuard _guard;
    private readonly ITemplateCellDataService _templateCellData;

    public TemplateCellOverridesService(
        PuSpecSheetDbContext db,
        TableTemplateReader reader,
        TemplateVersionGuard guard,
        ITemplateCellDataService templateCellData)
    {
        _db = db;
        _reader = reader;
        _guard = guard;
        _templateCellData = templateCellData;
    }

    public async Task<TableTemplate> UpdateAsync(
        int cellId,
        UpdateTemplateCellOverridesRequest request,
        CancellationToken cancellationToken)
    {
        var cell = await _templateCellData.FindWithTypeForEditAsync(cellId, cancellationToken);

        if (cell is null)
        {
            throw new NotFoundException($"Cell {cellId} was not found.");
        }

        var versionId = cell.TemplateRow.TemplateSection.TableTemplateVersionId;
        await _guard.EnsureEditableAsync(versionId, cancellationToken);

        cell.ConfigurationOverride = CleanConfiguration(cell.CellType.Kind, request.ConfigurationOverride);
        cell.StyleOverride = CleanStyle(request.StyleOverride);

        await _db.SaveChangesAsync(cancellationToken);
        return await _reader.ReadVersionAsync(versionId, cancellationToken);
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
