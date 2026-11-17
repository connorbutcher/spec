using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.SpecSheets;
using PUSpecSheet.Abstractions.Domain.SpecSheets.Properties;
using PUSpecSheet.Abstractions.Domain.Values;
using PUSpecSheet.Abstractions.Operations;
using PUSpecSheet.Abstractions.Requests.SpecSheets;
using PUSpecSheet.Abstractions.Requests.Workflow;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Common.Context;
using PUSpecSheet.Api.DAL.Caching;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;
using PUSpecSheet.Api.DAL.DataAccess.Types;
using PUSpecSheet.Api.DAL.Processors;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Mappers.SpecSheets;
using PUSpecSheet.Api.Database.Mappers.Types;
using PUSpecSheet.Api.Database.Schema.References;
using PUSpecSheet.Api.Database.Schema.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Values;
using PUSpecSheet.Api.Database.Schema.Workflow;
using PUSpecSheet.Api.Emails.Models;
using PUSpecSheet.Api.Emails.Services;
using PUSpecSheet.Query.Requests;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets;

internal class SpecSheetDataAccessService : ISpecSheetDataAccessService
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly ISpecSheetCache _specSheetCache;
    private readonly ISpecSheetReader _specSheetReader;
    private readonly ITypeDataAccessService _typeDataAccessService;
    private readonly IEmailBuilderService _emailBuilderService;
    private readonly IEmailService _emailService;
    private readonly IUserContext _userContext;
    private readonly ILogger<ISpecSheetDataAccessService> _logger;

    public SpecSheetDataAccessService(PUSpecSheetDbContext dbContext,
        ISpecSheetCache specSheetCache,
        ISpecSheetReader specSheetReader,
        ITypeDataAccessService typeDataAccessService,
        IEmailBuilderService emailBuilderService,
        IEmailService emailService,
        IUserContext userContext,
        ILogger<ISpecSheetDataAccessService> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(specSheetCache, nameof(specSheetCache));
        ArgumentNullException.ThrowIfNull(specSheetReader, nameof(specSheetReader));
        ArgumentNullException.ThrowIfNull(typeDataAccessService, nameof(typeDataAccessService));
        ArgumentNullException.ThrowIfNull(emailBuilderService, nameof(emailBuilderService));
        ArgumentNullException.ThrowIfNull(emailService, nameof(emailService));
        ArgumentNullException.ThrowIfNull(userContext, nameof(userContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _dbContext = dbContext;
        _specSheetCache = specSheetCache;
        _specSheetReader = specSheetReader;
        _typeDataAccessService = typeDataAccessService;
        _emailBuilderService = emailBuilderService;
        _emailService = emailService;
        _userContext = userContext;
        _logger = logger;
    }

    #region Spec Sheets

    public async Task<SpecSheet> GetSpecSheetByIdAsync(int id, CancellationToken cancellationToken, bool permissionTrimTables = true)
    {
        var currentUser = await _userContext.EnsureUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Current user not found in the context.");

        SpecSheet? domainSpecSheet;
        domainSpecSheet = await _specSheetCache.GetAsync(id, cancellationToken);
        if (domainSpecSheet is null)
        {
            domainSpecSheet = await _specSheetReader.ReadAsync(id, cancellationToken);
            await _specSheetCache.SetAsync(domainSpecSheet, cancellationToken);
        }

        if (permissionTrimTables)
        {
            var tablePermissions = await _dbContext.SpecSheetTablePermissions
                .Where(p => domainSpecSheet.Tables.Select(t => t.ReferenceId).Contains(p.TableReferenceId))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var tables = new List<SpecSheetTable>();
            foreach (var table in domainSpecSheet.Tables)
            {
                var hasPermission = tablePermissions.Any(p => p.TableReferenceId == table.ReferenceId);
                if (!hasPermission)
                {
                    tables.Add(table);
                    continue;
                }

                if (tablePermissions.Any(p => p.TableReferenceId == table.ReferenceId && p.UserId == currentUser.Id))
                {
                    tables.Add(table);
                    continue;
                }
            }

            domainSpecSheet = domainSpecSheet with { Tables = tables };
        }

        _logger.LogInformation("Retrieved SpecSheet with Id {SpecSheetId}", id);
        return domainSpecSheet;
    }

    public async Task<SpecSheet?> GetLatestPublishedSpecSheetByTypeAndPhaseAsync(
        int typeId,
        int phaseId,
        DateTime? publishedByDateTime,
        CancellationToken cancellationToken)
    {
        var latestSpecSheetId = await _dbContext.SpecSheets
            .AsNoTracking()
            .AsSplitQuery()
            .Where(ss => ss.IsPublished && ss.SpecSheetTypeId == typeId && ss.SpecSheetPhaseId == phaseId && (!publishedByDateTime.HasValue || ss.PublishedDateTime <= publishedByDateTime.Value))
            .OrderByDescending(ss => ss.PublishedDateTime)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestSpecSheetId == 0)
        {
            return null;
        }

        return await GetSpecSheetByIdAsync(latestSpecSheetId, cancellationToken);
    }

    public async Task<(SpecSheet? SpecSheet, IReadOnlyList<SpecSheetTableCell> Cells)> GetLatestPublishedSpecSheetWithCellsByTypeAndPhaseAsync(
        int typeId,
        int phaseId,
        IReadOnlyCollection<Guid> referenceIds,
        CancellationToken cancellationToken)
    {
        if (referenceIds.Count == 0)
        {
            return (null, []);
        }

        var latestSpecSheetId = await _dbContext.SpecSheets
            .AsNoTracking()
            .Where(ss => ss.IsPublished && ss.SpecSheetTypeId == typeId && ss.SpecSheetPhaseId == phaseId)
            .OrderByDescending(ss => ss.PublishedDateTime)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestSpecSheetId == 0)
        {
            return (null, []);
        }

        var specSheet = await _dbContext.SpecSheets
            .Where(x => x.Id == latestSpecSheetId)
            .Include(x => x.CreatedByUser)
            .Include(x => x.PublishedByUser)
            .Include(x => x.SpecSheetType)
            .Include(x => x.SpecSheetPhase)
            .Include(x => x.PropertyValues)
                .ThenInclude(x => x.SpecSheetTypeProperty)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstAsync(cancellationToken);

        var rowIdsInLatestSpecSheet = from row in _dbContext.SpecSheetTableRows
                                      join grp in _dbContext.SpecSheetTableGroups on row.SpecSheetTableGroupId equals grp.Id
                                      join tbl in _dbContext.SpecSheetTables on grp.SpecSheetTableId equals tbl.Id
                                      where tbl.SpecSheetId == latestSpecSheetId
                                          && !tbl.IsDeleted
                                          && !grp.IsDeleted
                                          && !row.IsDeleted
                                      select row.Id;

        var cells = await _dbContext.SpecSheetTableCells
            .Where(cell => !cell.IsDeleted
                && referenceIds.Contains(cell.ReferenceId)
                && rowIdsInLatestSpecSheet.Contains(cell.SpecSheetTableRowId))
            .Include(x => x.TableCellType)
            .Include(x => x.CellConfiguration)
            .Include(x => x.TextValue)
            .Include(x => x.NumericValue)
            .Include(x => x.PairValue)
            .Include(x => x.LinkedSpecSheetValue)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (specSheet.MapToDomain(), [.. cells.MapToDomain()]);
    }

    public async Task<(SpecSheet? SpecSheet, IReadOnlyList<SpecSheetTableCell> Cells)> GetPUSpecSheetWithCellsByTypeAndPhaseAsync(
        int puSpecSheetId,
        IReadOnlyCollection<Guid> referenceIds,
        CancellationToken cancellationToken)
    {
        if (referenceIds.Count == 0)
        {
            return (null, []);
        }

        var specSheet = await _dbContext.SpecSheets
            .Where(x => x.Id == puSpecSheetId)
            .Include(x => x.CreatedByUser)
            .Include(x => x.PublishedByUser)
            .Include(x => x.SpecSheetType)
            .Include(x => x.SpecSheetPhase)
            .Include(x => x.PropertyValues)
                .ThenInclude(x => x.SpecSheetTypeProperty)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstAsync(cancellationToken);

        var rowIdsInLatestSpecSheet = from row in _dbContext.SpecSheetTableRows
                                      join grp in _dbContext.SpecSheetTableGroups on row.SpecSheetTableGroupId equals grp.Id
                                      join tbl in _dbContext.SpecSheetTables on grp.SpecSheetTableId equals tbl.Id
                                      where tbl.SpecSheetId == puSpecSheetId
                                          && !tbl.IsDeleted
                                          && !grp.IsDeleted
                                          && !row.IsDeleted
                                      select row.Id;

        var cells = await _dbContext.SpecSheetTableCells
            .Where(cell => !cell.IsDeleted
                && referenceIds.Contains(cell.ReferenceId)
                && rowIdsInLatestSpecSheet.Contains(cell.SpecSheetTableRowId))
            .Include(x => x.TableCellType)
            .Include(x => x.CellConfiguration)
            .Include(x => x.TextValue)
            .Include(x => x.NumericValue)
            .Include(x => x.PairValue)
            .Include(x => x.LinkedSpecSheetValue)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (specSheet.MapToDomain(), [.. cells.MapToDomain()]);
    }

    public async Task<SpecSheetSummary?> GetLatestPublishedSpecSheetSummaryByTypeAndPhaseAsync(
        int typeId,
        int phaseId,
        DateTime? publishedByDateTime,
        CancellationToken cancellationToken)
    {
        var latestSpecSheet = await _dbContext.SpecSheets
            .Include(x => x.CreatedByUser)
            .Include(x => x.PublishedByUser)
            .Include(x => x.SpecSheetType)
            .Include(x => x.SpecSheetPhase)
            .Include(x => x.PropertyValues)
                .ThenInclude(x => x.SpecSheetTypeProperty)
            .Where(ss => ss.IsPublished && ss.SpecSheetTypeId == typeId && ss.SpecSheetPhaseId == phaseId && (!publishedByDateTime.HasValue || ss.PublishedDateTime <= publishedByDateTime.Value))
            .OrderByDescending(ss => ss.PublishedDateTime)
            .AsNoTracking()
            .AsSplitQuery()
            .MapToSummaryDomain()
            .FirstOrDefaultAsync(cancellationToken);

        return latestSpecSheet;
    }

    public async Task<IReadOnlyList<SpecSheetSummary>> GetAllSpecSheetsByPhaseAsync(int phaseId, bool includeChildren, CancellationToken cancellationToken)
    {
        var phaseIds = new List<int> { phaseId };
        if (includeChildren)
        {
            var childPhaseIds = await _dbContext.SpecSheetPhases
                .AsNoTracking()
                .Where(x => x.ParentSpecSheetPhaseId == phaseId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
            phaseIds.AddRange(childPhaseIds);
        }

        var specSheets = await _dbContext.SpecSheets
            .Where(x => phaseIds.Any(id => id == x.SpecSheetPhaseId))
            .Include(x => x.CreatedByUser)
            .Include(x => x.PublishedByUser)
            .Include(x => x.SpecSheetType)
            .Include(x => x.SpecSheetPhase)
            .Include(x => x.PropertyValues)
                .ThenInclude(x => x.SpecSheetTypeProperty)
            .AsNoTracking()
            .MapToSummaryDomain()
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        if (specSheets == null || specSheets.Count == 0)
        {
            _logger.LogWarning("No SpecSheets found for SpecSheetPhaseId {SpecSheetPhaseId}", phaseId);
            return [];
        }

        _logger.LogInformation("Retrieved {SpecSheetCount} SpecSheets for SpecSheetPhaseId {SpecSheetPhaseId}", specSheets.Count, phaseId);
        return specSheets;
    }

    public async Task<IReadOnlyList<SpecSheetSummary>> GetAllSpecSheetsByTypeAsync(int typeId, CancellationToken cancellationToken)
    {
        var specSheets = await _dbContext.SpecSheets
            .Where(x => x.SpecSheetTypeId == typeId)
            .Include(x => x.CreatedByUser)
            .Include(x => x.PublishedByUser)
            .Include(x => x.SpecSheetType)
            .Include(x => x.SpecSheetPhase)
            .Include(x => x.PropertyValues)
                .ThenInclude(x => x.SpecSheetTypeProperty)
            .AsNoTracking()
            .MapToSummaryDomain()
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        if (specSheets == null || specSheets.Count == 0)
        {
            _logger.LogWarning("No SpecSheets found for SpecSheetTypeId {SpecSheetTypeId}", typeId);
            return [];
        }

        _logger.LogInformation("Retrieved {SpecSheetCount} SpecSheets for SpecSheetTypeId {SpecSheetTypeId}", specSheets.Count, typeId);
        return specSheets;
    }

    public async Task<IReadOnlyList<SpecSheetSummary>> GetAllSpecSheetsByPhaseAndTypeAsync(int phaseId,
        int typeId,
        bool? published,
        CancellationToken cancellationToken)
    {
        var specSheets = await _dbContext.SpecSheets
            .Where(x => x.SpecSheetPhaseId == phaseId && x.SpecSheetTypeId == typeId && (!published.HasValue || x.IsPublished == published.Value))
            .Include(x => x.CreatedByUser)
            .Include(x => x.PublishedByUser)
            .Include(x => x.SpecSheetType)
            .Include(x => x.SpecSheetPhase)
            .Include(x => x.PropertyValues)
                .ThenInclude(x => x.SpecSheetTypeProperty)
            .AsNoTracking()
            .MapToSummaryDomain()
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        if (specSheets == null || specSheets.Count == 0)
        {
            _logger.LogWarning("No SpecSheets found for SpecSheetPhaseId {SpecSheetPhaseId} and SpecSheetTypeId {SpecSheetTypeId}", phaseId, typeId);
            return [];
        }

        _logger.LogInformation("Retrieved {SpecSheetCount} SpecSheets for SpecSheetPhaseId {SpecSheetPhaseId} and SpecSheetTypeId {SpecSheetTypeId}", specSheets.Count, phaseId, typeId);
        return specSheets;
    }

    public async Task<int> CreateSpecSheetAsync(CreateSpecSheetRequest createPUSpecSheetRequest, CancellationToken cancellationToken)
    {
        var creatingUser = await _userContext.EnsureUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Creating user not found in the context.");

        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Validate that there is not already a draft spec sheet for this phase and type.
            var existingDraftSpecSheet = await _dbContext.SpecSheets
                .AsNoTracking()
                .FirstOrDefaultAsync(ss => ss.SpecSheetPhaseId == createPUSpecSheetRequest.SpecSheetPhaseId
                    && ss.SpecSheetTypeId == createPUSpecSheetRequest.SpecSheetTypeId
                    && !ss.IsPublished, cancellationToken);
            if (existingDraftSpecSheet != null)
            {
                _logger.LogError("Attempted to create a new draft SpecSheet for SpecSheetPhaseId {SpecSheetPhaseId} and SpecSheetTypeId {SpecSheetTypeId}, but a draft with Id {ExistingDraftSpecSheetId} already exists", createPUSpecSheetRequest.SpecSheetPhaseId, createPUSpecSheetRequest.SpecSheetTypeId, existingDraftSpecSheet.Id);
                throw new InvalidOperationException($"A draft spec sheet with Id {existingDraftSpecSheet.Id} already exists for SpecSheetPhaseId {createPUSpecSheetRequest.SpecSheetPhaseId} and SpecSheetTypeId {createPUSpecSheetRequest.SpecSheetTypeId}. Please publish or delete the existing draft before creating a new one.");
            }

            var specSheetType = await _typeDataAccessService.GetSpecSheetTypeByIdAsync(createPUSpecSheetRequest.SpecSheetTypeId, cancellationToken)
                ?? throw new KeyNotFoundException($"Spec Sheet Type with Id {createPUSpecSheetRequest.SpecSheetTypeId} not found.");

            var copyFromProperty = specSheetType.Properties
                .FirstOrDefault(p => p.ValueControlType == "Copy From");
            var tables = new List<DbSpecSheetTable>();
            var externalReferences = new List<DbSpecSheetExternalReference>();
            if (copyFromProperty != null)
            {
                var copyFromPropertyValue = createPUSpecSheetRequest.PropertyValues
                    .FirstOrDefault(pv => pv.SpecSheetTypePropertyId == copyFromProperty.Id);
                if (copyFromPropertyValue is not null &&
                    copyFromPropertyValue.PropertyValue is not null &&
                    copyFromPropertyValue.PropertyValue is IntPropertyValue copyFromId &&
                    copyFromId.Value.HasValue)
                {
                    var specSheetToCopyFrom = await GetSpecSheetByIdAsync(copyFromId.Value.Value, cancellationToken, false)
                        ?? throw new KeyNotFoundException($"Spec Sheet to copy from with Id {copyFromId.Value.Value} not found.");
                    tables = BuildNewDbSpecSheetTables(creatingUser, specSheetToCopyFrom.Tables);
                    externalReferences = specSheetToCopyFrom.ExternalReferences
                        .Select(r => new DbSpecSheetExternalReference
                        {
                            Id = 0,
                            SpecSheetId = 0,
                            ExternalId = r.ExternalId,
                            ReferenceId = r.ReferenceId,
                            SystemName = r.SystemName,
                            OwnerName = r.OwnerName,
                            Url = r.Url,
                            Type = r.Type,
                            Grouping = r.Grouping,
                            SystemIconUrl = r.SystemIconUrl,
                            Description = r.Description
                        })
                        .ToList();
                }
            }

            var propertyValues = createPUSpecSheetRequest.PropertyValues.Select(pv => new DbSpecSheetPropertyValue
            {
                Id = 0,
                SpecSheetId = 0,
                SpecSheetTypePropertyId = pv.SpecSheetTypePropertyId,
                PropertyValue = pv.PropertyValue?.ToString(),
            }).ToList();

            var newDbSpecSheet = new DbSpecSheet
            {
                Id = 0,
                SpecSheetPhaseId = createPUSpecSheetRequest.SpecSheetPhaseId,
                SpecSheetTypeId = createPUSpecSheetRequest.SpecSheetTypeId,
                CreatedByUserId = creatingUser.Id,
                CreatedDateTime = DateTime.UtcNow,
                IsPublished = false,
                Tables = tables,
                PropertyValues = propertyValues,
                HasBeenMigrated = false,
                ExternalReferences = externalReferences
            };

            await _dbContext.SpecSheets.AddAsync(newDbSpecSheet, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Created SpecSheet with Id {SpecSheetId} by UserId {UserId}", newDbSpecSheet.Id, creatingUser.Id);
            return newDbSpecSheet.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SpecSheet");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateSpecSheetAsync(SpecSheetChanges specSheetChanges, CancellationToken cancellationToken)
    {
        var updatingUser = await _userContext.EnsureUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Updating user not found in the context.");

        try
        {
            var processor = new SpecSheetChangeProcessor(_dbContext, updatingUser, _logger);
            await processor.ApplyChangesAsync(specSheetChanges.Operations, cancellationToken);

            await _specSheetCache.RemoveAsync(specSheetChanges.SpecSheetId, cancellationToken);
            _logger.LogInformation("Updated SpecSheet with Id {SpecSheetId} by UserId {UserId}", specSheetChanges.SpecSheetId, updatingUser.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SpecSheet with Id {SpecSheetId}", specSheetChanges.SpecSheetId);
        }
    }

    public async Task PublishSpecSheetAsync(PublishSpecSheetRequest publishSpecSheetRequest, CancellationToken cancellationToken)
    {
        var publishingUser = await _userContext.EnsureUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Publishing user not found in the context.");

        var specSheetToPublish = await _dbContext.SpecSheets
            .Include(x => x.SpecSheetPhase)
            .Include(x => x.SpecSheetType)
            .FirstOrDefaultAsync(x => x.Id == publishSpecSheetRequest.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Spec Sheet with Id {publishSpecSheetRequest.Id} not found.");

        specSheetToPublish.PublishedByUserId = publishingUser.Id;
        specSheetToPublish.ChangeLog = publishSpecSheetRequest.ChangeLog;

        if (!specSheetToPublish.SpecSheetType!.RequiresApprovalWorkflow)
        {
            specSheetToPublish.IsPublished = true;
            specSheetToPublish.PublishedDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Published SpecSheet with Id {SpecSheetId} by UserId {UserId} without workflow as the SpecSheetType does not require approval", publishSpecSheetRequest.Id, publishingUser.Id);

            await _specSheetCache.RemoveAsync(publishSpecSheetRequest.Id, cancellationToken);
            return;
        }

        // Begin the workflow by creating the first workflow step for the spec sheet
        var workflowStep = new DbWorkflowStep
        {
            Id = 0,
            SpecSheetId = publishSpecSheetRequest.Id,
            State = WorkflowState.Pending,
            StartedDateTime = DateTime.UtcNow,
            Actors = publishSpecSheetRequest.Approvers
                .Select(approver => new DbWorkflowStepActor
                {
                    Id = 0,
                    WorkflowStepId = 0,
                    State = WorkflowState.Pending,
                    ActorUserId = approver.Id,
                    ActionedDateTime = null,
                    Comment = null
                })
                .ToList()
        };

        specSheetToPublish.WorkflowSteps.Add(workflowStep);

        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var approver in publishSpecSheetRequest.Approvers)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == approver.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Approver user with Id {approver.Id} not found.");

            _logger.LogInformation("Sending approval notification email to UserId {UserId} for SpecSheetId {SpecSheetId}", user.Id, specSheetToPublish.Id);

            var emailArgs = new SpecSheetEmailArgs
            {
                SpecSheetId = specSheetToPublish.Id,
                SpecSheetPhaseId = specSheetToPublish.SpecSheetPhaseId,
                SpecSheetTypeId = specSheetToPublish.SpecSheetTypeId,
                TypeName = specSheetToPublish.SpecSheetType!.Type,
                PhaseName = specSheetToPublish.SpecSheetPhase!.Phase,
                Content = publishSpecSheetRequest.ChangeLog,
                User = user.MapToDomain(),
            };
            var email = _emailBuilderService.BuildApprovalNotificationEmail(emailArgs);
            await _emailService.SendApprovalEmailAsync(email, cancellationToken);
        }

        await _specSheetCache.RemoveAsync(publishSpecSheetRequest.Id, cancellationToken);

        _logger.LogInformation("Published SpecSheet with Id {SpecSheetId} by UserId {UserId}", publishSpecSheetRequest.Id, publishingUser.Id);
    }

    public async Task DeleteSpecSheetAsync(int id, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Reusable subqueries (translated server-side).
            var tableIds = _dbContext.SpecSheetTables
                .Where(t => t.SpecSheetId == id)
                .Select(t => t.Id);

            var groupIds = _dbContext.SpecSheetTableGroups
                .Where(g => tableIds.Contains(g.SpecSheetTableId))
                .Select(g => g.Id);

            var rowIds = _dbContext.SpecSheetTableRows
                .Where(r => groupIds.Contains(r.SpecSheetTableGroupId))
                .Select(r => r.Id);

            var cellIds = _dbContext.SpecSheetTableCells
                .Where(c => rowIds.Contains(c.SpecSheetTableRowId))
                .Select(c => c.Id);

            var tableWidgetIds = _dbContext.SpecSheetTableWidgets
                .Where(w => tableIds.Contains(w.SpecSheetTableId))
                .Select(w => w.Id);

            var groupWidgetIds = _dbContext.SpecSheetTableGroupWidgets
                .Where(w => groupIds.Contains(w.SpecSheetTableGroupId))
                .Select(w => w.Id);

            var workflowStepIds = _dbContext.WorkflowSteps
                .Where(ws => ws.SpecSheetId == id)
                .Select(ws => ws.Id);

            // Delete deepest dependents first.
            await _dbContext.NumericValues
                .Where(v => cellIds.Contains(v.SpecSheetTableCellId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.TextValues
                .Where(v => cellIds.Contains(v.SpecSheetTableCellId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.PairValues
                .Where(v => cellIds.Contains(v.SpecSheetTableCellId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.LinkedSpecSheetValues
                .Where(v => cellIds.Contains(v.SpecSheetTableCellId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.TableWidgetValues
                .Where(v => tableWidgetIds.Contains(v.SpecSheetTableWidgetId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.TableGroupWidgetValues
                .Where(v => groupWidgetIds.Contains(v.SpecSheetTableGroupWidgetId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.SpecSheetTableCells
                .Where(c => rowIds.Contains(c.SpecSheetTableRowId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.SpecSheetTableRows
                .Where(r => groupIds.Contains(r.SpecSheetTableGroupId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.SpecSheetTableGroupWidgets
                .Where(w => groupIds.Contains(w.SpecSheetTableGroupId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.SpecSheetTableGroups
                .Where(g => tableIds.Contains(g.SpecSheetTableId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.SpecSheetTableWidgets
                .Where(w => tableIds.Contains(w.SpecSheetTableId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.SpecSheetTableNotes
                .Where(n => tableIds.Contains(n.SpecSheetTableId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.WorkflowStepActors
                .Where(a => workflowStepIds.Contains(a.WorkflowStepId))
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.WorkflowSteps
                .Where(ws => ws.SpecSheetId == id)
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.SpecSheetExternalReferences
                .Where(r => r.SpecSheetId == id)
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.SpecSheetPropertyValues
                .Where(p => p.SpecSheetId == id)
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.SpecSheetTables
                .Where(t => t.SpecSheetId == id)
                .ExecuteDeleteAsync(cancellationToken);

            var deletedSpecSheets = await _dbContext.SpecSheets
                .Where(s => s.Id == id)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedSpecSheets == 0)
            {
                throw new KeyNotFoundException($"Spec Sheet with Id {id} not found.");
            }

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Deleted SpecSheet with Id {SpecSheetId} using set-based deletes", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SpecSheet with Id {SpecSheetId}", id);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateWorkflowStepActorStateAsync(UpdateWorkflowStepActorRequest updateWorkflowStepActorRequest, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var respondingUser = await _userContext.EnsureUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Responding user not found in the context.");

        try
        {
            var workflowStep = await _dbContext.WorkflowSteps
                .Include(ws => ws.Actors)
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.Id == updateWorkflowStepActorRequest.WorkflowStepId, cancellationToken)
                ?? throw new KeyNotFoundException($"Workflow step with Id {updateWorkflowStepActorRequest.WorkflowStepId} not found");
            var specSheet = await _dbContext.SpecSheets
                .Include(ss => ss.SpecSheetPhase)
                .Include(ss => ss.SpecSheetType)
                .AsSplitQuery()
                .FirstOrDefaultAsync(ss => ss.Id == workflowStep.SpecSheetId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Spec Sheet with Id {workflowStep.SpecSheetId} not found.");

            var workflowStepActor = workflowStep.Actors.FirstOrDefault(a => a.Id == updateWorkflowStepActorRequest.WorkflowStepActorId)
                ?? throw new KeyNotFoundException($"Workflow step actor with Id {updateWorkflowStepActorRequest.WorkflowStepActorId} not found in workflow step with Id {updateWorkflowStepActorRequest.WorkflowStepId}");

            workflowStepActor.State = updateWorkflowStepActorRequest.State;
            workflowStepActor.Comment = updateWorkflowStepActorRequest.Comment;
            workflowStepActor.ActionedDateTime = DateTime.UtcNow;

            _logger.LogInformation("Updated WorkflowStepActor with Id {WorkflowStepActorId} to state {State}", workflowStepActor.Id, workflowStepActor.State);

            var emailsToSend = new List<EmailArgs>();

            // Reject the whole step if any actor rejects.
            if (updateWorkflowStepActorRequest.State == WorkflowState.Rejected)
            {
                var publishedByUser = await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == specSheet.PublishedByUserId, cancellationToken)
                    ?? throw new KeyNotFoundException($"PublishedByUser with Id {specSheet.PublishedByUserId} not found.");

                _logger.LogInformation("SpecSheet with Id {SpecSheetId} has been unpublished due to rejection in WorkflowStepActor with Id {WorkflowStepActorId}", workflowStep.SpecSheetId, workflowStepActor.Id);

                specSheet.PublishedByUserId = null;
                specSheet.PublishedDateTime = null;
                specSheet.IsPublished = false;
                workflowStep.State = WorkflowState.Rejected;

                var emailArgs = new SpecSheetEmailArgs
                {
                    SpecSheetId = specSheet.Id,
                    SpecSheetPhaseId = specSheet.SpecSheetPhaseId,
                    SpecSheetTypeId = specSheet.SpecSheetTypeId,
                    TypeName = specSheet.SpecSheetType!.Type,
                    PhaseName = specSheet.SpecSheetPhase!.Phase,
                    Content = updateWorkflowStepActorRequest.Comment!,
                    User = publishedByUser.MapToDomain(),
                    RejectedBy = respondingUser.DisplayName
                };
                var email = _emailBuilderService.BuildRejectionNotificationEmail(emailArgs);
                emailsToSend.Add(email);

                _logger.LogInformation("Built rejection notification email for UserId {UserId} for SpecSheetId {SpecSheetId}", publishedByUser.Id, specSheet.Id);
            }

            // Approve the step if all actors have approved and set the spec sheet to published.
            if (workflowStep.Actors.All(a => a.State == WorkflowState.Approved))
            {
                workflowStep.State = WorkflowState.Approved;
                specSheet.IsPublished = true;
                specSheet.PublishedDateTime = DateTime.UtcNow;

                _logger.LogInformation("WorkflowStep with Id {WorkflowStepId} has been approved as all actors have approved", workflowStep.Id);

                var phaseIdToMatch = specSheet.SpecSheetPhase!.ParentSpecSheetPhaseId ?? specSheet.SpecSheetPhaseId;

                // Get all the observers for the phase
                var observersForPhase = await _dbContext.SpecSheetPhaseObservers
                    .Where(o => o.SpecSheetPhaseId == phaseIdToMatch)
                    .Include(o => o.User)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .ToListAsync(cancellationToken);
                foreach (var observer in observersForPhase)
                {
                    var emailArgs = new SpecSheetEmailArgs
                    {
                        SpecSheetId = specSheet.Id,
                        SpecSheetPhaseId = specSheet.SpecSheetPhaseId,
                        SpecSheetTypeId = specSheet.SpecSheetTypeId,
                        TypeName = specSheet.SpecSheetType!.Type,
                        PhaseName = specSheet.SpecSheetPhase!.Phase,
                        Content = specSheet.ChangeLog!,
                        User = observer.User!.MapToDomain(),
                    };
                    var email = _emailBuilderService.BuildPublishedNotificationEmail(emailArgs);
                    emailsToSend.Add(email);

                    _logger.LogInformation("Built published notification email for observer UserId {UserId} for SpecSheetId {SpecSheetId}", observer.UserId, specSheet.Id);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            foreach (var emailToSend in emailsToSend)
            {
                switch (emailToSend.BodyContent)
                {
                    case RejectionNotificationContent:
                        _logger.LogInformation("Sending rejection notification email to UserId {UserId} for SpecSheetId {SpecSheetId}", emailToSend.To, specSheet.Id);
                        await _emailService.SendRejectionEmailAsync(emailToSend, cancellationToken);
                        break;
                    case PublishedNotificationContent:
                        _logger.LogInformation("Sending published notification email to UserId {UserId} for SpecSheetId {SpecSheetId}", emailToSend.To, specSheet.Id);
                        await _emailService.SendPublishedEmailAsync(emailToSend, cancellationToken);
                        break;
                }
            }

            await _specSheetCache.RemoveAsync(specSheet.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Workflow with Id {WorkflowId}", updateWorkflowStepActorRequest.WorkflowStepId);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ReassignWorkflowStepActorsAsync(ReassignWorkflowStepActorsRequest reassignWorkflowStepActorsRequest, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var specSheet = await _dbContext.SpecSheets
                .Include(ss => ss.SpecSheetPhase)
                .Include(ss => ss.SpecSheetType)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(ss => ss.Id == reassignWorkflowStepActorsRequest.SpecSheetId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Spec Sheet with Id {reassignWorkflowStepActorsRequest.SpecSheetId} not found.");

            foreach (var actorToDeleteId in reassignWorkflowStepActorsRequest.DeleteWorkflowStepActorRequests)
            {
                var dbActorToDelete = await _dbContext.WorkflowStepActors
                    .FirstOrDefaultAsync(a => a.Id == actorToDeleteId, cancellationToken)
                    ?? throw new KeyNotFoundException($"WorkflowStepActor with Id {actorToDeleteId} not found.");

                _dbContext.WorkflowStepActors.Remove(dbActorToDelete);
                _logger.LogInformation("Deleted WorkflowStepActor with Id {WorkflowStepActorId} from WorkflowStep with Id {WorkflowStepId}", dbActorToDelete.Id, reassignWorkflowStepActorsRequest.WorkflowStepId);
            }

            foreach (var actorToCreate in reassignWorkflowStepActorsRequest.CreateWorkflowStepActorRequests)
            {
                var newDbActor = new DbWorkflowStepActor
                {
                    Id = 0,
                    WorkflowStepId = reassignWorkflowStepActorsRequest.WorkflowStepId,
                    ActorUserId = actorToCreate,
                    State = WorkflowState.Pending,
                    ActionedDateTime = null,
                    Comment = null
                };

                await _dbContext.WorkflowStepActors.AddAsync(newDbActor, cancellationToken);
                _logger.LogInformation("Added WorkflowStepActor for UserId {UserId} to WorkflowStep with Id {WorkflowStepId}", actorToCreate, reassignWorkflowStepActorsRequest.WorkflowStepId);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            foreach (var actorToCreate in reassignWorkflowStepActorsRequest.CreateWorkflowStepActorRequests)
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == actorToCreate, cancellationToken)
                    ?? throw new KeyNotFoundException($"User with Id {actorToCreate} not found.");

                _logger.LogInformation("Sending approval notification email to UserId {UserId} for SpecSheetId {SpecSheetId}", user.Id, specSheet.Id);

                var emailArgs = new SpecSheetEmailArgs
                {
                    SpecSheetId = specSheet.Id,
                    SpecSheetPhaseId = specSheet.SpecSheetPhaseId,
                    SpecSheetTypeId = specSheet.SpecSheetTypeId,
                    TypeName = specSheet.SpecSheetType!.Type,
                    PhaseName = specSheet.SpecSheetPhase!.Phase,
                    Content = specSheet.ChangeLog!,
                    User = user.MapToDomain(),
                };
                var email = _emailBuilderService.BuildApprovalNotificationEmail(emailArgs);
                await _emailService.SendApprovalEmailAsync(email, cancellationToken);
            }

            await _specSheetCache.RemoveAsync(specSheet.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reassigning WorkflowStepActors for WorkflowStep with Id {WorkflowStepId}", reassignWorkflowStepActorsRequest.WorkflowStepId);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateSystemReferencesAsync(ICollection<ExternalSystemReferenceRequest> systemReferences,
        IDbContextTransaction? transactionContext,
        CancellationToken cancellationToken)
    {
        var transaction = transactionContext ?? await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Group the incoming references by SpecSheetId and SystemName to minimize database calls
            var groupedSystemReferences = systemReferences
                .GroupBy(r => new { r.PUSpecSheetId, r.SystemName })
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var kvp in groupedSystemReferences)
            {
                var specSheetId = kvp.Key.PUSpecSheetId;
                var systemName = kvp.Key.SystemName;
                var systemReferencesForSpecSheet = kvp.Value;

                var existingReferencesForSpecSheet = await _dbContext.SpecSheetExternalReferences
                    .Where(r => r.SpecSheetId == specSheetId && r.SystemName == systemName)
                    .ToListAsync(cancellationToken);

                foreach (var systemReference in systemReferencesForSpecSheet)
                {
                    var existingReference = existingReferencesForSpecSheet.FirstOrDefault(r => r.ReferenceId == systemReference.ReferenceId);
                    if (existingReference is not null)
                    {
                        existingReference.ExternalId = systemReference.ExternalId;
                        existingReference.SystemName = systemReference.SystemName;
                        existingReference.OwnerName = systemReference.OwnerName;
                        existingReference.Url = systemReference.Url;
                        existingReference.Type = Enum.GetName(systemReference.Type)?.ToLowerInvariant() ?? string.Empty;
                        existingReference.Grouping = systemReference.Grouping;
                        existingReference.SystemIconUrl = systemReference.SystemIconUrl;
                        existingReference.Description = systemReference.Description;

                        _logger.LogInformation("Updated existing system reference with ReferenceId {ReferenceId} for SpecSheet with Id {SpecSheetId}", systemReference.ReferenceId, specSheetId);
                    }
                    else
                    {
                        var newDbReference = new DbSpecSheetExternalReference
                        {
                            Id = 0,
                            SpecSheetId = systemReference.PUSpecSheetId,
                            ExternalId = systemReference.ExternalId,
                            ReferenceId = systemReference.ReferenceId,
                            SystemName = systemReference.SystemName,
                            OwnerName = systemReference.OwnerName,
                            Url = systemReference.Url,
                            Type = Enum.GetName(systemReference.Type)?.ToLowerInvariant() ?? string.Empty,
                            Grouping = systemReference.Grouping,
                            SystemIconUrl = systemReference.SystemIconUrl,
                            Description = systemReference.Description
                        };
                        await _dbContext.SpecSheetExternalReferences.AddAsync(newDbReference, cancellationToken);

                        _logger.LogInformation("Added new system reference with ReferenceId {ReferenceId} for SpecSheet with Id {SpecSheetId}", systemReference.ReferenceId, specSheetId);
                    }
                }

                // Find references that exist in the database but were not included in the update request and remove them
                var referenceIdsInRequest = systemReferences.Select(r => r.ReferenceId).ToHashSet();
                var referencesToRemove = existingReferencesForSpecSheet.Where(r => !referenceIdsInRequest.Contains(r.ReferenceId)).ToList();
                if (referencesToRemove.Count > 0)
                {
                    _dbContext.SpecSheetExternalReferences.RemoveRange(referencesToRemove);
                    _logger.LogInformation("Removed {RemovedReferenceCount} system references for SpecSheet with Id {SpecSheetId} that were not included in the update request", referencesToRemove.Count, specSheetId);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (transactionContext == null)
            {
                await transaction.CommitAsync(cancellationToken);
                await transaction.DisposeAsync();
            }


            foreach (var kvp in groupedSystemReferences)
            {
                await _specSheetCache.RemoveAsync(kvp.Key.PUSpecSheetId, cancellationToken);
            }

            _logger.LogInformation("Updated system references");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system references");
            if (transactionContext == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                await transaction.DisposeAsync();
            }

            throw;
        }
    }

    public async Task DeleteSystemReferencesByExternalIdAsync(int externalId, CancellationToken cancellationToken)
    {
        var references = await _dbContext.SpecSheetExternalReferences
            .Where(r => r.ExternalId == externalId)
            .ToListAsync(cancellationToken);

        if (references.Count == 0)
        {
            _logger.LogWarning("No system references found for SpecSheet with ExternalId {ExternalId} to delete", externalId);
            return;
        }

        _dbContext.SpecSheetExternalReferences.RemoveRange(references);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted {DeletedReferenceCount} system references for SpecSheet with ExternalId {ExternalId}", references.Count, externalId);

        await _specSheetCache.RemoveAsync(externalId, cancellationToken);
    }

    #endregion Spec Sheets

    #region Spec Sheet Tables

    public async Task<IReadOnlyList<SpecSheetTable>> GetAllUniqueSpecSheetTablesAsync(CancellationToken cancellationToken)
    {
        var currentUser = await _userContext.EnsureUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Current user not found in the context.");

        var specSheetTables = await _dbContext.SpecSheetTables
            .Include(x => x.CreatedByUser)
            .Include(x => x.LastModifiedByUser)
            .AsNoTracking()
            .MapToDomain()
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var tablePermissions = await _dbContext.SpecSheetTablePermissions
            .Where(x => x.UserId == currentUser.Id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Remove duplicates based on ReferenceId, keep the latest created
        var uniqueSpecSheetTables = specSheetTables
            .GroupBy(t => t.ReferenceId)
            .Select(g => g.OrderByDescending(t => t.CreatedDateTime).First())
            .ToList();

        // Filter based on permissions
        uniqueSpecSheetTables = uniqueSpecSheetTables
            .Where(t => tablePermissions.Any(p => p.TableReferenceId == t.ReferenceId))
            .ToList();

        _logger.LogInformation("Retrieved {SpecSheetTableCount} unique SpecSheetTables", uniqueSpecSheetTables.Count);
        return uniqueSpecSheetTables;
    }

    #endregion Spec Sheet Tables

    #region Build    

    #region Spec Sheet Tables

    private static List<DbSpecSheetTable> BuildNewDbSpecSheetTables(User user, ICollection<SpecSheetTable> specSheetTables)
    {
        var dbSpecSheetTables = new List<DbSpecSheetTable>();
        foreach (var specSheetTable in specSheetTables)
        {
            var dbSpecSheetTable = new DbSpecSheetTable
            {
                Id = 0,
                SpecSheetId = 0,
                TableTemplateId = specSheetTable.TableTemplateId,
                ReferenceId = specSheetTable.ReferenceId,
                Title = specSheetTable.Title,
                DisplayOrder = specSheetTable.DisplayOrder,
                CreatedByUserId = user.Id,
                CreatedDateTime = specSheetTable.CreatedDateTime,
                IsDeleted = specSheetTable.IsDeleted,
                Groups = BuildNewDbSpecSheetTableGroupsAsync(user, specSheetTable.Groups),
                Widgets = BuildNewDbSpecSheetTableWidgets(user, specSheetTable.Widgets),
                Notes = BuildNewDbSpecSheetTableNotes(user, specSheetTable.Notes),
                ConfigurationOverride = specSheetTable.ConfigurationOverride,
            };

            dbSpecSheetTables.Add(dbSpecSheetTable);
        }
        return dbSpecSheetTables;
    }

    #endregion Spec Sheet Tables

    #region Spec Sheet Table Widgets

    private static List<DbSpecSheetTableWidget> BuildNewDbSpecSheetTableWidgets(User user,
        ICollection<SpecSheetTableWidget> specSheetTableWidgets)
    {
        var dbSpecSheetTableWidgets = new List<DbSpecSheetTableWidget>();
        foreach (var specSheetTableWidget in specSheetTableWidgets)
        {
            var dbSpecSheetTableWidget = new DbSpecSheetTableWidget
            {
                Id = 0,
                SpecSheetTableId = 0,
                TableTemplateWidgetId = specSheetTableWidget.TableTemplateWidgetId,
                Value = new DbTableWidgetValue
                {
                    Id = 0,
                    SpecSheetTableWidgetId = 0,
                    Value = specSheetTableWidget.Value.Value
                }
            };

            dbSpecSheetTableWidgets.Add(dbSpecSheetTableWidget);
        }
        return dbSpecSheetTableWidgets;
    }

    #endregion Spec Sheet Table Widgets

    #region Spec Sheet Table Notes

    private static ICollection<DbSpecSheetTableNote> BuildNewDbSpecSheetTableNotes(User user, ICollection<SpecSheetTableNote> notes)
    {
        var dbNotes = new List<DbSpecSheetTableNote>();
        foreach (var note in notes)
        {
            var dbNote = new DbSpecSheetTableNote
            {
                Id = 0,
                SpecSheetTableId = 0,
                Note = note.Note,
                SourceSystem = note.SourceSystem,
                IsObsolete = note.IsObsolete,
                CreatedByUserId = user.Id,
                CreatedDateTime = DateTime.UtcNow
            };

            dbNotes.Add(dbNote);
        }

        return dbNotes;
    }

    #endregion Spec Sheet Table Notes

    #region Spec Sheet Table Groups

    private static List<DbSpecSheetTableGroup> BuildNewDbSpecSheetTableGroupsAsync(User user, ICollection<SpecSheetTableGroup> specSheetTableGroups)
    {
        var dbSpecSheetTableGroups = new List<DbSpecSheetTableGroup>();
        var groupMap = new Dictionary<int, DbSpecSheetTableGroup>();

        foreach (var specSheetTableGroup in specSheetTableGroups)
        {
            var dbSpecSheetTableGroup = new DbSpecSheetTableGroup
            {
                Id = 0,
                SpecSheetTableId = 0,
                TableGroupTemplateId = specSheetTableGroup.TableGroupTemplateId,
                LinkedSpecSheetTableGroupId = null,
                ReferenceId = specSheetTableGroup.ReferenceId,
                Order = specSheetTableGroup.Order,
                CreatedByUserId = user.Id,
                CreatedDateTime = DateTime.UtcNow,
                IsDeleted = false,
                Rows = BuildNewDbSpecSheetTableRows(user, specSheetTableGroup.Rows),
                Widgets = BuildNewDbSpecSheetTableWidgets(user, specSheetTableGroup.Widgets),
            };

            groupMap[specSheetTableGroup.Id] = dbSpecSheetTableGroup;
            dbSpecSheetTableGroups.Add(dbSpecSheetTableGroup);
        }

        foreach (var specSheetTableGroup in specSheetTableGroups)
        {
            if (specSheetTableGroup.LinkedSpecSheetTableGroupId.HasValue &&
                groupMap.TryGetValue(specSheetTableGroup.LinkedSpecSheetTableGroupId.Value, out var linkedGroup))
            {
                var dbGroup = groupMap[specSheetTableGroup.Id];
                dbGroup.LinkedSpecSheetTableGroup = linkedGroup;
            }
        }

        return dbSpecSheetTableGroups;
    }

    #endregion Spec Sheet Table Groups

    #region Spec Sheet Widgets

    private static List<DbSpecSheetTableGroupWidget> BuildNewDbSpecSheetTableWidgets(User user,
        ICollection<SpecSheetTableGroupWidget> specSheetTableGroupWidgets)
    {
        var dbSpecSheetTableGroupWidgets = new List<DbSpecSheetTableGroupWidget>();
        foreach (var specSheetTableGroupWidget in specSheetTableGroupWidgets)
        {
            var dbSpecSheetTableGroupWidget = new DbSpecSheetTableGroupWidget
            {
                Id = 0,
                SpecSheetTableGroupId = 0,
                TableGroupTemplateWidgetId = specSheetTableGroupWidget.TableGroupTemplateWidgetId,
                Value = new DbTableGroupWidgetValue
                {
                    Id = 0,
                    SpecSheetTableGroupWidgetId = 0,
                    Value = specSheetTableGroupWidget.Value.Value
                }
            };

            dbSpecSheetTableGroupWidgets.Add(dbSpecSheetTableGroupWidget);
        }

        return dbSpecSheetTableGroupWidgets;
    }

    #endregion Spec Sheet Widgets

    #region Spec Sheet Table Rows

    private static List<DbSpecSheetTableRow> BuildNewDbSpecSheetTableRows(User user, ICollection<SpecSheetTableRow> specSheetTableRows)
    {
        var dbSpecSheetTableRows = new List<DbSpecSheetTableRow>();
        foreach (var specSheetTableRow in specSheetTableRows)
        {
            var dbSpecSheetTableRow = new DbSpecSheetTableRow
            {
                Id = 0,
                SpecSheetTableGroupId = 0,
                TableRowTemplateId = specSheetTableRow.TableRowTemplateId,
                ReferenceId = specSheetTableRow.ReferenceId,
                RowNumber = specSheetTableRow.RowNumber,
                CreatedByUserId = user.Id,
                CreatedDateTime = specSheetTableRow.CreatedDateTime,
                IsDeleted = specSheetTableRow.IsDeleted,
                Cells = BuildNewDbSpecSheetTableCells(user, specSheetTableRow.Cells),
            };

            dbSpecSheetTableRows.Add(dbSpecSheetTableRow);
        }

        return dbSpecSheetTableRows;
    }

    #endregion Spec Sheet Table Rows

    #region Spec Sheet Table Row Cells

    private static List<DbSpecSheetTableCell> BuildNewDbSpecSheetTableCells(User user, ICollection<SpecSheetTableCell> specSheetTableCells)
    {
        var dbSpecSheetTableCells = new List<DbSpecSheetTableCell>();
        foreach (var specSheetTableCell in specSheetTableCells)
        {
            var dbSpecSheetTableCell = new DbSpecSheetTableCell
            {
                Id = 0,
                SpecSheetTableRowId = specSheetTableCell.SpecSheetTableRowId,
                TableCellTypeId = specSheetTableCell.TableCellTypeId,
                CellConfigurationId = specSheetTableCell.CellConfigurationId,
                ReferenceId = specSheetTableCell.ReferenceId,
                RowNumber = specSheetTableCell.RowNumber,
                ColumnNumber = specSheetTableCell.ColumnNumber,
                ColSpan = specSheetTableCell.ColSpan,
                RowSpan = specSheetTableCell.RowSpan,
                CreatedByUserId = user.Id,
                CreatedDateTime = specSheetTableCell.CreatedDateTime,
                IsDeleted = specSheetTableCell.IsDeleted,
                ConfigurationOverride = specSheetTableCell.ConfigurationOverride,
                NumericValue = specSheetTableCell.CellValue is NumericValue numericValue
                    ? BuildNewDbNumericValue(numericValue)
                    : null,
                TextValue = specSheetTableCell.CellValue is TextValue textValue
                    ? BuildNewDbTextValue(textValue)
                    : null,
                PairValue = specSheetTableCell.CellValue is PairValue pairValue
                    ? BuildNewDbPairValue(pairValue)
                    : null,
                LinkedSpecSheetValue = specSheetTableCell.CellValue is LinkedSpecSheetValue linkedSpecSheetValue
                    ? BuildNewDbLinkedSpecSheetValue(linkedSpecSheetValue)
                    : null,
            };

            dbSpecSheetTableCells.Add(dbSpecSheetTableCell);
        }
        return dbSpecSheetTableCells;
    }

    #endregion Spec Sheet Table Row Cells

    #region Values

    private static DbNumericValue BuildNewDbNumericValue(NumericValue cellValue)
    {
        return new DbNumericValue
        {
            Id = 0,
            SpecSheetTableCellId = cellValue.SpecSheetTableCellId,
            Value = cellValue.Value,
        };
    }

    private static DbTextValue BuildNewDbTextValue(TextValue cellValue)
    {
        return new DbTextValue
        {
            Id = 0,
            SpecSheetTableCellId = cellValue.SpecSheetTableCellId,
            Value = cellValue.Value,
        };
    }

    private static DbPairValue BuildNewDbPairValue(PairValue cellValue)
    {
        return new DbPairValue
        {
            Id = 0,
            SpecSheetTableCellId = cellValue.SpecSheetTableCellId,
            PairValueX = cellValue.ValueX,
            PairValueY = cellValue.ValueY,
        };
    }

    private static DbLinkedSpecSheetValue BuildNewDbLinkedSpecSheetValue(LinkedSpecSheetValue cellValue)
    {
        return new DbLinkedSpecSheetValue
        {
            Id = 0,
            SpecSheetTableCellId = cellValue.SpecSheetTableCellId,
            LinkedSpecSheetTypeId = cellValue.LinkedSpecSheetTypeId,
            LinkedSpecSheetPhaseId = cellValue.LinkedSpecSheetPhaseId,
            LinkedReferenceId = cellValue.LinkedReferenceId,
            TableReferenceId = cellValue.TableReferenceId,
        };
    }

    #endregion Values

    #endregion Build
}
