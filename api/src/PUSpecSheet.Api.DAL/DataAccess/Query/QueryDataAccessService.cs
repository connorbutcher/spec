using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Common.Context;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;
using PUSpecSheet.Api.DAL.DataAccess.Types;
using PUSpecSheet.Api.DAL.Query.Builders;
using PUSpecSheet.Api.DAL.Query.Mappers;
using PUSpecSheet.Query.Models;
using PUSpecSheet.Query.Models.Analytics;
using PUSpecSheet.Query.Models.Phase;
using PUSpecSheet.Query.Models.Properties;
using PUSpecSheet.Query.Models.Structure;
using PUSpecSheet.Query.Requests;

namespace PUSpecSheet.Api.DAL.DataAccess.Query;

internal class QueryDataAccessService : IQueryDataAccessService
{
    private readonly ISheetDataAccessService _sheetDataAccessService;
    private readonly SheetExternalReferences _sheetExternalReferences;
    private readonly ITypeDataAccessService _typeDataAccessService;
    private readonly SlimTableBuilder _slimTableBuilder;
    private readonly ILogger<IQueryDataAccessService> _logger;

    public QueryDataAccessService(ISheetDataAccessService sheetDataAccessService,
        SheetExternalReferences sheetExternalReferences,
        ITypeDataAccessService typeDataAccessService,
        IUserContext userContext,
        SlimTableBuilder slimTableBuilder,
        ILogger<IQueryDataAccessService> logger)
    {
        ArgumentNullException.ThrowIfNull(sheetDataAccessService, nameof(sheetDataAccessService));
        ArgumentNullException.ThrowIfNull(sheetExternalReferences, nameof(sheetExternalReferences));
        ArgumentNullException.ThrowIfNull(typeDataAccessService, nameof(typeDataAccessService));
        ArgumentNullException.ThrowIfNull(userContext, nameof(userContext));
        ArgumentNullException.ThrowIfNull(slimTableBuilder, nameof(slimTableBuilder));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _sheetDataAccessService = sheetDataAccessService;
        _sheetExternalReferences = sheetExternalReferences;
        _typeDataAccessService = typeDataAccessService;
        _slimTableBuilder = slimTableBuilder;
        _logger = logger;
    }

    public async Task<ICollection<SlimPUSpecSheetType>> GetAllSpecSheetTypesAsync(CancellationToken cancellationToken)
    {
        var specSheetTypes = await _typeDataAccessService.GetAllSpecSheetTypesAsync(cancellationToken)
            ?? throw new Exception("Failed to retrieve SpecSheetTypes.");
        return [.. specSheetTypes.MapToSlimTypes()];
    }

    public async Task<ICollection<SlimSpecSheetPhaseWithChildren>> GetAllSpecSheetPhasesAsync(CancellationToken cancellationToken)
    {
        var specSheetPhases = await _typeDataAccessService.GetAllSpecSheetPhasesAsync(cancellationToken)
            ?? throw new Exception("Failed to retrieve SpecSheetPhases.");
        return [.. specSheetPhases.MapToPhases()];
    }

    public async Task<SlimPUSpecSheet> GetSpecSheetByIdAsync(int specSheetId, CancellationToken cancellationToken)
    {
        var location = await _sheetDataAccessService.FindSheetAsync(specSheetId, cancellationToken)
            ?? throw new Exception($"SpecSheet with Id {specSheetId} not found.");

        if (location.IsDraft)
        {
            _logger.LogError("SpecSheet with Id {SpecSheetId} is not published. Only published SpecSheets can be retrieved.", specSheetId);
            throw new Exception($"SpecSheet with Id {specSheetId} is not published. Only published SpecSheets can be retrieved.");
        }

        var sheet = await _sheetDataAccessService.GetPublishedSheetAsync(location.SheetId, location.VersionNumber, cancellationToken)
            ?? throw new Exception($"SpecSheet with Id {specSheetId} not found.");

        return await MapToSlimSheetAsync(sheet, cancellationToken);
    }

