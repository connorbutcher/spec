using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Properties.Table;
using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets.Properties;
using PUSpecSheet.Abstractions.Domain.Template;
using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Common.Context;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;
using PUSpecSheet.Api.DAL.DataAccess.Types;
using PUSpecSheet.Api.DAL.Query.Mappers;
using PUSpecSheet.Api.DAL.Query.Resolvers.Cells;
using PUSpecSheet.Api.DAL.Query.Resolvers.Tables;
using PUSpecSheet.Query.Models;
using PUSpecSheet.Query.Models.Analytics;
using PUSpecSheet.Query.Models.Phase;
using PUSpecSheet.Query.Models.Properties;
using PUSpecSheet.Query.Models.Structure;
using PUSpecSheet.Query.Models.Table;
using PUSpecSheet.Query.Models.Table.Cell;
using PUSpecSheet.Query.Models.Values;
using PUSpecSheet.Query.Requests;

namespace PUSpecSheet.Api.DAL.DataAccess.Query;

internal class QueryDataAccessService : IQueryDataAccessService
{
    private readonly ISpecSheetDataAccessService _specSheetDataAccessService;
    private readonly SheetExternalReferences _sheetExternalReferences;
    private readonly ITypeDataAccessService _typeDataAccessService;
    private readonly LinkedTableResolver _linkedTableResolver;
    private readonly LinkedSpecSheetLookupCellResolver _linkedSpecSheetLookupCellResolver;
    private readonly ILogger<IQueryDataAccessService> _logger;

    public QueryDataAccessService(ISpecSheetDataAccessService specSheetDataAccessService,
        SheetExternalReferences sheetExternalReferences,
        ITypeDataAccessService typeDataAccessService,
        IUserContext userContext,
        LinkedTableResolver linkedTableResolver,
        LinkedSpecSheetLookupCellResolver linkedSpecSheetLookupCellResolver,
        ILogger<IQueryDataAccessService> logger)
    {
        ArgumentNullException.ThrowIfNull(specSheetDataAccessService, nameof(specSheetDataAccessService));
        ArgumentNullException.ThrowIfNull(sheetExternalReferences, nameof(sheetExternalReferences));
        ArgumentNullException.ThrowIfNull(typeDataAccessService, nameof(typeDataAccessService));
        ArgumentNullException.ThrowIfNull(userContext, nameof(userContext));
        ArgumentNullException.ThrowIfNull(linkedTableResolver, nameof(linkedTableResolver));
        ArgumentNullException.ThrowIfNull(linkedSpecSheetLookupCellResolver, nameof(linkedSpecSheetLookupCellResolver));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _specSheetDataAccessService = specSheetDataAccessService;
        _sheetExternalReferences = sheetExternalReferences;
        _typeDataAccessService = typeDataAccessService;
        _linkedTableResolver = linkedTableResolver;
        _linkedSpecSheetLookupCellResolver = linkedSpecSheetLookupCellResolver;
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
        var specSheet = await _specSheetDataAccessService.GetSpecSheetByIdAsync(specSheetId, cancellationToken)
            ?? throw new Exception($"SpecSheet with Id {specSheetId} not found.");

        if (!specSheet.IsPublished)
        {
            _logger.LogError("SpecSheet with Id {SpecSheetId} is not published. Only published SpecSheets can be retrieved.", specSheetId);
            throw new Exception($"SpecSheet with Id {specSheetId} is not published. Only published SpecSheets can be retrieved.");
        }

        var specSheetType = await _typeDataAccessService.GetSpecSheetTypeByIdAsync(specSheet.SpecSheetTypeId, cancellationToken)
            ?? throw new Exception($"SpecSheetType with Id {specSheet.SpecSheetTypeId} not found for SpecSheet with Id {specSheet.Id}.");

        var tables = await MapToSlimTablesAsync(specSheet, specSheetType, cancellationToken);
        var properties = MapToSlimPropertyValuesAsync(specSheet);
        var displayLevel = MapToSlimStructureLevel(specSheetType);

        return new SlimPUSpecSheet
        {
            Id = specSheet.Id,
            LegacySpecSheetId = specSheet.LegacySpecSheetId,
            PUSpecSheetPhase = specSheet.SpecSheetPhase.MapToSlimPhase(),
            PUSpecSheetType = specSheet.SpecSheetType.MapToSlimType(),
            PublishedDateTime = specSheet.PublishedDateTime!.Value,
            ChangeLog = specSheet.ChangeLog!,
            Tables = tables,
            Properties = properties,
            DisplayLevel = displayLevel
        };
    }

