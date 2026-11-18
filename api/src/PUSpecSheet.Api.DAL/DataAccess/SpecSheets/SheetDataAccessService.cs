using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Requests.SpecSheets;
using PUSpecSheet.Abstractions.Requests.Sheets;
using PUSpecSheet.Abstractions.Requests.Workflow;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Common.Context;
using PUSpecSheet.Api.DAL.Caching;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Sheets;
using PUSpecSheet.Api.DAL.Processors.Drafts;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Schema.Legacy.Workflow;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets;

internal class SheetDataAccessService : ISheetDataAccessService
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly SheetReader _sheetReader;
    private readonly SheetEditSwitch _editSwitch;
    private readonly IUserContext _userContext;
    private readonly ISpecSheetDataAccessService _specSheetDataAccessService;
    private readonly ISpecSheetCache _specSheetCache;
    private readonly ILogger<ISheetDataAccessService> _logger;

    public SheetDataAccessService(PUSpecSheetDbContext dbContext,
        SheetReader sheetReader,
        SheetEditSwitch editSwitch,
        IUserContext userContext,
        ISpecSheetDataAccessService specSheetDataAccessService,
        ISpecSheetCache specSheetCache,
        ILogger<ISheetDataAccessService> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(sheetReader, nameof(sheetReader));
        ArgumentNullException.ThrowIfNull(editSwitch, nameof(editSwitch));
        ArgumentNullException.ThrowIfNull(userContext, nameof(userContext));
        ArgumentNullException.ThrowIfNull(specSheetDataAccessService, nameof(specSheetDataAccessService));
        ArgumentNullException.ThrowIfNull(specSheetCache, nameof(specSheetCache));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _dbContext = dbContext;
        _sheetReader = sheetReader;
        _editSwitch = editSwitch;
        _userContext = userContext;
        _specSheetDataAccessService = specSheetDataAccessService;
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

    // Creating, publishing, deleting and the approval workflow are the spec sheet methods', which keep the draft's
    // SpecSheets row and its workflow steps in step with the sheet's drafts and submissions; the sheet is mapped to
    // that row here. Applying changes needs no row but its id, so it is done directly. These are the one place the
    // obsolete spec sheet methods are still called, until the copies' edits are gone.
#pragma warning disable CS0618
    public async Task<SheetLocation> CreateDraftAsync(CreateSheetRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        EnsureOnRevisions(request.SpecSheetTypeId, $"Sheet type with Id {request.SpecSheetTypeId}");

        var specSheetId = await _specSheetDataAccessService.CreateSpecSheetAsync(new CreateSpecSheetRequest
        {
            SpecSheetPhaseId = request.SpecSheetPhaseId,
            SpecSheetTypeId = request.SpecSheetTypeId,
            PropertyValues = request.PropertyValues
        }, cancellationToken);

        var sheetId = await _dbContext.Sheets
            .AsNoTracking()
            .Where(s => s.SpecSheetPhaseId == request.SpecSheetPhaseId && s.SpecSheetTypeId == request.SpecSheetTypeId)
            .Select(s => s.Id)
            .FirstAsync(cancellationToken);

        return new SheetLocation { SpecSheetId = specSheetId, SheetId = sheetId, VersionNumber = null, IsDraft = true };
    }

    public async Task PublishAsync(int sheetId, PublishSheetRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var specSheetId = await FindDraftSpecSheetIdAsync(sheetId, cancellationToken);

        var approverIds = request.ApproverUserIds.Distinct().ToList();
        var approvers = await _dbContext.Users
            .AsNoTracking()
            .Where(user => approverIds.Contains(user.Id))
            .ToListAsync(cancellationToken);
        if (approvers.Count != approverIds.Count)
        {
            throw new KeyNotFoundException($"An approver of Sheet with Id {sheetId} was not found.");
        }

        await _specSheetDataAccessService.PublishSpecSheetAsync(new PublishSpecSheetRequest
        {
            Id = specSheetId,
            ChangeLog = request.ChangeLog,
            Approvers = [.. approvers.Select(user => user.MapToDomain())]
        }, cancellationToken);
    }

    public async Task DeleteDraftAsync(int sheetId, CancellationToken cancellationToken)
    {
        var specSheetId = await FindDraftSpecSheetIdAsync(sheetId, cancellationToken);
        await _specSheetDataAccessService.DeleteSpecSheetAsync(specSheetId, cancellationToken);
    }

    public async Task DecideSubmissionAsync(int sheetId, DecideSubmissionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (request.Decision == ApproverDecision.Rejected && string.IsNullOrWhiteSpace(request.Comment))
        {
            throw new InvalidOperationException("A rejection needs a comment saying why.");
        }

        var currentUser = await _userContext.EnsureUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Current user not found in the context.");

        var step = await FindPendingStepAsync(sheetId, cancellationToken);
        var actor = step.Actors.FirstOrDefault(a => a.ActorUserId == currentUser.Id)
            ?? throw new KeyNotFoundException($"User with Id {currentUser.Id} isn't an approver of the submission of Sheet with Id {sheetId}.");

        await _specSheetDataAccessService.UpdateWorkflowStepActorStateAsync(new UpdateWorkflowStepActorRequest
        {
            WorkflowStepActorId = actor.Id,
            WorkflowStepId = step.Id,
            State = request.Decision switch
            {
                ApproverDecision.Approved => WorkflowState.Approved,
                ApproverDecision.Rejected => WorkflowState.Rejected,
                _ => WorkflowState.Pending
            },
            Comment = request.Comment
        }, cancellationToken);
    }

    public async Task ReassignSubmissionApproversAsync(int sheetId, ReassignSubmissionApproversRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var step = await FindPendingStepAsync(sheetId, cancellationToken);
        var removedActorIds = new List<int>();
        foreach (var userId in request.RemovedUserIds.Distinct())
        {
            var actor = step.Actors.FirstOrDefault(a => a.ActorUserId == userId)
                ?? throw new KeyNotFoundException($"User with Id {userId} isn't an approver of the submission of Sheet with Id {sheetId}.");
            removedActorIds.Add(actor.Id);
        }

        await _specSheetDataAccessService.ReassignWorkflowStepActorsAsync(new ReassignWorkflowStepActorsRequest
        {
            SpecSheetId = step.SpecSheetId,
            WorkflowStepId = step.Id,
            CreateWorkflowStepActorRequests = [.. request.AddedUserIds.Distinct()],
            DeleteWorkflowStepActorRequests = removedActorIds
        }, cancellationToken);
    }
#pragma warning restore CS0618

    public async Task ApplyChangesAsync(int sheetId, SheetChanges changes, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(changes, nameof(changes));

        var currentUser = await _userContext.EnsureUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Current user not found in the context.");
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

    private async Task<DbWorkflowStep> FindPendingStepAsync(int sheetId, CancellationToken cancellationToken)
    {
        var specSheetId = await FindDraftSpecSheetIdAsync(sheetId, cancellationToken);
        return await _dbContext.WorkflowSteps
            .AsNoTracking()
            .Include(step => step.Actors)
            .Where(step => step.SpecSheetId == specSheetId && step.State == WorkflowState.Pending)
            .OrderByDescending(step => step.StartedDateTime)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Sheet with Id {sheetId} has no submission awaiting approval.");
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