    public async Task<ICollection<SlimPUSpecSheetSummary>> GetAllSpecSheetsByTypeAsync(int typeId, CancellationToken cancellationToken)
    {
        var versions = await _sheetDataAccessService.GetPublishedVersionSummariesAsync(typeId, null, cancellationToken);
        return await MapToSlimSummariesAsync(versions, cancellationToken);
    }

    public async Task<ICollection<SlimPUSpecSheetSummary>> GetAllSpecSheetsByTypeAndPhaseAsync(int typeId, int phaseId, CancellationToken cancellationToken)
    {
        var versions = await _sheetDataAccessService.GetPublishedVersionSummariesAsync(typeId, phaseId, cancellationToken);
        return await MapToSlimSummariesAsync(versions, cancellationToken);
    }

    public async Task<SlimPUSpecSheet> GetLatestPublishedSpecSheetByPhaseAndTypeAsync(int typeId,
        int phaseId,
        DateTime? publishedByDateTime,
        CancellationToken cancellationToken)
    {
        var sheet = await _sheetDataAccessService.GetLatestPublishedSheetByPhaseAndTypeAsync(phaseId, typeId, publishedByDateTime, cancellationToken)
            ?? throw new Exception($"No published SpecSheet found for TypeId {typeId} and PhaseId {phaseId}.");

        return await MapToSlimSheetAsync(sheet, cancellationToken);
    }

    public async Task<ICollection<SlimPUSpecSheetSummary>> GetPublishedSpecSheetsByPhaseAndTypeAfterDateAsync(int typeId, int phaseId, DateTime publishedAfterDateTime, CancellationToken cancellationToken)
    {
        var versions = await _sheetDataAccessService.GetPublishedVersionSummariesAsync(typeId, phaseId, cancellationToken);
        var result = await MapToSlimSummariesAsync([.. versions.Where(version => version.Version.PublishedDateTime > publishedAfterDateTime)], cancellationToken);

        _logger.LogInformation("Retrieved {Count} published SpecSheets for TypeId {TypeId} and PhaseId {PhaseId} after {PublishedAfterDateTime}", result.Count, typeId, phaseId, publishedAfterDateTime);
        return result;
    }

    public async Task<ICollection<DataPointResult>> GetDatasetDataAsync(DatasetRequest datasetRequest, CancellationToken cancellationToken)
    {
        if (datasetRequest.ReferenceIds.Length == 0)
        {
            return [];
        }

        var requestedReferenceIds = datasetRequest.ReferenceIds.ToHashSet();

        SheetCells? sheetCells;
        if (datasetRequest.UseLatestVersion)
        {
            sheetCells = await _sheetDataAccessService.GetLatestPublishedCellsByPhaseAndTypeAsync(
                datasetRequest.PUSpecSheetPhaseId,
                datasetRequest.PUSpecSheetTypeId,
                requestedReferenceIds,
                null,
                cancellationToken);
        }
        else
        {
            if (!datasetRequest.PUSpecSheetId.HasValue)
            {
                throw new ArgumentException("PUSpecSheetId must be provided when UseLatestVersion is false.", nameof(datasetRequest.PUSpecSheetId));
            }

            var location = await _sheetDataAccessService.FindSheetAsync(datasetRequest.PUSpecSheetId.Value, cancellationToken);
            sheetCells = location is null || location.IsDraft
                ? null
                : await _sheetDataAccessService.GetPublishedCellsAsync(location.SheetId, location.VersionNumber, requestedReferenceIds, cancellationToken);
        }

        if (sheetCells is null || sheetCells.Cells.Count == 0)
        {
            return [];
        }

        var sheet = new Sheet
        {
            Id = sheetCells.SheetId,
            ReferenceId = Guid.Empty,
            SpecSheetPhaseId = sheetCells.SpecSheetPhaseId,
            SpecSheetTypeId = sheetCells.SpecSheetTypeId,
            CreatedDateTime = sheetCells.Version.PublishedDateTime,
            Version = sheetCells.Version,
            AsOf = sheetCells.Version.PublishedDateTime,
            IsDraft = false,
            Tables = [],
            Properties = sheetCells.Properties,
            LinkedTables = [],
            ExternalReferences = []
        };

        var matchedCellsByReferenceId = sheetCells.Cells.ToDictionary(cell => cell.ReferenceId);
        var properties = sheetCells.Properties.MapToSlimPropertyValueSummaries().ToList();
        var result = new List<DataPointResult>(matchedCellsByReferenceId.Count);

        foreach (var referenceId in datasetRequest.ReferenceIds)
        {
            if (!matchedCellsByReferenceId.TryGetValue(referenceId, out var cell))
            {
                continue;
            }

            var slimCell = await _slimTableBuilder.BuildCellAsync(sheet, cell, cancellationToken);
            if (slimCell is null)
            {
                continue;
            }

            result.Add(new DataPointResult
            {
                PUSpecSheetTypeId = datasetRequest.PUSpecSheetTypeId,
                PUSpecSheetPhaseId = datasetRequest.PUSpecSheetPhaseId,
                DataPointId = $"{slimCell.Id}",
                ReferenceId = referenceId,
                DisplayName = slimCell.CellType.Title,
                Value = slimCell.Value,
                Properties = properties
            });
        }

        return result;
    }