    public async Task<ICollection<SlimPUSpecSheetSummary>> GetAllSpecSheetsByTypeAsync(int typeId, CancellationToken cancellationToken)
    {
        var specSheets = await _specSheetDataAccessService.GetAllSpecSheetsByTypeAsync(typeId, cancellationToken)
            ?? throw new Exception($"Failed to retrieve SpecSheets for TypeId {typeId}.");
        var result = new List<SlimPUSpecSheetSummary>();
        foreach (var specSheet in specSheets)
        {
            if (!specSheet.IsPublished)
            {
                _logger.LogWarning("SpecSheet with Id {SpecSheetId} is not published. Skipping this SpecSheet in the results.", specSheet.Id);
                continue;
            }

            var specSheetType = await _typeDataAccessService.GetSpecSheetTypeByIdAsync(specSheet.SpecSheetTypeId, cancellationToken)
                ?? throw new Exception($"SpecSheetType with Id {specSheet.SpecSheetTypeId} not found for SpecSheet with Id {specSheet.Id}.");
            var displayLevel = MapToSlimStructureLevel(specSheetType);
            var properties = MapToSlimPropertyValuesAsync(specSheet);

            result.Add(new SlimPUSpecSheetSummary
            {
                Id = specSheet.Id,
                LegacySpecSheetId = specSheet.LegacySpecSheetId,
                PUSpecSheetPhase = specSheet.SpecSheetPhase.MapToSlimPhase(),
                PUSpecSheetType = specSheet.SpecSheetType.MapToSlimType(),
                PublishedDateTime = specSheet.PublishedDateTime!.Value,
                ChangeLog = specSheet.ChangeLog!,
                Properties = properties,
                DisplayLevel = displayLevel
            });
        }
        return result;
    }

    public async Task<ICollection<SlimPUSpecSheetSummary>> GetAllSpecSheetsByTypeAndPhaseAsync(int typeId, int phaseId, CancellationToken cancellationToken)
    {
        var specSheets = await _specSheetDataAccessService.GetAllSpecSheetsByPhaseAndTypeAsync(phaseId, typeId, true, cancellationToken)
            ?? throw new Exception($"Failed to retrieve SpecSheets for TypeId {typeId} and PhaseId {phaseId}.");

        var result = new List<SlimPUSpecSheetSummary>();
        foreach (var specSheet in specSheets)
        {
            if (!specSheet.IsPublished)
            {
                _logger.LogWarning("SpecSheet with Id {SpecSheetId} is not published. Skipping this SpecSheet in the results.", specSheet.Id);
                continue;
            }

            var specSheetType = await _typeDataAccessService.GetSpecSheetTypeByIdAsync(specSheet.SpecSheetTypeId, cancellationToken)
                ?? throw new Exception($"SpecSheetType with Id {specSheet.SpecSheetTypeId} not found for SpecSheet with Id {specSheet.Id}.");
            var displayLevel = MapToSlimStructureLevel(specSheetType);
            var properties = MapToSlimPropertyValuesAsync(specSheet);

            result.Add(new SlimPUSpecSheetSummary
            {
                Id = specSheet.Id,
                LegacySpecSheetId = specSheet.LegacySpecSheetId,
                PUSpecSheetPhase = specSheet.SpecSheetPhase.MapToSlimPhase(),
                PUSpecSheetType = specSheet.SpecSheetType.MapToSlimType(),
                PublishedDateTime = specSheet.PublishedDateTime!.Value,
                ChangeLog = specSheet.ChangeLog!,
                Properties = properties,
                DisplayLevel = displayLevel
            });
        }

        return result;
    }

