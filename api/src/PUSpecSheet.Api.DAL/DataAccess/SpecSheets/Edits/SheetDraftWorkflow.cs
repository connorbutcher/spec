using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Api.Emails.Models;
using PUSpecSheet.Api.Emails.Services;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// Submits a user's checked-out changes to a sheet and decides submissions. A sheet type without approval publishes the
/// changes as the next version straight away; otherwise the chosen approvers must all approve, and the people involved
/// are emailed.
/// </summary>
internal class SheetDraftWorkflow
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly SheetDraftPublisher _publisher;
    private readonly SheetSubmissions _submissions;
    private readonly IEmailBuilderService _emailBuilderService;
    private readonly IEmailService _emailService;
    private readonly ILogger<SheetDraftWorkflow> _logger;

    public SheetDraftWorkflow(PUSpecSheetDbContext dbContext,
        SheetDraftPublisher publisher,
        SheetSubmissions submissions,
        IEmailBuilderService emailBuilderService,
        IEmailService emailService,
        ILogger<SheetDraftWorkflow> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(publisher, nameof(publisher));
        ArgumentNullException.ThrowIfNull(submissions, nameof(submissions));
        ArgumentNullException.ThrowIfNull(emailBuilderService, nameof(emailBuilderService));
        ArgumentNullException.ThrowIfNull(emailService, nameof(emailService));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _dbContext = dbContext;
        _publisher = publisher;
        _submissions = submissions;
        _emailBuilderService = emailBuilderService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Submits the user's checked-out changes: publishes them as the next version when the sheet type doesn't need
    /// approval, otherwise sends them to the approvers.
    /// </summary>
    public async Task SubmitAsync(int sheetId, User submittingUser, string changeLog, IReadOnlyCollection<int> approverUserIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(submittingUser, nameof(submittingUser));
        ArgumentNullException.ThrowIfNull(approverUserIds, nameof(approverUserIds));

        var sheet = await GetSheetAsync(sheetId, cancellationToken);

        if (!sheet.SpecSheetType!.RequiresApprovalWorkflow)
        {
            await using var publishing = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var submission = await _submissions.SubmitAsync(sheetId, submittingUser.Id, changeLog, [], cancellationToken);
            await _submissions.ApproveAsync(submission, cancellationToken);
            var version = await _publisher.PublishAsync(submission, submittingUser.Id, cancellationToken);
            await _submissions.SetVersionAsync(submission, version, cancellationToken);
            await publishing.CommitAsync(cancellationToken);

            _logger.LogInformation("Published UserId {UserId}'s changes to Sheet with Id {SheetId} as version {VersionNumber} without approval", submittingUser.Id, sheetId, version.VersionNumber);
            return;
        }

        if (approverUserIds.Count == 0)
        {
            throw new InvalidOperationException("Choose at least one approver.");
        }

        if (approverUserIds.Contains(submittingUser.Id))
        {
            throw new InvalidOperationException("You can't approve your own changes.");
        }

        var approvers = await _dbContext.Users
            .AsNoTracking()
            .Where(user => approverUserIds.Contains(user.Id))
            .ToListAsync(cancellationToken);
        if (approvers.Count != approverUserIds.Count)
        {
            throw new KeyNotFoundException($"An approver of the changes to Sheet with Id {sheetId} was not found.");
        }

        await using var submitting = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var submitted = await _submissions.SubmitAsync(sheetId, submittingUser.Id, changeLog, approverUserIds, cancellationToken);
        await submitting.CommitAsync(cancellationToken);

        foreach (var approver in approvers)
        {
            await SendSafelyAsync(() => _emailService.SendApprovalEmailAsync(_emailBuilderService.BuildApprovalNotificationEmail(NotificationArgs(sheet, approver.MapToDomain(), changeLog)), cancellationToken), sheetId);
        }

        _logger.LogInformation("Submitted UserId {UserId}'s changes to Sheet with Id {SheetId} for approval as submission {SubmissionId}", submittingUser.Id, sheetId, submitted.Id);
    }

    /// <summary>
    /// Records the user's decision on the submission. A rejection hands its changes back to their author, still checked
    /// out to them; the last approval publishes them as the next version.
    /// </summary>
    public async Task DecideAsync(int sheetId, int submissionId, User decidingUser, ApproverDecision decision, string? comment, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(decidingUser, nameof(decidingUser));

        var sheet = await GetSheetAsync(sheetId, cancellationToken);
        var emails = new List<(EmailArgs Email, bool Rejection)>();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var submission = await _submissions.GetPendingAsync(sheetId, submissionId, cancellationToken);
        await _submissions.RecordDecisionAsync(submission, decidingUser.Id, decision, comment, cancellationToken);

        if (decision == ApproverDecision.Rejected)
        {
            await _submissions.HandBackAsync(submission, SubmissionStatus.Rejected, cancellationToken);

            var submitter = await GetUserAsync(submission.SubmittedByUserId, cancellationToken);
            emails.Add((_emailBuilderService.BuildRejectionNotificationEmail(NotificationArgs(sheet, submitter, comment!) with { RejectedBy = decidingUser.DisplayName }), true));
        }
        else if (submission.Approvers.All(a => a.Decision == ApproverDecision.Approved))
        {
            await _submissions.ApproveAsync(submission, cancellationToken);
            var version = await _publisher.PublishAsync(submission, submission.SubmittedByUserId, cancellationToken);
            await _submissions.SetVersionAsync(submission, version, cancellationToken);

            // Everyone watching the phase, or its parent phase, is told it has been published.
            var phaseId = sheet.SpecSheetPhase!.ParentSpecSheetPhaseId ?? sheet.SpecSheetPhaseId;
            var observers = await _dbContext.SpecSheetPhaseObservers
                .Where(observer => observer.SpecSheetPhaseId == phaseId)
                .Include(observer => observer.User)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync(cancellationToken);
            foreach (var observer in observers)
            {
                emails.Add((_emailBuilderService.BuildPublishedNotificationEmail(NotificationArgs(sheet, observer.User!.MapToDomain(), submission.Note ?? string.Empty)), false));
            }
        }

        await transaction.CommitAsync(cancellationToken);

        foreach (var (email, rejection) in emails)
        {
            if (rejection)
            {
                await SendSafelyAsync(() => _emailService.SendRejectionEmailAsync(email, cancellationToken), sheetId);
            }
            else
            {
                await SendSafelyAsync(() => _emailService.SendPublishedEmailAsync(email, cancellationToken), sheetId);
            }
        }

        _logger.LogInformation("UserId {UserId} decided {Decision} on submission {SubmissionId} of Sheet with Id {SheetId}", decidingUser.Id, decision, submissionId, sheetId);
    }

    /// <summary>
    /// Takes back a submission its author no longer wants approved: its changes go back to being checked out to them.
    /// </summary>
    public async Task WithdrawAsync(int sheetId, int submissionId, User withdrawingUser, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(withdrawingUser, nameof(withdrawingUser));

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var submission = await _submissions.GetPendingAsync(sheetId, submissionId, cancellationToken);
        if (submission.SubmittedByUserId != withdrawingUser.Id)
        {
            throw new InvalidOperationException("Only who submitted these changes can withdraw them.");
        }

        await _submissions.HandBackAsync(submission, SubmissionStatus.Withdrawn, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("UserId {UserId} withdrew submission {SubmissionId} of Sheet with Id {SheetId}", withdrawingUser.Id, submissionId, sheetId);
    }

    /// <summary>
    /// Changes who may approve the submission, and emails those added.
    /// </summary>
    public async Task ReassignAsync(int sheetId, int submissionId, IReadOnlyCollection<int> addedUserIds, IReadOnlyCollection<int> removedUserIds, CancellationToken cancellationToken)
    {
        var sheet = await GetSheetAsync(sheetId, cancellationToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var submission = await _submissions.GetPendingAsync(sheetId, submissionId, cancellationToken);
        if (addedUserIds.Contains(submission.SubmittedByUserId))
        {
            throw new InvalidOperationException("Who submitted the changes can't approve them.");
        }

        var newUserIds = addedUserIds.Where(userId => submission.Approvers.All(a => a.UserId != userId)).ToList();
        await _submissions.ReassignAsync(submission, removedUserIds, newUserIds, cancellationToken);
        if (submission.Approvers.Count == 0)
        {
            throw new InvalidOperationException("A submission needs at least one approver.");
        }

        await transaction.CommitAsync(cancellationToken);

        var added = await _dbContext.Users
            .AsNoTracking()
            .Where(user => newUserIds.Contains(user.Id))
            .ToListAsync(cancellationToken);
        foreach (var user in added)
        {
            await SendSafelyAsync(() => _emailService.SendApprovalEmailAsync(_emailBuilderService.BuildApprovalNotificationEmail(NotificationArgs(sheet, user.MapToDomain(), submission.Note ?? string.Empty)), cancellationToken), sheetId);
        }
    }

    // The emails go after the change is committed, so one that can't be sent is logged rather than failing the request.
    private async Task SendSafelyAsync(Func<Task> send, int sheetId)
    {
        try
        {
            await send();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not send an email about Sheet with Id {SheetId}", sheetId);
        }
    }

    private async Task<DbSheet> GetSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _dbContext.Sheets
            .AsNoTracking()
            .Include(s => s.SpecSheetPhase)
            .Include(s => s.SpecSheetType)
            .FirstOrDefaultAsync(s => s.Id == sheetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sheet with Id {sheetId} not found.");
    }

    private async Task<User> GetUserAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with Id {userId} not found.");
        return user.MapToDomain();
    }

    private static SpecSheetEmailArgs NotificationArgs(DbSheet sheet, User user, string content)
    {
        return new SpecSheetEmailArgs
        {
            SpecSheetId = sheet.Id,
            SpecSheetPhaseId = sheet.SpecSheetPhaseId,
            SpecSheetTypeId = sheet.SpecSheetTypeId,
            TypeName = sheet.SpecSheetType!.Type,
            PhaseName = sheet.SpecSheetPhase!.Phase,
            Content = content,
            User = user
        };
    }
}
