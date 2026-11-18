using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.Sheets;
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
    private readonly IUserContext _userContext;
    private readonly SheetDraftCreator _draftCreator;
    private readonly SheetDraftDeleter _draftDeleter;
    private readonly SheetDraftWorkflow _workflow;
    private readonly ISpecSheetCache _specSheetCache;
    private readonly ILogger<ISheetDataAccessService> _logger;

    public SheetDataAccessService(PUSpecSheetDbContext dbContext,
        SheetReader sheetReader,
        IUserContext userContext,
        SheetDraftCreator draftCreator,
        SheetDraftDeleter draftDeleter,
        SheetDraftWorkflow workflow,
        ISpecSheetCache specSheetCache,
        ILogger<ISheetDataAccessService> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(sheetReader, nameof(sheetReader));
        ArgumentNullException.ThrowIfNull(userContext, nameof(userContext));
        ArgumentNullException.ThrowIfNull(draftCreator, nameof(draftCreator));
        ArgumentNullException.ThrowIfNull(draftDeleter, nameof(draftDeleter));
        ArgumentNullException.ThrowIfNull(workflow, nameof(workflow));
        ArgumentNullException.ThrowIfNull(specSheetCache, nameof(specSheetCache));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _dbContext = dbContext;
        _sheetReader = sheetReader;
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

    public async Task<SheetCells?> GetLatestPublishedCellsByPhaseAndTypeAsync(int phaseId, int sheetTypeId, IReadOnlyCollection<Guid> referenceIds, DateTime? publishedBy, CancellationToken cancellationToken)
    {
        if (referenceIds.Count == 0)
        {
            return null;
        }

        var sheetId = await _sheetReader.FindSheetIdAsync(phaseId, sheetTypeId, cancellationToken);
        return sheetId is null ? null : await _sheetReader.ReadCellsAsync(sheetId.Value, referenceIds, publishedBy, cancellationToken);
    }

    public async Task<SheetCells?> GetPublishedCellsAsync(int sheetId, int? versionNumber, IReadOnlyCollection<Guid> referenceIds, CancellationToken cancellationToken)
    {
        if (referenceIds.Count == 0)
        {
            return null;
        }

        return await _sheetReader.ReadPublishedCellsAsync(sheetId, versionNumber, referenceIds, cancellationToken);
    }

    public async Task<IReadOnlyList<SheetTable>> GetUniqueTablesAsync(CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var tables = await _sheetReader.ReadUniqueTablesAsync(cancellationToken);

        var permitted = await _dbContext.SpecSheetTablePermissions
            .AsNoTracking()
            .Where(permission => permission.UserId == currentUser.Id)
            .Select(permission => permission.TableReferenceId)
            .ToListAsync(cancellationToken);

        return [.. tables.Where(table => permitted.Contains(table.ReferenceId))];
    }

    public async Task<IReadOnlyList<SheetVersionSummary>> GetPublishedVersionSummariesAsync(int sheetTypeId, int? phaseId, CancellationToken cancellationToken)
    {
        return await _sheetReader.ReadVersionSummariesAsync(sheet => sheet.SpecSheetTypeId == sheetTypeId && (phaseId == null || sheet.SpecSheetPhaseId == phaseId), cancellationToken);
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

    public async Task<IReadOnlyList<SheetListItem>> GetSheetListAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        var versions = await GetPublishedVersionSummariesAsync(sheetTypeId, phaseId, cancellationToken);
        var items = versions.Select(MapToListItem).ToList();

        var sheet = await GetSheetSummaryByPhaseAndTypeAsync(phaseId, sheetTypeId, cancellationToken);
        if (sheet?.DraftSpecSheetId is not null)
        {
            items.Add(new SheetListItem
            {
                SheetId = sheet.Id,
                SpecSheetPhaseId = sheet.SpecSheetPhaseId,
                SpecSheetTypeId = sheet.SpecSheetTypeId,
                SpecSheetPhase = sheet.SpecSheetPhase,
                SpecSheetType = sheet.SpecSheetType,
                VersionNumber = (sheet.LatestVersion?.VersionNumber ?? 0) + 1,
                IsDraft = true,
                Version = null,
                Properties = sheet.Properties
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<SheetListItem>> GetPublishedSheetListAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        var versions = await GetPublishedVersionSummariesAsync(sheetTypeId, null, cancellationToken);
        return [.. versions.Select(MapToListItem)];
    }

    public async Task<SheetDetail?> GetPublishedSheetDetailAsync(int sheetId, int? versionNumber, CancellationToken cancellationToken)
    {
        var sheet = await GetPublishedSheetAsync(sheetId, versionNumber, cancellationToken);
        if (sheet is null)
        {
            return null;
        }

        var submissions = await _sheetReader.ReadSubmissionsAsync(sheetId, sheet.Version?.VersionNumber, cancellationToken);
        return new SheetDetail { Sheet = sheet, Submissions = [.. submissions ?? []] };
    }

    public async Task<SheetDetail?> GetDraftSheetDetailAsync(int sheetId, CancellationToken cancellationToken)
    {
        var sheet = await GetDraftSheetAsync(sheetId, cancellationToken);
        if (sheet is null)
        {
            return null;
        }

        var submissions = await _sheetReader.ReadSubmissionsAsync(sheetId, null, cancellationToken);
        return new SheetDetail { Sheet = sheet, Submissions = [.. submissions ?? []] };
    }

    public async Task<SheetLocation?> FindSheetAsync(int specSheetId, CancellationToken cancellationToken)
    {
        return await _sheetReader.FindLocationAsync(specSheetId, cancellationToken);
    }

    #region Writes

    public async Task<SheetLocation> CreateDraftAsync(CreateSheetRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var currentUser = await GetCurrentUserAsync(cancellationToken);

        var specSheetId = await _draftCreator.CreateAsync(request, currentUser, cancellationToken);

        var sheetId = await _sheetReader.FindSheetIdAsync(request.SpecSheetPhaseId, request.SpecSheetTypeId, cancellationToken)
            ?? throw new InvalidOperationException($"No sheet found for PhaseId {request.SpecSheetPhaseId} and TypeId {request.SpecSheetTypeId} after creating its draft.");

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

    // The unpublished SpecSheets row that is the sheet's draft: what the spec sheet methods act on.
    private async Task<int> FindDraftSpecSheetIdAsync(int sheetId, CancellationToken cancellationToken)
    {
        var sheet = await _dbContext.Sheets
            .AsNoTracking()
            .Where(s => s.Id == sheetId)
            .Select(s => new { s.SpecSheetTypeId, s.SpecSheetPhaseId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Sheet with Id {sheetId} not found.");

        return await _dbContext.SpecSheets
            .AsNoTracking()
            .Where(s => !s.IsPublished && s.SpecSheetTypeId == sheet.SpecSheetTypeId && s.SpecSheetPhaseId == sheet.SpecSheetPhaseId)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Sheet with Id {sheetId} has no draft.");
    }

    #endregion Writes

    private static SheetListItem MapToListItem(SheetVersionSummary summary)
    {
        return new SheetListItem
        {
            SheetId = summary.SheetId,
            SpecSheetPhaseId = summary.SpecSheetPhaseId,
            SpecSheetTypeId = summary.SpecSheetTypeId,
            SpecSheetPhase = summary.SpecSheetPhase,
            SpecSheetType = summary.SpecSheetType,
            VersionNumber = summary.Version.VersionNumber,
            IsDraft = false,
            Version = summary.Version,
            Properties = summary.Properties
        };
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        return await _userContext.EnsureUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Current user not found in the context.");
    }

    // A table that has permissions is only shown to the users it has them for.
    private async Task<Sheet> TrimTablesAsync(Sheet sheet, CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);

        var tableReferenceIds = sheet.Tables.Select(table => table.ReferenceId).ToList();
        var permittedUserIds = (await _dbContext.SpecSheetTablePermissions
            .AsNoTracking()
            .Where(permission => tableReferenceIds.Contains(permission.TableReferenceId))
            .Select(permission => new { permission.TableReferenceId, permission.UserId })
            .ToListAsync(cancellationToken))
            .ToLookup(permission => permission.TableReferenceId, permission => permission.UserId);

        var tables = sheet.Tables
            .Where(table => !permittedUserIds.Contains(table.ReferenceId) || permittedUserIds[table.ReferenceId].Contains(currentUser.Id))
            .ToList();

        return sheet with { Tables = tables };
    }
}