    public async Task UpdateSystemReferencesAsync(ICollection<ExternalSystemReferenceRequest> systemReferences,
        CancellationToken cancellationToken)
    {
        await _sheetExternalReferences.UpdateAsync(systemReferences, cancellationToken);
    }

    public async Task DeleteSystemReferencesByExternalIdAsync(int externalId, CancellationToken cancellationToken)
    {
        await _sheetExternalReferences.DeleteByExternalIdAsync(externalId, cancellationToken);
    }

    #region Mappers

    private async Task<SlimPUSpecSheet> MapToSlimSheetAsync(Sheet sheet, CancellationToken cancellationToken)
    {
        var specSheetType = await _typeDataAccessService.GetSpecSheetTypeByIdAsync(sheet.SpecSheetTypeId, cancellationToken)
            ?? throw new Exception($"SpecSheetType with Id {sheet.SpecSheetTypeId} not found for Sheet with Id {sheet.Id}.");

        var tables = await _slimTableBuilder.BuildTablesAsync(sheet, specSheetType, cancellationToken);
        return sheet.MapToSlimSheet(tables, MapToSlimStructureLevel(specSheetType));
    }

    private async Task<ICollection<SlimPUSpecSheetSummary>> MapToSlimSummariesAsync(IReadOnlyList<SheetVersionSummary> versions, CancellationToken cancellationToken)
    {
        var displayLevels = new Dictionary<int, SlimStructureLevel>();
        var result = new List<SlimPUSpecSheetSummary>(versions.Count);
        foreach (var version in versions)
        {
            if (!displayLevels.TryGetValue(version.SpecSheetTypeId, out var displayLevel))
            {
                var specSheetType = await _typeDataAccessService.GetSpecSheetTypeByIdAsync(version.SpecSheetTypeId, cancellationToken)
                    ?? throw new Exception($"SpecSheetType with Id {version.SpecSheetTypeId} not found for Sheet with Id {version.SheetId}.");
                displayLevel = MapToSlimStructureLevel(specSheetType);
                displayLevels[version.SpecSheetTypeId] = displayLevel;
            }

            result.Add(version.MapToSlimSummary(displayLevel));
        }

        return result;
    }

    private static SlimStructureLevel MapToSlimStructureLevel(SpecSheetType specSheetType)
    {
        var displayStructureLevel = specSheetType.StructureLevels.FirstOrDefault(sl => sl.StructureLevelType == StructureLevelType.Display)
            ?? throw new Exception($"No display structure level defined for SpecSheetType with Id {specSheetType.Id}.");

        return displayStructureLevel.MapToSlimStructureLevel();
    }

    #endregion Mappers
}
