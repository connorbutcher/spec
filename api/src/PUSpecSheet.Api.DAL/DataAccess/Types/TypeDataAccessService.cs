using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Abstractions.Requests.Types.Phase;
using PUSpecSheet.Abstractions.Requests.Types.Properties;
using PUSpecSheet.Abstractions.Requests.Types.Structure;
using PUSpecSheet.Abstractions.Requests.Types.Type;
using PUSpecSheet.Api.Common.Context;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Mappers.SpecSheets;
using PUSpecSheet.Api.Database.Mappers.Types;
using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.DAL.DataAccess.Types;

internal class TypeDataAccessService : ITypeDataAccessService
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly IUserContext _userContext;
    private readonly ILogger<ITypeDataAccessService> _logger;

    public TypeDataAccessService(PUSpecSheetDbContext dbContext,
        IUserContext userContext,
        ILogger<ITypeDataAccessService> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(userContext, nameof(userContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _dbContext = dbContext;
        _userContext = userContext;
        _logger = logger;
    }

    #region Spec Sheet Types

    public async Task<ICollection<SpecSheetType>> GetAllSpecSheetTypesAsync(CancellationToken cancellationToken)
    {
        var specSheetTypes = await _dbContext.SpecSheetTypes
             .Include(x => x.TableTemplates)
                .ThenInclude(x => x.TableTemplateWidgets)
                    .ThenInclude(x => x.WidgetType)

            .Include(x => x.TableTemplates)
                 .ThenInclude(x => x.TableGroupTemplates.OrderBy(x => x.DisplayOrder))
                    .ThenInclude(x => x.TableGroupTemplateWidgets)
                        .ThenInclude(x => x.WidgetType)

             .Include(x => x.TableTemplates)
                 .ThenInclude(x => x.TableGroupTemplates.OrderBy(x => x.DisplayOrder))
                     .ThenInclude(x => x.RowTemplates.OrderBy(x => x.RowNumber))
                         .ThenInclude(x => x.CellTemplates.OrderBy(x => x.ColumnNumber))
                             .ThenInclude(x => x.TableCellType)

             .Include(x => x.TableTemplates)
                 .ThenInclude(x => x.TableGroupTemplates.OrderBy(x => x.DisplayOrder))
                     .ThenInclude(x => x.RowTemplates.OrderBy(x => x.RowNumber))
                         .ThenInclude(x => x.CellTemplates.OrderBy(x => x.ColumnNumber))
                             .ThenInclude(x => x.CellConfiguration)

             .Include(x => x.Permissions)
                 .ThenInclude(x => x.Permission)

            .Include(x => x.Properties.OrderBy(p => p.DisplayOrder))

            .Include(x => x.StructureLevels.OrderBy(sl => sl.Level))
                .ThenInclude(x => x.LevelValues.OrderBy(lv => lv.DisplayOrder))

             .OrderBy(x => x.DisplayOrder)
             .AsNoTracking()
             .MapToDomain()
             .AsSplitQuery()
             .ToListAsync(cancellationToken);

        if (specSheetTypes == null || specSheetTypes.Count == 0)
        {
            _logger.LogWarning("No SpecSheetTypes found in the database.");
            return [];
        }

        _logger.LogInformation("Retrieved {SpecSheetTypeCount} SpecSheetTypes", specSheetTypes.Count);
        return specSheetTypes;
    }

    public async Task<SpecSheetType> GetSpecSheetTypeByIdAsync(int id, CancellationToken cancellationToken)
    {
        var specSheetType = await _dbContext.SpecSheetTypes
            .Where(x => x.Id == id)

            .Include(x => x.TableTemplates)
                .ThenInclude(x => x.TableTemplateWidgets)
                    .ThenInclude(x => x.WidgetType)

            .Include(x => x.TableTemplates)
                 .ThenInclude(x => x.TableGroupTemplates.OrderBy(x => x.DisplayOrder))
                    .ThenInclude(x => x.TableGroupTemplateWidgets)
                        .ThenInclude(x => x.WidgetType)

             .Include(x => x.TableTemplates)
                 .ThenInclude(x => x.TableGroupTemplates.OrderBy(x => x.DisplayOrder))
                     .ThenInclude(x => x.RowTemplates.OrderBy(x => x.RowNumber))
                         .ThenInclude(x => x.CellTemplates.OrderBy(x => x.ColumnNumber))
                             .ThenInclude(x => x.TableCellType)

             .Include(x => x.TableTemplates)
                 .ThenInclude(x => x.TableGroupTemplates.OrderBy(x => x.DisplayOrder))
                     .ThenInclude(x => x.RowTemplates.OrderBy(x => x.RowNumber))
                         .ThenInclude(x => x.CellTemplates.OrderBy(x => x.ColumnNumber))
                             .ThenInclude(x => x.CellConfiguration)

            .Include(x => x.Permissions)
                .ThenInclude(x => x.Permission)

            .Include(x => x.Properties.OrderBy(p => p.DisplayOrder))

            .Include(x => x.StructureLevels.OrderBy(sl => sl.Level))
                .ThenInclude(x => x.LevelValues.OrderBy(lv => lv.DisplayOrder))

            .AsNoTracking()
            .MapToDomain()
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"SpecSheetType with Id {id} not found.");

        _logger.LogInformation("Retrieved SpecSheetType with Id {SpecSheetTypeId}", id);
        return specSheetType;
    }

    public async Task CreateSpecSheetTypeAsync(CreateSpecSheetTypeRequest createSpecSheetTypeRequest, CancellationToken cancellationToken)
    {
        var specSheetPhases = await _dbContext.SpecSheetPhases
            .Where(x => createSpecSheetTypeRequest.SpecSheetPhaseIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var properties = createSpecSheetTypeRequest.Properties.Select(p => new DbSpecSheetTypeProperty
        {
            Id = 0,
            SpecSheetTypeId = 0,
            ReferenceId = p.ReferenceId,
            ValueName = p.ValueName,
            ValueKey = p.ValueKey,
            ValueType = p.ValueType,
            PlaceholderText = p.PlaceholderText,
            ValueControlType = p.ValueControlType,
            CanValueBeSetInForm = p.CanValueBeSetInForm,
            DisplayOrder = p.DisplayOrder,
            ValidatorConfiguration = p.Validators,
            SaveValueAsProperty = true
        }).ToList();

        var structureLevels = createSpecSheetTypeRequest.StructureLevels.Select(sl => new DbSpecSheetTypeStructureLevel
        {
            Id = 0,
            SpecSheetTypeId = 0,
            Level = sl.Level,
            SortOrientation = sl.SortOrientation,
            StructureLevelType = sl.StructureLevelType,
            LevelValues = [.. sl.ValueRequests.Select(lv => new DbSpecSheetTypeStructureLevelValue
            {
                Id = 0,
                SpecSheetTypeStructureLevelId = 0,
                DisplayOrder = lv.DisplayOrder,
                SpecSheetTypePropertyReferenceId = lv.SpecSheetTypePropertyReferenceId
            })]
        }).ToList();

        var newDbSpecSheetType = new DbSpecSheetType
        {
            Id = 0,
            Type = createSpecSheetTypeRequest.Type,
            TableOrientation = createSpecSheetTypeRequest.TableOrientation,
            DisplayOrder = createSpecSheetTypeRequest.DisplayOrder,
            Phases = specSheetPhases,
            TableTemplates = [],
            Permissions = [],
            Properties = properties,
            StructureLevels = structureLevels,
            CanCopyFromMigrated = createSpecSheetTypeRequest.CanCopyFromMigrated,
            RequiresApprovalWorkflow = createSpecSheetTypeRequest.RequiresApprovalWorkflow
        };

        await _dbContext.SpecSheetTypes.AddAsync(newDbSpecSheetType, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created SpecSheetType with Id {SpecSheetTypeId}", newDbSpecSheetType.Id);
    }

    public async Task UpdateSpecSheetTypeAsync(UpdateSpecSheetTypeRequest updateSpecSheetTypeRequest, CancellationToken cancellationToken)
    {
        var dbSpecSheetType = await _dbContext.SpecSheetTypes
            .Include(x => x.Phases)
            .FirstOrDefaultAsync(x => x.Id == updateSpecSheetTypeRequest.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"SpecSheetType with Id {updateSpecSheetTypeRequest.Id} not found.");

        var specSheetPhases = await _dbContext.SpecSheetPhases
            .ToListAsync(cancellationToken);

        dbSpecSheetType.Type = updateSpecSheetTypeRequest.Type;
        dbSpecSheetType.DisplayOrder = updateSpecSheetTypeRequest.DisplayOrder;
        dbSpecSheetType.CanCopyFromMigrated = updateSpecSheetTypeRequest.CanCopyFromMigrated;
        dbSpecSheetType.RequiresApprovalWorkflow = updateSpecSheetTypeRequest.RequiresApprovalWorkflow;

        foreach (var phaseId in updateSpecSheetTypeRequest.NewSpecSheetPhaseIds)
        {
            if (!dbSpecSheetType.Phases.Any(x => x.Id == phaseId))
            {
                var specSheetPhaseToAdd = specSheetPhases.FirstOrDefault(x => x.Id == phaseId)
                    ?? throw new KeyNotFoundException($"SpecSheetPhase with Id {phaseId} not found.");
                dbSpecSheetType.Phases.Add(specSheetPhaseToAdd);
            }
        }

        foreach (var phaseId in updateSpecSheetTypeRequest.RemovedSpecSheetPhaseIds)
        {
            var specSheetPhaseToRemove = dbSpecSheetType.Phases.FirstOrDefault(x => x.Id == phaseId);
            if (specSheetPhaseToRemove != null)
            {
                dbSpecSheetType.Phases.Remove(specSheetPhaseToRemove);
            }
        }

        if (updateSpecSheetTypeRequest.Properties is not null)
        {
            await CreatePropertiesAsync(updateSpecSheetTypeRequest.Properties.CreatePropertyRequests, cancellationToken);
            await UpdatePropertiesAsync(updateSpecSheetTypeRequest.Properties.UpdatePropertyRequests, cancellationToken);
            await DeletePropertiesAsync(updateSpecSheetTypeRequest.Properties.DeletePropertyRequests, cancellationToken);
        }

        if (updateSpecSheetTypeRequest.Structure is not null)
        {
            await CreateStructureLevelAsync(updateSpecSheetTypeRequest.Structure.CreateLevelRequests, cancellationToken);
            await UpdateStructureLevelAsync(updateSpecSheetTypeRequest.Structure.UpdateLevelRequests, cancellationToken);
            await DeleteStructureLevelAsync(updateSpecSheetTypeRequest.Structure.DeleteLevelRequests, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated SpecSheetType with Id {SpecSheetTypeId}", updateSpecSheetTypeRequest.Id);
    }

    #endregion Spec Sheet Types

    #region Spec Sheet Phases

    public async Task<IReadOnlyList<SpecSheetPhase>> GetAllSpecSheetPhasesAsync(CancellationToken cancellationToken)
    {
        var specSheetPhases = await _dbContext.SpecSheetPhases
            .Include(x => x.SpecSheetTypes.OrderBy(t => t.DisplayOrder))
            .Include(x => x.PhaseObservers)
                .ThenInclude(po => po.User)
            .AsSplitQuery()
            .AsNoTracking()
            .MapToDomain()
            .ToListAsync(cancellationToken);

        if (specSheetPhases == null || specSheetPhases.Count == 0)
        {
            _logger.LogWarning("No SpecSheetPhases found in the database.");
            return [];
        }

        _logger.LogInformation("Retrieved {SpecSheetPhaseCount} SpecSheetPhases", specSheetPhases.Count);
        return specSheetPhases.MapToHierarchy();
    }

    public async Task CreateSpecSheetPhaseAsync(CreateSpecSheetPhaseRequest createSpecSheetPhaseRequest, CancellationToken cancellationToken)
    {
        var specSheetTypes = await _dbContext.SpecSheetTypes
            .Where(x => createSpecSheetPhaseRequest.SpecSheetTypeIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var newDbSpecSheetPhaseObservers = createSpecSheetPhaseRequest.CreateSpecSheetPhaseObserverRequests.Select(co => new DbSpecSheetPhaseObserver
        {
            Id = 0,
            SpecSheetPhaseId = 0,
            UserId = co.UserId,
            IsApprover = co.IsApprover
        }).ToList();

        var newDbSpecSheetPhase = new DbSpecSheetPhase
        {
            Id = 0,
            ParentSpecSheetPhaseId = createSpecSheetPhaseRequest.ParentSpecSheetPhaseId,
            Phase = createSpecSheetPhaseRequest.Phase,
            PhaseShort = createSpecSheetPhaseRequest.PhaseShort,
            DisplayOrder = createSpecSheetPhaseRequest.DisplayOrder,
            SpecSheetTypes = specSheetTypes,
            PhaseObservers = newDbSpecSheetPhaseObservers,
            IsObsolete = false
        };

        await _dbContext.SpecSheetPhases.AddAsync(newDbSpecSheetPhase, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created SpecSheetPhase with Id {SpecSheetPhaseId}", newDbSpecSheetPhase.Id);
    }

    public async Task UpdateSpecSheetPhaseAsync(UpdateSpecSheetPhaseRequest updateSpecSheetPhaseRequest, CancellationToken cancellationToken)
    {
        var dbSpecSheetPhase = await _dbContext.SpecSheetPhases
            .Include(x => x.SpecSheetTypes)
            .Include(x => x.PhaseObservers)
            .FirstOrDefaultAsync(x => x.Id == updateSpecSheetPhaseRequest.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"SpecSheetPhase with Id {updateSpecSheetPhaseRequest.Id} not found.");

        var specSheetTypes = await _dbContext.SpecSheetTypes
            .ToListAsync(cancellationToken);

        dbSpecSheetPhase.Phase = updateSpecSheetPhaseRequest.Phase;
        dbSpecSheetPhase.PhaseShort = updateSpecSheetPhaseRequest.PhaseShort;
        dbSpecSheetPhase.DisplayOrder = updateSpecSheetPhaseRequest.DisplayOrder;
        dbSpecSheetPhase.IsObsolete = updateSpecSheetPhaseRequest.IsObsolete;
        dbSpecSheetPhase.ParentSpecSheetPhaseId = updateSpecSheetPhaseRequest.ParentSpecSheetPhaseId;

        foreach (var typeId in updateSpecSheetPhaseRequest.NewSpecSheetTypeIds)
        {
            if (!dbSpecSheetPhase.SpecSheetTypes.Any(x => x.Id == typeId))
            {
                var specSheetTypeToAdd = specSheetTypes.FirstOrDefault(x => x.Id == typeId)
                    ?? throw new KeyNotFoundException($"SpecSheetType with Id {typeId} not found.");
                dbSpecSheetPhase.SpecSheetTypes.Add(specSheetTypeToAdd);
            }
        }

        foreach (var typeId in updateSpecSheetPhaseRequest.RemovedSpecSheetTypeIds)
        {
            var specSheetTypeToRemove = dbSpecSheetPhase.SpecSheetTypes.FirstOrDefault(x => x.Id == typeId);
            if (specSheetTypeToRemove != null)
            {
                dbSpecSheetPhase.SpecSheetTypes.Remove(specSheetTypeToRemove);
            }
        }

        foreach (var createObserverRequest in updateSpecSheetPhaseRequest.CreateSpecSheetPhaseObserverRequests)
        {
            dbSpecSheetPhase.PhaseObservers.Add(new DbSpecSheetPhaseObserver
            {
                Id = 0,
                SpecSheetPhaseId = dbSpecSheetPhase.Id,
                UserId = createObserverRequest.UserId,
                IsApprover = createObserverRequest.IsApprover
            });
        }

        foreach (var updateObserverRequest in updateSpecSheetPhaseRequest.UpdateSpecSheetPhaseObserverRequests)
        {
            var observerToUpdate = dbSpecSheetPhase.PhaseObservers.FirstOrDefault(po => po.Id == updateObserverRequest.Id);
            observerToUpdate?.IsApprover = updateObserverRequest.IsApprover;
        }

        foreach (var deleteObserverRequest in updateSpecSheetPhaseRequest.RemoveSpecSheetPhaseObserverIds)
        {
            var observerToDelete = dbSpecSheetPhase.PhaseObservers.FirstOrDefault(po => po.Id == deleteObserverRequest);
            if (observerToDelete != null)
            {
                _dbContext.Remove(observerToDelete);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated SpecSheetPhase with Id {SpecSheetPhaseId}", updateSpecSheetPhaseRequest.Id);
    }

    public async Task UpdateManySpecSheetPhasesAsync(IReadOnlyList<UpdateSpecSheetPhaseRequest> updateRequests, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var updateRequest in updateRequests)
        {
            await UpdateSpecSheetPhaseAsync(updateRequest, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Updated {SpecSheetPhaseCount} SpecSheetPhases", updateRequests.Count);
    }

    #endregion Spec Sheet Phases    

    #region Properties

    private async Task CreatePropertiesAsync(ICollection<CreateSpecSheetTypePropertyRequest> propertyRequests, CancellationToken cancellationToken)
    {
        var dbProperties = propertyRequests.Select(pr => new DbSpecSheetTypeProperty
        {
            Id = 0,
            SpecSheetTypeId = pr.SpecSheetTypeId,
            ReferenceId = pr.ReferenceId,
            ValueKey = pr.ValueKey,
            ValueName = pr.ValueName,
            ValueControlType = pr.ValueControlType,
            ValueType = pr.ValueType,
            PlaceholderText = pr.PlaceholderText,
            DisplayOrder = pr.DisplayOrder,
            CanValueBeSetInForm = pr.CanValueBeSetInForm,
            SaveValueAsProperty = true,
            ValidatorConfiguration = pr.Validators
        });
        await _dbContext.SpecSheetTypeProperties.AddRangeAsync(dbProperties, cancellationToken);
    }

    private async Task UpdatePropertiesAsync(ICollection<UpdateSpecSheetTypePropertyRequest> propertyRequests, CancellationToken cancellationToken)
    {
        foreach (var propertyRequest in propertyRequests)
        {
            var existingProperty = await _dbContext.SpecSheetTypeProperties
                .Where(p => p.Id == propertyRequest.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"SpecSheetTypeProperty with Id {propertyRequest.Id} not found.");

            if (propertyRequest.ValueName != null)
            {
                existingProperty.ValueName = propertyRequest.ValueName;
            }

            if (propertyRequest.ValueKey != null)
            {
                existingProperty.ValueKey = propertyRequest.ValueKey;
            }

            if (propertyRequest.ValueType != null)
            {
                existingProperty.ValueType = propertyRequest.ValueType;
            }

            if (propertyRequest.PlaceholderText != null && propertyRequest.PlaceholderText.HasChanged)
            {
                existingProperty.PlaceholderText = propertyRequest.PlaceholderText.Value;
            }

            if (propertyRequest.ValueControlType != null)
            {
                existingProperty.ValueControlType = propertyRequest.ValueControlType;
            }

            if (propertyRequest.CanValueBeSetInForm.HasValue)
            {
                existingProperty.CanValueBeSetInForm = propertyRequest.CanValueBeSetInForm.Value;
            }

            if (propertyRequest.DisplayOrder.HasValue)
            {
                existingProperty.DisplayOrder = propertyRequest.DisplayOrder.Value;
            }

            if (propertyRequest.Validators != null && propertyRequest.Validators.HasChanged)
            {
                existingProperty.ValidatorConfiguration = propertyRequest.Validators.Value;
            }
        }
    }

    private async Task DeletePropertiesAsync(ICollection<int> propertyRequests, CancellationToken cancellationToken)
    {
        foreach (var propertyRequest in propertyRequests)
        {
            var propertyToDelete = await _dbContext.SpecSheetTypeProperties
                .FindAsync([propertyRequest], cancellationToken)
                ?? throw new KeyNotFoundException($"SpecSheetTypeProperty with Id {propertyRequest} not found.");

            _dbContext.Remove(propertyToDelete);
        }
    }

    #endregion Properties

    #region Structure

    private async Task CreateStructureLevelAsync(ICollection<CreateSpecSheetTypeStructureLevelRequest> structureLevelRequests, CancellationToken cancellationToken)
    {
        var dbStructureLevels = structureLevelRequests.Select(slr => new DbSpecSheetTypeStructureLevel
        {
            Id = 0,
            SpecSheetTypeId = slr.SpecSheetTypeId,
            StructureLevelType = slr.StructureLevelType,
            Level = slr.Level,
            SortOrientation = slr.SortOrientation,
            LevelValues = CreateStructureLevelValues(slr.ValueRequests)
        });
        await _dbContext.AddRangeAsync(dbStructureLevels);
    }

    private async Task UpdateStructureLevelAsync(ICollection<UpdateSpecSheetTypeStructureLevelRequest> structureLevelRequests, CancellationToken cancellationToken)
    {
        foreach (var structureLevelRequest in structureLevelRequests)
        {
            var structureLevelToUpdate = await _dbContext.SpecSheetTypeStructureLevels
                .Where(p => p.Id == structureLevelRequest.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"SpecSheetTypeStructureLevel with Id {structureLevelRequest.Id} not found.");

            structureLevelToUpdate.Level = structureLevelRequest.Level;
            structureLevelToUpdate.SortOrientation = structureLevelRequest.SortOrientation;

            if (structureLevelRequest.ManageLevelValueRequest is not null)
            {
                var newStructureLevels = CreateStructureLevelValues(structureLevelRequest.ManageLevelValueRequest.CreateLevelValueRequests);
                await _dbContext.AddRangeAsync(newStructureLevels);
                await UpdateStructureLevelValueAsync(structureLevelRequest.ManageLevelValueRequest.UpdateLevelValueRequests, cancellationToken);
                await DeleteStructureLevelValueAsync(structureLevelRequest.ManageLevelValueRequest.DeleteLevelValueRequests, cancellationToken);
            }
        }
    }

    private async Task DeleteStructureLevelAsync(ICollection<int> structureLevelRequests, CancellationToken cancellationToken)
    {
        foreach (var structureLevelRequest in structureLevelRequests)
        {
            var propertyToDelete = await _dbContext.SpecSheetTypeStructureLevels
                .Include(x => x.LevelValues)
                .Where(lv => lv.Id == structureLevelRequest)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"SpecSheetTypeStructureLevels with Id {structureLevelRequest} not found.");

            _dbContext.Remove(propertyToDelete);
        }
    }

    private static ICollection<DbSpecSheetTypeStructureLevelValue> CreateStructureLevelValues(ICollection<CreateSpecSheetTypeStructureLevelValueRequest> structureLevelValueRequests)
    {
        return structureLevelValueRequests
            .Select(slv => new DbSpecSheetTypeStructureLevelValue
            {
                Id = 0,
                SpecSheetTypeStructureLevelId = slv.SpecSheetTypeStructureLevelId,
                DisplayOrder = slv.DisplayOrder,
                SpecSheetTypePropertyReferenceId = slv.SpecSheetTypePropertyReferenceId
            })
            .ToList();
    }

    private async Task UpdateStructureLevelValueAsync(ICollection<UpdateSpecSheetTypeStructureLevelValueRequest> structureLevelValueRequests, CancellationToken cancellationToken)
    {
        foreach (var structureLevelValueRequest in structureLevelValueRequests)
        {
            var structureLevelValueToUpdate = await _dbContext.SpecSheetTypeStructureLevelValues
                .Where(p => p.Id == structureLevelValueRequest.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"SpecSheetTypeStructureLevelValue with Id {structureLevelValueRequest.Id} not found.");

            structureLevelValueToUpdate.DisplayOrder = structureLevelValueRequest.DisplayOrder;
            structureLevelValueToUpdate.SpecSheetTypePropertyReferenceId = structureLevelValueRequest.SpecSheetTypePropertyReferenceId;
        }
    }

    private async Task DeleteStructureLevelValueAsync(ICollection<int> structureLevelValueRequests, CancellationToken cancellationToken)
    {
        foreach (var structureLevelValueRequest in structureLevelValueRequests)
        {
            var structureLevelValuetoDelete = await _dbContext.SpecSheetTypeStructureLevelValues
                .FindAsync([structureLevelValueRequest], cancellationToken)
                ?? throw new KeyNotFoundException($"SpecSheetTypeStructureLevelValue with Id {structureLevelValueRequest} not found.");

            _dbContext.Remove(structureLevelValuetoDelete);
        }
    }

    #endregion Structure
}
