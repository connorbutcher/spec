using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.DAL.Caching;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Legacy.Workflow;
using PUSpecSheet.Api.Emails.Models;
using PUSpecSheet.Api.Emails.Services;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// Publishes, submits for approval and decides a sheet's drafts. The draft's
/// SpecSheets row still holds its workflow steps and who published it, which the lists and screens read, so the row,
/// the sheet's submission and its drafts are kept in step here, and the people involved are emailed.
/// </summary>
internal class SheetDraftWorkflow
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly SheetDraftPublisher _publisher;
    private readonly SheetSubmissions _submissions;
    private readonly IEmailBuilderService _emailBuilderService;
    private readonly IEmailService _emailService;
    private readonly ISpecSheetCache _specSheetCache;
    private readonly ILogger<SheetDraftWorkflow> _logger;

    public SheetDraftWorkflow(PUSpecSheetDbContext dbContext,
        SheetDraftPublisher publisher,
        SheetSubmissions submissions,
        IEmailBuilderService emailBuilderService,
        IEmailService emailService,
        ISpecSheetCache specSheetCache,
        ILogger<SheetDraftWorkflow> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(publisher, nameof(publisher));
        ArgumentNullException.ThrowIfNull(submissions, nameof(submissions));
        ArgumentNullException.ThrowIfNull(emailBuilderService, nameof(emailBuilderService));
        ArgumentNullException.ThrowIfNull(emailService, nameof(emailService));
        ArgumentNullException.ThrowIfNull(specSheetCache, nameof(specSheetCache));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _dbContext = dbContext;
        _publisher = publisher;
        _submissions = submissions;
        _emailBuilderService = emailBuilderService;
        _emailService = emailService;
        _specSheetCache = specSheetCache;
        _logger = logger;
    }

    /// <summary>
    /// Publishes the draft as the next version, or, when its sheet type needs approval, submits it to the approvers.
    /// </summary>
    public async Task PublishAsync(int sheetId, int specSheetId, User publishingUser, string changeLog, IReadOnlyCollection<int> approverUserIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(publishingUser, nameof(publishingUser));

        var header = await GetHeaderAsync(specSheetId, true, cancellationToken);
        header.PublishedByUserId = publishingUser.Id;
        header.ChangeLog = changeLog;

        if (!header.SpecSheetType!.RequiresApprovalWorkflow)
        {
            await using var publishing = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await _publisher.PublishAsync(sheetId, header, publishingUser.Id, changeLog, cancellationToken);
            await publishing.CommitAsync(cancellationToken);

            await _specSheetCache.RemoveAsync(specSheetId, cancellationToken);
            _logger.LogInformation("Published SpecSheet with Id {SpecSheetId} by UserId {UserId} without workflow as the SpecSheetType does not require approval", specSheetId, publishingUser.Id);
            return;
        }

        var approvers = await _dbContext.Users
            .AsNoTracking()
            .Where(user => approverUserIds.Contains(user.Id))
            .ToListAsync(cancellationToken);
        if (approvers.Count != approverUserIds.Count)
        {
            throw new KeyNotFoundException($"An approver of Spec Sheet with Id {specSheetId} was not found.");
        }

        // The workflow step is saved with the submission, so that neither is kept without the other.
        header.WorkflowSteps.Add(new DbWorkflowStep
        {
            Id = 0,
            SpecSheetId = specSheetId,
            State = WorkflowState.Pending,
            StartedDateTime = DateTime.UtcNow,
            Actors = [.. approverUserIds.Select(userId => NewActor(userId))]
        });

        await using var submitting = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _submissions.SubmitAsync(sheetId, publishingUser.Id, changeLog, approverUserIds, cancellationToken);
        await submitting.CommitAsync(cancellationToken);

        foreach (var approver in approvers)
        {
            await _emailService.SendApprovalEmailAsync(_emailBuilderService.BuildApprovalNotificationEmail(NotificationArgs(header, approver.MapToDomain(), changeLog)), cancellationToken);
        }

        await _specSheetCache.RemoveAsync(specSheetId, cancellationToken);
        _logger.LogInformation("Submitted SpecSheet with Id {SpecSheetId} for approval by UserId {UserId}", specSheetId, publishingUser.Id);
    }

    /// <summary>
    /// Records the user's decision on the pending submission. Any rejection hands the draft back to be edited; the last
    /// approval publishes it.
    /// </summary>
    public async Task DecideAsync(int sheetId, int specSheetId, User decidingUser, ApproverDecision decision, string? comment, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(decidingUser, nameof(decidingUser));

        var state = decision switch
        {
            ApproverDecision.Approved => WorkflowState.Approved,
            ApproverDecision.Rejected => WorkflowState.Rejected,
            _ => WorkflowState.Pending
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var header = await GetHeaderAsync(specSheetId, true, cancellationToken);
        var step = await GetPendingStepAsync(specSheetId, cancellationToken);
        var actor = step.Actors.FirstOrDefault(a => a.ActorUserId == decidingUser.Id)
            ?? throw new KeyNotFoundException($"User with Id {decidingUser.Id} isn't an approver of the submission of Spec Sheet with Id {specSheetId}.");

        actor.State = state;
        actor.Comment = comment;
        actor.ActionedDateTime = DateTime.UtcNow;
        await _submissions.RecordDecisionAsync(sheetId, decidingUser.Id, state, comment, cancellationToken);

        var emails = new List<(EmailArgs Email, bool Rejection)>();

        if (state == WorkflowState.Rejected)
        {
            var publishedByUser = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == header.PublishedByUserId, cancellationToken)
                ?? throw new KeyNotFoundException($"PublishedByUser with Id {header.PublishedByUserId} not found.");

            header.PublishedByUserId = null;
            header.PublishedDateTime = null;
            header.IsPublished = false;
            step.State = WorkflowState.Rejected;
            await _submissions.RejectAsync(sheetId, cancellationToken);

            emails.Add((_emailBuilderService.BuildRejectionNotificationEmail(NotificationArgs(header, publishedByUser.MapToDomain(), comment!) with { RejectedBy = decidingUser.DisplayName }), true));
        }
        else if (step.Actors.All(a => a.State == WorkflowState.Approved))
        {
            step.State = WorkflowState.Approved;

            var submission = await _submissions.ApproveAsync(sheetId, cancellationToken);
            var version = await _publisher.PublishAsync(sheetId, header, header.PublishedByUserId ?? decidingUser.Id, header.ChangeLog, cancellationToken);
            await _submissions.SetVersionAsync(submission, version, cancellationToken);

            // Everyone watching the phase, or its parent phase, is told it has been published.
            var phaseId = header.SpecSheetPhase!.ParentSpecSheetPhaseId ?? header.SpecSheetPhaseId;
            var observers = await _dbContext.SpecSheetPhaseObservers
                .Where(observer => observer.SpecSheetPhaseId == phaseId)
                .Include(observer => observer.User)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync(cancellationToken);
            foreach (var observer in observers)
            {
                emails.Add((_emailBuilderService.BuildPublishedNotificationEmail(NotificationArgs(header, observer.User!.MapToDomain(), header.ChangeLog!)), false));
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        foreach (var (email, rejection) in emails)
        {
            if (rejection)
            {
                await _emailService.SendRejectionEmailAsync(email, cancellationToken);
            }
            else
            {
                await _emailService.SendPublishedEmailAsync(email, cancellationToken);
            }
        }

        await _specSheetCache.RemoveAsync(specSheetId, cancellationToken);
        _logger.LogInformation("UserId {UserId} decided {Decision} on the submission of SpecSheet with Id {SpecSheetId}", decidingUser.Id, decision, specSheetId);
    }

    /// <summary>
    /// Changes who may approve the pending submission, and emails those added.
    /// </summary>
    public async Task ReassignAsync(int sheetId, int specSheetId, IReadOnlyCollection<int> addedUserIds, IReadOnlyCollection<int> removedUserIds, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var header = await GetHeaderAsync(specSheetId, false, cancellationToken);
        var step = await GetPendingStepAsync(specSheetId, cancellationToken);

        foreach (var userId in removedUserIds)
        {
            var actor = step.Actors.FirstOrDefault(a => a.ActorUserId == userId)
                ?? throw new KeyNotFoundException($"User with Id {userId} isn't an approver of the submission of Spec Sheet with Id {specSheetId}.");
            step.Actors.Remove(actor);
            _dbContext.WorkflowStepActors.Remove(actor);
        }

        var newUserIds = addedUserIds.Where(userId => step.Actors.All(a => a.ActorUserId != userId)).ToList();
        foreach (var userId in newUserIds)
        {
            step.Actors.Add(NewActor(userId, step.Id));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _submissions.ReassignAsync(sheetId, removedUserIds, newUserIds, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var added = await _dbContext.Users
            .AsNoTracking()
            .Where(user => newUserIds.Contains(user.Id))
            .ToListAsync(cancellationToken);
        foreach (var user in added)
        {
            await _emailService.SendApprovalEmailAsync(_emailBuilderService.BuildApprovalNotificationEmail(NotificationArgs(header, user.MapToDomain(), header.ChangeLog!)), cancellationToken);
        }

        await _specSheetCache.RemoveAsync(specSheetId, cancellationToken);
    }

    private static DbWorkflowStepActor NewActor(int userId, int workflowStepId = 0)
    {
        return new DbWorkflowStepActor
        {
            Id = 0,
            WorkflowStepId = workflowStepId,
            ActorUserId = userId,
            State = WorkflowState.Pending,
            ActionedDateTime = null,
            Comment = null
        };
    }

    private async Task<DbSpecSheet> GetHeaderAsync(int specSheetId, bool tracked, CancellationToken cancellationToken)
    {
        var query = _dbContext.SpecSheets
            .Include(s => s.SpecSheetPhase)
            .Include(s => s.SpecSheetType)
            .AsQueryable();
        if (!tracked)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(s => s.Id == specSheetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Spec Sheet with Id {specSheetId} not found.");
    }

    private async Task<DbWorkflowStep> GetPendingStepAsync(int specSheetId, CancellationToken cancellationToken)
    {
        return await _dbContext.WorkflowSteps
            .Include(step => step.Actors)
            .Where(step => step.SpecSheetId == specSheetId && step.State == WorkflowState.Pending)
            .OrderByDescending(step => step.StartedDateTime)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Spec Sheet with Id {specSheetId} has no submission awaiting approval.");
    }

    private static SpecSheetEmailArgs NotificationArgs(DbSpecSheet header, User user, string content)
    {
        return new SpecSheetEmailArgs
        {
            SpecSheetId = header.Id,
            SpecSheetPhaseId = header.SpecSheetPhaseId,
            SpecSheetTypeId = header.SpecSheetTypeId,
            TypeName = header.SpecSheetType!.Type,
            PhaseName = header.SpecSheetPhase!.Phase,
            Content = content,
            User = user
        };
    }
}