    public async Task<SlimPUSpecSheet> GetLatestPublishedSpecSheetByPhaseAndTypeAsync(int typeId,
        int phaseId,
        DateTime? publishedByDateTime,
        CancellationToken cancellationToken)
    {
        var specSheet = await _specSheetDataAccessService.GetLatestPublishedSpecSheetByTypeAndPhaseAsync(typeId, phaseId, publishedByDateTime, cancellationToken)
            ?? throw new Exception($"No published SpecSheet found for TypeId {typeId} and PhaseId {phaseId}.");
        if (!specSheet.IsPublished)
        {
            _logger.LogError("SpecSheet with Id {SpecSheetId} is not published. Only published SpecSheets can be retrieved.", specSheet.Id);
            throw new Exception($"SpecSheet with Id {specSheet.Id} is not published. Only published SpecSheets can be retrieved.");
        }

        var specSheetType = await _typeDataAccessService.GetSpecSheetTypeByIdAsync(specSheet.SpecSheetTypeId, cancellationToken)
            ?? throw new Exception($"SpecSheetType with Id {specSheet.SpecSheetTypeId} not found for SpecSheet with Id {specSheet.Id}.");

        var tables = await MapToSlimTablesAsync(specSheet, specSheetType, cancellationToken);
        var properties = MapToSlimPropertyValuesAsync(specSheet);
        var displayLevel = MapToSlimStructureLevel(specSheetType);

        return new SlimPUSpecSheet
        {
            Id = specSheet.Id,
            LegacySpecSheetId = specSheet.LegacySpecSheetId,
            PUSpecSheetPhase = specSheet.SpecSheetPhase.MapToSlimPhase(),
            PUSpecSheetType = specSheet.SpecSheetType.MapToSlimType(),
            PublishedDateTime = specSheet.PublishedDateTime!.Value,
            ChangeLog = specSheet.ChangeLog!,
            Tables = tables,
            Properties = properties,
            DisplayLevel = displayLevel
        };
    }

