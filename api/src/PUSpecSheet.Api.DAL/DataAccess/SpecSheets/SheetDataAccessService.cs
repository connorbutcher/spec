using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Requests.SpecSheets;
using PUSpecSheet.Abstractions.Requests.Sheets;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Common.Context;
using PUSpecSheet.Api.DAL.Caching;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Sheets;
using PUSpecSheet.Api.DAL.Processors.Drafts;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets;

internal class SheetDataAccessService : ISheetDataAccessService
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly SheetReader _sheetReader;
    private readonly SheetEditSwitch _editSwitch;
    private readonly IUserContext _userContext;
    private readonly SheetDraftCreator _draftCreator;
    private readonly SheetDraftDeleter _draftDeleter;
    private readonly SheetDraftWorkflow _workflow;
    private readonly ISpecSheetCache _specSheetCache;
    private readonly ILogger<ISheetDataAccessService> _logger;

    public SheetDataAccessService(PUSpecSheetDbContext dbContext,
        SheetReader sheetReader,
        SheetEditSwitch editSwitch,
        IUserContext userContext,
        SheetDraftCreator draftCreator,
        SheetDraftDeleter draftDeleter,
        SheetDraftWorkflow workflow,
        ISpecSheetCache specSheetCache,
        ILogger<ISheetDataAccessService> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(sheetReader, nameof(sheetReader));
        ArgumentNullException.ThrowIfNull(editSwitch, nameof(editSwitch));
        ArgumentNullException.ThrowIfNull(userContext, nameof(userContext));
        ArgumentNullException.ThrowIfNull(draftCreator, nameof(draftCreator));
        ArgumentNullException.ThrowIfNull(draftDeleter, nameof(draftDeleter));
        ArgumentNullException.ThrowIfNull(workflow, nameof(workflow));
        ArgumentNullException.ThrowIfNull(specSheetCache, nameof(specSheetCache));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _dbContext = dbContext;
        _sheetReader = sheetReader;
        _editSwitch = editSwitch;
        _userContext = userContext;
        _draftCreator = draftCreator;
        _draftDeleter = draftDeleter;
        _workflow = workflow;
        _specSheetCache = specSheetCache;
        _logger = logger;
    }

    public async Task<Sheet?> GetPublishedSheetAsync(int sheetId, int? versionNumber, CancellationToken cancellationToken)
    {
        var sheet = await _sheetReader.ReadPublishedAsync(sheetId, versionNumber, cancellationToken);
        return sheet is null ? null : await TrimTablesAsync(sheet, cancellationToken);
    }

    public async Task<Sheet?> GetDraftSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        var sheet = await _sheetReader.ReadDraftAsync(sheetId, cancellationToken);
        return sheet is null ? null : await TrimTablesAsync(sheet, cancellationToken);
    }

    public async Task<IReadOnlyList<SheetVersion>> GetSheetVersionsAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _sheetReader.ReadVersionsAsync(sheetId, cancellationToken);
    }

    public async Task<IReadOnlyList<SheetSubmission>?> GetSheetSubmissionsAsync(int sheetId, int? versionNumber, CancellationToken cancellationToken)
    {
        return await _sheetReader.ReadSubmissionsAsync(sheetId, versionNumber, cancellationToken);
    }

    public async Task<Sheet?> GetLatestPublishedSheetByPhaseAndTypeAsync(int phaseId, int sheetTypeId, DateTime? publishedBy, CancellationToken cancellationToken)
    {
        var sheetId = await _sheetReader.FindSheetIdAsync(phaseId, sheetTypeId, cancellationToken);
        if (sheetId is null)
        {
            return null;
        }

        var sheet = await _sheetReader.ReadPublishedByAsync(sheetId.Value, publishedBy, cancellationToken);
        return sheet is null ? null : await TrimTablesAsync(sheet, cancellationToken);
    }

    public async Task<IReadOnlyList<SheetSummary>> GetSheetSummariesByPhaseAsync(int phaseId, bool includeChildren, CancellationToken cancellationToken)
    {
        var phaseIds = new List<int> { phaseId };
        if (includeChildren)
        {
            phaseIds.AddRange(await _dbContext.SpecSheetPhases
                .AsNoTracking()
                .Where(phase => phase.ParentSpecSheetPhaseId == phaseId)
                .Select(phase => phase.Id)
                .ToListAsync(cancellationToken));
        }

        return await _sheetReader.ReadSummariesAsync(sheet => phaseIds.Contains(sheet.SpecSheetPhaseId), cancellationToken);
    }

    public async Task<IReadOnlyList<SheetSummary>> GetSheetSummariesByTypeAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        return await _sheetReader.ReadSummariesAsync(sheet => sheet.SpecSheetTypeId == sheetTypeId, cancellationToken);
    }

    public async Task<SheetSummary?> GetSheetSummaryByPhaseAndTypeAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        var summaries = await _sheetReader.ReadSummariesAsync(sheet => sheet.SpecSheetPhaseId == phaseId && sheet.SpecSheetTypeId == sheetTypeId, cancellationToken);
        return summaries.FirstOrDefault();
    }

    public async Task<SheetLocation?> FindSheetAsync(int specSheetId, CancellationToken cancellationToken)
    {
        return await _sheetReader.FindLocationAsync(specSheetId, _editSwitch, cancellationToken);
    }

    #region Writes

    public async Task<SheetLocation> CreateDraftAsync(CreateSheetRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        EnsureOnRevisions(request.SpecSheetTypeId, $"Sheet type with Id {request.SpecSheetTypeId}");
        var currentUser = await GetCurrentUserAsync(cancellationToken);

        var specSheetId = await _draftCreator.CreateAsync(new CreateSpecSheetRequest
        {
            SpecSheetPhaseId = request.SpecSheetPhaseId,
            SpecSheetTypeId = request.SpecSheetTypeId,
            PropertyValues = request.PropertyValues
        }, currentUser, cancellationToken);

        var sheetId = await _dbContext.Sheets
            .AsNoTracking()
            .Where(s => s.SpecSheetPhaseId == request.SpecSheetPhaseId && s.SpecSheetTypeId == request.SpecSheetTypeId)
            .Select(s => s.Id)
            .FirstAsync(cancellationToken);

        _logger.LogInformation("Created the draft of Sheet with Id {SheetId} by UserId {UserId}", sheetId, currentUser.Id);
        return new SheetLocation { SpecSheetId = specSheetId, SheetId = sheetId, VersionNumber = null, IsDraft = true };
    }

    public async Task PublishAsync(int sheetId, PublishSheetRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var specSheetId = await FindDraftSpecSheetIdAsync(sheetId, cancellationToken);
        await _workflow.PublishAsync(sheetId, specSheetId, currentUser, request.ChangeLog, [.. request.ApproverUserIds.Distinct()], cancellationToken);
    }

    public async Task DeleteDraftAsync(int sheetId, CancellationToken cancellationToken)
    {
        var specSheetId = await FindDraftSpecSheetIdAsync(sheetId, cancellationToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _draftDeleter.DeleteDraftsAsync(sheetId, cancellationToken);

        // The draft's SpecSheets row goes with what hangs off it.
        var workflowStepIds = _dbContext.WorkflowSteps
            .Where(step => step.SpecSheetId == specSheetId)
            .Select(step => step.Id);
        await _dbContext.WorkflowStepActors
            .Where(actor => workflowStepIds.Contains(actor.WorkflowStepId))
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.WorkflowSteps
            .Where(step => step.SpecSheetId == specSheetId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SpecSheetExternalReferences
            .Where(reference => reference.SpecSheetId == specSheetId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SpecSheetPropertyValues
            .Where(property => property.SpecSheetId == specSheetId)
            .ExecuteDeleteAsync(cancellationToken);
        var deleted = await _dbContext.SpecSheets
            .Where(s => s.Id == specSheetId)
            .ExecuteDeleteAsync(cancellationToken);
        if (deleted == 0)
        {
            throw new KeyNotFoundException($"Spec Sheet with Id {specSheetId} not found.");
        }

        await transaction.CommitAsync(cancellationToken);

        await _specSheetCache.RemoveAsync(specSheetId, cancellationToken);
        _logger.LogInformation("Deleted the draft of Sheet with Id {SheetId}", sheetId);
    }

    public async Task DecideSubmissionAsync(int sheetId, DecideSubmissionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (request.Decision == ApproverDecision.Rejected && string.IsNullOrWhiteSpace(request.Comment))
        {
            throw new InvalidOperationException("A rejection needs a comment saying why.");
        }

        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var specSheetId = await FindDraftSpecSheetIdAsync(sheetId, cancellationToken);
        await _workflow.DecideAsync(sheetId, specSheetId, currentUser, request.Decision, request.Comment, cancellationToken);
    }

    public async Task ReassignSubmissionApproversAsync(int sheetId, ReassignSubmissionApproversRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var specSheetId = await FindDraftSpecSheetIdAsync(sheetId, cancellationToken);
        await _workflow.ReassignAsync(sheetId, specSheetId, [.. request.AddedUserIds.Distinct()], [.. request.RemovedUserIds.Distinct()], cancellationToken);
    }

    public async Task ApplyChangesAsync(int sheetId, SheetChanges changes, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(changes, nameof(changes));

        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var specSheetId = await FindDraftSpecSheetIdAsync(sheetId, cancellationToken);

        await new SheetDraftChangeProcessor(_dbContext, currentUser, sheetId, _logger).ApplyChangesAsync(changes.Operations, cancellationToken);

        await _specSheetCache.RemoveAsync(specSheetId, cancellationToken);
        _logger.LogInformation("Applied {OperationCount} change(s) to the draft of Sheet with Id {SheetId} by UserId {UserId}", changes.Operations.Length, sheetId, currentUser.Id);
    }

    private void EnsureOnRevisions(int sheetTypeId, string what)
    {
        if (!_editSwitch.IsOnRevisions(sheetTypeId))
        {
            throw new NotSupportedException($"{what} is still edited on its copies, so it can't be changed through the sheets yet.");
        }
    }

    // The unpublished SpecSheets row that is the sheet's draft: what the spec sheet methods act on.
    private async Task<int> FindDraftSpecSheetIdAsync(int sheetId, CancellationToken cancellationToken)
    {
        var sheet = await _dbContext.Sheets
            .AsNoTracking()
            .Where(s => s.Id == sheetId)
            .Select(s => new { s.SpecSheetTypeId, s.SpecSheetPhaseId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Sheet with Id {sheetId} not found.");
        EnsureOnRevisions(sheet.SpecSheetTypeId, $"Sheet with Id {sheetId}");

        return await _dbContext.SpecSheets
            .AsNoTracking()
            .Where(s => !s.IsPublished && s.SpecSheetTypeId == sheet.SpecSheetTypeId && s.SpecSheetPhaseId == sheet.SpecSheetPhaseId)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Sheet with Id {sheetId} has no draft.");
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        return await _userContext.EnsureUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Current user not found in the context.");
    }

    #endregion Writes

    // A table that has permissions is only shown to the users it has them for, as a copy's is.
    private async Task<Sheet> TrimTablesAsync(Sheet sheet, CancellationToken cancellationToken)
    {
        var currentUser = await _userContext.EnsureUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Current user not found in the context.");

        var tableReferenceIds = sheet.Tables.Select(table => table.ReferenceId).ToList();
        var permissions = await _dbContext.SpecSheetTablePermissions
            .AsNoTracking()
            .Where(permission => tableReferenceIds.Contains(permission.TableReferenceId))
            .ToListAsync(cancellationToken);

        var tables = sheet.Tables
            .Where(table => !permissions.Any(permission => permission.TableReferenceId == table.ReferenceId)
                || permissions.Any(permission => permission.TableReferenceId == table.ReferenceId && permission.UserId == currentUser.Id))
            .ToList();

        return sheet with { Tables = tables };
    }
}