    public async Task<ICollection<SlimPUSpecSheetSummary>> GetPublishedSpecSheetsByPhaseAndTypeAfterDateAsync(int typeId, int phaseId, DateTime publishedAfterDateTime, CancellationToken cancellationToken)
    {
        var allSpecSheets = await _specSheetDataAccessService.GetAllSpecSheetsByPhaseAndTypeAsync(phaseId, typeId, published: true, cancellationToken)
            ?? throw new Exception($"Failed to retrieve SpecSheets for TypeId {typeId} and PhaseId {phaseId}.");

        var filteredSpecSheets = allSpecSheets.Where(ss => ss.PublishedDateTime.HasValue && ss.PublishedDateTime.Value > publishedAfterDateTime);

        var result = new List<SlimPUSpecSheetSummary>();
        var specSheetType = await _typeDataAccessService.GetSpecSheetTypeByIdAsync(typeId, cancellationToken)
            ?? throw new Exception($"SpecSheetType with Id {typeId} not found.");
        var displayLevel = MapToSlimStructureLevel(specSheetType);

        foreach (var specSheet in filteredSpecSheets)
        {
            if (!specSheet.IsPublished)
            {
                _logger.LogWarning("SpecSheet with Id {SpecSheetId} is not published. Skipping this SpecSheet in the results.", specSheet.Id);
                continue;
            }

            var properties = MapToSlimPropertyValuesAsync(specSheet);
            result.Add(new SlimPUSpecSheetSummary
            {
                Id = specSheet.Id,
                LegacySpecSheetId = specSheet.LegacySpecSheetId,
                PUSpecSheetPhase = specSheet.SpecSheetPhase.MapToSlimPhase(),
                PUSpecSheetType = specSheet.SpecSheetType.MapToSlimType(),
                PublishedDateTime = specSheet.PublishedDateTime!.Value,
                ChangeLog = specSheet.ChangeLog!,
                Properties = properties,
                DisplayLevel = displayLevel
            });
        }

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

        SpecSheet? latestSpecSheet = null;
        IReadOnlyList<SpecSheetTableCell> matchedCells = [];
        if (datasetRequest.UseLatestVersion)
        {
            (latestSpecSheet, matchedCells) = await _specSheetDataAccessService.GetLatestPublishedSpecSheetWithCellsByTypeAndPhaseAsync(
                datasetRequest.PUSpecSheetTypeId,
                datasetRequest.PUSpecSheetPhaseId,
                requestedReferenceIds,
                cancellationToken);
        }
        else
        {
            if (!datasetRequest.PUSpecSheetId.HasValue)
            {
                throw new ArgumentException("PUSpecSheetId must be provided when UseLatestVersion is false.", nameof(datasetRequest.PUSpecSheetId));
            }

            (latestSpecSheet, matchedCells) = await _specSheetDataAccessService.GetPUSpecSheetWithCellsByTypeAndPhaseAsync(
                datasetRequest.PUSpecSheetId.Value,
                requestedReferenceIds,
                cancellationToken);
        }

        if (latestSpecSheet is null || matchedCells.Count == 0)
        {
            return [];
        }

        var matchedCellsByReferenceId = matchedCells.ToDictionary(cell => cell.ReferenceId);
        var properties = MapToSlimPropertyValueSummaries(latestSpecSheet);
        var result = new List<DataPointResult>(matchedCellsByReferenceId.Count);

        foreach (var (referenceId, index) in datasetRequest.ReferenceIds.Select((referenceId, index) => (referenceId, index)))
        {
            if (!matchedCellsByReferenceId.TryGetValue(referenceId, out var cell))
            {
                continue;
            }

            var slimCell = await MapToSlimCell(latestSpecSheet, cell, cancellationToken);
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

    // The references of sheets of a type edited on the revisions are theirs to update; the rest are the copies'.
    public async Task UpdateSystemReferencesAsync(ICollection<ExternalSystemReferenceRequest> systemReferences,
        CancellationToken cancellationToken)
    {
        var copyReferences = await _sheetExternalReferences.UpdateAsync(systemReferences, cancellationToken);
        if (copyReferences.Count > 0)
        {
            await _specSheetDataAccessService.UpdateSystemReferencesAsync(copyReferences, null, cancellationToken);
        }
    }

    public async Task DeleteSystemReferencesByExternalIdAsync(int externalId, CancellationToken cancellationToken)
    {
        await _sheetExternalReferences.DeleteByExternalIdAsync(externalId, cancellationToken);
        await _specSheetDataAccessService.DeleteSystemReferencesByExternalIdAsync(externalId, cancellationToken);
    }

    #region Mappers

    #region Tables

    private async Task<ICollection<SlimPUSpecSheetTable>> MapToSlimTablesAsync(SpecSheet specSheet, SpecSheetType specSheetType, CancellationToken cancellationToken)
    {
        var SlimTables = new List<SlimPUSpecSheetTable>();
        foreach (var table in specSheet.Tables.OrderBy(x => x.DisplayOrder))
        {
            var tableTemplate = specSheetType.TableTemplates.FirstOrDefault(tt => tt.Id == table.TableTemplateId)
                ?? throw new Exception($"TableTemplate with Id {table.TableTemplateId} not found for SpecSheetTable with Id {table.Id}.");

            var SlimTable = await MapToSlimTable(specSheet, table, tableTemplate, cancellationToken);
            if (SlimTable is not null)
            {
                SlimTables.Add(SlimTable);
            }
        }

        return SlimTables;
    }

    private async Task<SlimPUSpecSheetTable?> MapToSlimTable(SpecSheet specSheet, SpecSheetTable table, TableTemplate tableTemplate, CancellationToken cancellationToken)
    {
        if (table.ConfigurationOverride is not null)
        {
            switch (table.ConfigurationOverride)
            {
                case LinkedTableProperties linkedTableProperties:
                    var tableToMapResult = await _linkedTableResolver.ResolveAsync(specSheet, linkedTableProperties, cancellationToken);
                    if (tableToMapResult is null)
                    {
                        _logger.LogWarning("Failed to resolve linked table for SpecSheetTable with Id {SpecSheetTableId} using LinkedTableProperties. Skipping this table.", table.Id);
                        return null;
                    }

                    var tableToMap = tableToMapResult.Result;
                    var specSheetType = tableToMapResult.SpecSheetType;

                    var tableTemplateForLinkedTable = specSheetType.TableTemplates.FirstOrDefault(tt => tt.Id == tableToMap.TableTemplateId)
                        ?? throw new Exception($"TableTemplate with Id {tableToMap.TableTemplateId} not found for linked SpecSheetTable with Id {tableToMap.Id}.");

                    return await MapToSlimTable(specSheet, tableToMap, tableTemplateForLinkedTable, cancellationToken);
                default:
                    _logger.LogWarning("Unknown ConfigurationOverride type {ConfigurationOverrideType} for SpecSheetTable with Id {SpecSheetTableId}", table.ConfigurationOverride.GetType().Name, table.Id);
                    break;
            }
        }

        var rootGroups = table.Groups.Where(g => g.LinkedSpecSheetTableGroupId is null);
        var notes = table.Notes.MapToSlimNotes().ToList();
        var hierarchicalGroups = await BuildGroupHierarchy(specSheet, rootGroups, table.Groups, tableTemplate, cancellationToken);
        return new SlimPUSpecSheetTable
        {
            Id = table.Id,
            ReferenceId = table.ReferenceId,
            Title = table.Title.ToUpperInvariant(),
            Order = table.DisplayOrder,
            Notes = notes,
            Groups = hierarchicalGroups
        };
    }

    #endregion Tables

    #region Groups

    private async Task<ICollection<SlimPUSpecSheetGroup>> BuildGroupHierarchy(SpecSheet specSheet,
    IEnumerable<SpecSheetTableGroup> rootGroups,
    ICollection<SpecSheetTableGroup> allGroups,
    TableTemplate tableTemplate,
    CancellationToken cancellationToken)
    {
        var result = new List<SlimPUSpecSheetGroup>();

        foreach (var group in rootGroups.OrderBy(g => g.Order))
        {
            var SlimGroup = await MapToSlimGroup(specSheet, group, null, allGroups, tableTemplate, cancellationToken);
            result.Add(SlimGroup);
        }

        return result;
    }

    private async Task<SlimPUSpecSheetGroup> MapToSlimGroup(SpecSheet specSheet,
        SpecSheetTableGroup group,
        SpecSheetTableGroup? relatedGroup,
        ICollection<SpecSheetTableGroup> allGroups,
        TableTemplate tableTemplate,
        CancellationToken cancellationToken)
    {
        var groupTemplate = tableTemplate.TableGroupTemplates.FirstOrDefault(gt => gt.Id == group.TableGroupTemplateId)
            ?? throw new Exception($"TableGroupTemplate with Id {group.TableGroupTemplateId} not found for SpecSheetTableGroup with Id {group.Id}.");

        var childGroups = allGroups
            .Where(g => g.LinkedSpecSheetTableGroupId == group.Id)
            .OrderBy(g => g.Order);

        var SlimChildGroups = new List<SlimPUSpecSheetGroup>();
        foreach (var childGroup in childGroups)
        {
            var childGroupTemplate = tableTemplate.TableGroupTemplates.FirstOrDefault(gt => gt.Id == childGroup.TableGroupTemplateId)
                ?? throw new Exception($"TableGroupTemplate with Id {group.TableGroupTemplateId} not found for SpecSheetTableGroup with Id {group.Id}.");

            var linkedGroup = groupTemplate.GroupKey is not null
                ? group
                : null;
            var SlimChildGroup = await MapToSlimGroup(specSheet, childGroup, linkedGroup, allGroups, tableTemplate, cancellationToken);
            SlimChildGroups.Add(SlimChildGroup);
        }

        var SlimRows = await MapToSlimRows(specSheet, group.Rows, relatedGroup?.Rows, cancellationToken);
        return new SlimPUSpecSheetGroup
        {
            Id = group.Id,
            ReferenceId = group.ReferenceId,
            ParentGroupId = group.LinkedSpecSheetTableGroupId,
            GroupType = groupTemplate.GroupType,
            Order = group.Order,
            Groups = SlimChildGroups,
            Rows = SlimRows
        };
    }

    #endregion Groups

    #region Rows

    private async Task<ICollection<SlimPUSpecSheetRow>> MapToSlimRows(SpecSheet specSheet, IEnumerable<SpecSheetTableRow> rows, IEnumerable<SpecSheetTableRow>? linkedRows, CancellationToken cancellationToken)
    {
        var result = new List<SlimPUSpecSheetRow>();
        foreach (var row in rows.OrderBy(r => r.RowNumber))
        {
            var queryCells = new List<SlimPUSpecSheetCell>();

            var linkedRow = linkedRows?.FirstOrDefault(lr => lr.RowNumber == row.RowNumber);
            if (linkedRow != null)
            {
                var linkedCells = await MapToSlimCells(specSheet, linkedRow.Cells, cancellationToken);
                queryCells.AddRange(linkedCells);
            }

            var rowCells = await MapToSlimCells(specSheet, row.Cells, cancellationToken);
            queryCells.AddRange(rowCells);

            result.Add(new SlimPUSpecSheetRow
            {
                Id = row.Id,
                ReferenceId = row.ReferenceId,
                Order = row.RowNumber,
                Cells = queryCells
            });
        }

        return result;
    }

    #endregion Rows

    #region Cells

    private async Task<ICollection<SlimPUSpecSheetCell>> MapToSlimCells(SpecSheet specSheet, IEnumerable<SpecSheetTableCell> cells, CancellationToken cancellationToken)
    {
        var result = new List<SlimPUSpecSheetCell>();
        foreach (var cell in cells.OrderBy(c => c.ColumnNumber))
        {
            var SlimCell = await MapToSlimCell(specSheet, cell, cancellationToken);
            if (SlimCell is not null)
            {
                result.Add(SlimCell);
            }
        }
        return result;
    }

    private async Task<SlimPUSpecSheetCell?> MapToSlimCell(SpecSheet specSheet, SpecSheetTableCell cell, CancellationToken cancellationToken)
    {
        return await cell.MapToSlimCell(specSheet, _linkedSpecSheetLookupCellResolver, cancellationToken);
    }

    #endregion Cells

    #region Properties

    private ICollection<SlimPUSpecSheetPropertyValue> MapToSlimPropertyValuesAsync(SpecSheet specSheet)
    {
        var result = new List<SlimPUSpecSheetPropertyValue>();
        foreach (var propertyValue in specSheet.PropertyValues)
        {
            var SlimPropertyValue = MapToSlimPropertyValueAsync(propertyValue);
            result.Add(SlimPropertyValue);
        }

        return result;
    }

    private ICollection<SlimPUSpecSheetPropertyValueSummary> MapToSlimPropertyValueSummaries(SpecSheet specSheet)
    {
        var result = new List<SlimPUSpecSheetPropertyValueSummary>();
        foreach (var propertyValue in specSheet.PropertyValues)
        {
            var slimPropertyValue = MapToSlimPropertyValueSummary(propertyValue);
            result.Add(slimPropertyValue);
        }

        return result;
    }

    private ICollection<SlimPUSpecSheetPropertyValueSummary> MapToSlimPropertyValuesAsync(SpecSheetSummary specSheet)
    {
        var result = new List<SlimPUSpecSheetPropertyValueSummary>();
        foreach (var propertyValue in specSheet.PropertyValues)
        {
            var SlimPropertyValue = MapToSlimPropertyValueAsync(propertyValue);
            result.Add(SlimPropertyValue);
        }

        return result;
    }

    private SlimPUSpecSheetPropertyValue MapToSlimPropertyValueAsync(SpecSheetPropertyValue propertyValue)
    {
        return propertyValue.PropertyValue switch
        {
            IntPropertyValue intValue => new SlimPUSpecSheetPropertyValue
            {
                Id = propertyValue.Id,
                PropertyInfo = propertyValue.MapToSlimPUSpecSheetPropertyInfo(),
                Value = new SlimIntValue
                {
                    Id = propertyValue.Id,
                    Value = intValue.Value
                }
            },
            DoublePropertyValue intValue => new SlimPUSpecSheetPropertyValue
            {
                Id = propertyValue.Id,
                PropertyInfo = propertyValue.MapToSlimPUSpecSheetPropertyInfo(),
                Value = new SlimDoubleValue
                {
                    Id = propertyValue.Id,
                    Value = intValue.Value
                }
            },
            StringPropertyValue textValue => new SlimPUSpecSheetPropertyValue
            {
                Id = propertyValue.Id,
                PropertyInfo = propertyValue.MapToSlimPUSpecSheetPropertyInfo(),
                Value = new SlimTextValue
                {
                    Id = propertyValue.Id,
                    Value = textValue.Value
                }
            },
            DateTimePropertyValue dateTimeValue => new SlimPUSpecSheetPropertyValue
            {
                Id = propertyValue.Id,
                PropertyInfo = propertyValue.MapToSlimPUSpecSheetPropertyInfo(),
                Value = new SlimDateTimeValue
                {
                    Id = propertyValue.Id,
                    Value = dateTimeValue.Value
                }
            },
            BoolPropertyValue boolPropertyValue => new SlimPUSpecSheetPropertyValue
            {
                Id = propertyValue.Id,
                PropertyInfo = propertyValue.MapToSlimPUSpecSheetPropertyInfo(),
                Value = new SlimBoolValue
                {
                    Id = propertyValue.Id,
                    Value = boolPropertyValue.Value
                }
            },
            _ => throw new NotImplementedException(),
        };
    }

    private SlimPUSpecSheetPropertyValueSummary MapToSlimPropertyValueSummary(SpecSheetPropertyValue propertyValue)
    {
        return propertyValue.PropertyValue switch
        {
            IntPropertyValue intValue => new SlimPUSpecSheetPropertyValueSummary
            {
                ReferenceId = propertyValue.SpecSheetTypeProperty.ReferenceId,
                PropertyKey = propertyValue.SpecSheetTypeProperty.ValueKey,
                PropertyName = propertyValue.SpecSheetTypeProperty.ValueName,
                ValueControlType = propertyValue.SpecSheetTypeProperty.ValueControlType,
                Value = new SlimIntValue
                {
                    Id = propertyValue.Id,
                    Value = intValue.Value
                }
            },
            DoublePropertyValue doubleValue => new SlimPUSpecSheetPropertyValueSummary
            {
                ReferenceId = propertyValue.SpecSheetTypeProperty.ReferenceId,
                PropertyKey = propertyValue.SpecSheetTypeProperty.ValueKey,
                PropertyName = propertyValue.SpecSheetTypeProperty.ValueName,
                ValueControlType = propertyValue.SpecSheetTypeProperty.ValueControlType,
                Value = new SlimDoubleValue
                {
                    Id = propertyValue.Id,
                    Value = doubleValue.Value
                }
            },
            StringPropertyValue textValue => new SlimPUSpecSheetPropertyValueSummary
            {
                ReferenceId = propertyValue.SpecSheetTypeProperty.ReferenceId,
                PropertyKey = propertyValue.SpecSheetTypeProperty.ValueKey,
                PropertyName = propertyValue.SpecSheetTypeProperty.ValueName,
                ValueControlType = propertyValue.SpecSheetTypeProperty.ValueControlType,
                Value = new SlimTextValue
                {
                    Id = propertyValue.Id,
                    Value = textValue.Value
                }
            },
            DateTimePropertyValue dateTimeValue => new SlimPUSpecSheetPropertyValueSummary
            {
                ReferenceId = propertyValue.SpecSheetTypeProperty.ReferenceId,
                PropertyKey = propertyValue.SpecSheetTypeProperty.ValueKey,
                PropertyName = propertyValue.SpecSheetTypeProperty.ValueName,
                ValueControlType = propertyValue.SpecSheetTypeProperty.ValueControlType,
                Value = new SlimDateTimeValue
                {
                    Id = propertyValue.Id,
                    Value = dateTimeValue.Value
                }
            },
            BoolPropertyValue boolPropertyValue => new SlimPUSpecSheetPropertyValueSummary
            {
                ReferenceId = propertyValue.SpecSheetTypeProperty.ReferenceId,
                PropertyKey = propertyValue.SpecSheetTypeProperty.ValueKey,
                PropertyName = propertyValue.SpecSheetTypeProperty.ValueName,
                ValueControlType = propertyValue.SpecSheetTypeProperty.ValueControlType,
                Value = new SlimBoolValue
                {
                    Id = propertyValue.Id,
                    Value = boolPropertyValue.Value
                }
            },
            _ => throw new NotImplementedException(),
        };
    }

    private SlimPUSpecSheetPropertyValueSummary MapToSlimPropertyValueAsync(SpecSheetPropertyValueSummary propertyValue)
    {
        return propertyValue.PropertyValue switch
        {
            IntPropertyValue intValue => new SlimPUSpecSheetPropertyValueSummary
            {
                ReferenceId = propertyValue.ReferenceId,
                PropertyKey = propertyValue.PropertyKey,
                PropertyName = propertyValue.PropertyName,
                ValueControlType = propertyValue.ValueControlType,
                Value = new SlimIntValue
                {
                    Id = -1,
                    Value = intValue.Value
                }
            },
            DoublePropertyValue intValue => new SlimPUSpecSheetPropertyValueSummary
            {
                ReferenceId = propertyValue.ReferenceId,
                PropertyKey = propertyValue.PropertyKey,
                PropertyName = propertyValue.PropertyName,
                ValueControlType = propertyValue.ValueControlType,
                Value = new SlimDoubleValue
                {
                    Id = -1,
                    Value = intValue.Value
                }
            },
            StringPropertyValue textValue => new SlimPUSpecSheetPropertyValueSummary
            {
                ReferenceId = propertyValue.ReferenceId,
                PropertyKey = propertyValue.PropertyKey,
                PropertyName = propertyValue.PropertyName,
                ValueControlType = propertyValue.ValueControlType,
                Value = new SlimTextValue
                {
                    Id = -1,
                    Value = textValue.Value
                }
            },
            DateTimePropertyValue dateTimeValue => new SlimPUSpecSheetPropertyValueSummary
            {
                ReferenceId = propertyValue.ReferenceId,
                PropertyKey = propertyValue.PropertyKey,
                PropertyName = propertyValue.PropertyName,
                ValueControlType = propertyValue.ValueControlType,
                Value = new SlimDateTimeValue
                {
                    Id = -1,
                    Value = dateTimeValue.Value
                }
            },
            BoolPropertyValue boolPropertyValue => new SlimPUSpecSheetPropertyValueSummary
            {
                ReferenceId = propertyValue.ReferenceId,
                PropertyKey = propertyValue.PropertyKey,
                PropertyName = propertyValue.PropertyName,
                ValueControlType = propertyValue.ValueControlType,
                Value = new SlimBoolValue
                {
                    Id = -1,
                    Value = boolPropertyValue.Value
                }
            },
            _ => throw new NotImplementedException(),
        };
    }

    #endregion Properties

    #region Structure

    public SlimStructureLevel MapToSlimStructureLevel(SpecSheetType specSheetType)
    {
        var displayStructureLevel = specSheetType.StructureLevels.FirstOrDefault(sl => sl.StructureLevelType == StructureLevelType.Display)
            ?? throw new Exception($"No display structure level defined for SpecSheetType with Id {specSheetType.Id}.");

        return displayStructureLevel.MapToSlimStructureLevel();
    }

    #endregion Structure

    #endregion Mappers
}
