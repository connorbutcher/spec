using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Migrator.Legacy;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// Turns the workflow steps of a sheet's copies into the submissions that go with its revisions.
/// </summary>
internal sealed class SubmissionPlanner
{
    private readonly DbSheet _sheet;
    private readonly List<string> _warnings;

    public SubmissionPlanner(DbSheet sheet, List<string> warnings)
    {
        _sheet = sheet;
        _warnings = warnings;
    }

    public List<PlannedSubmission> Planned { get; } = [];

    // A copy's workflow steps become submissions. The step that approved a published copy is the submission that
    // published its version, and the revisions written for that version are its changes. A pending step on the working
    // copy is a submission awaiting approval, holding the drafts. Every other step is kept as decided.
    public void Plan(LegacyCopy copy, DbSheetVersion? version, DateTime at, IReadOnlyList<IDbSheetRevision> revisionsWritten)
    {
        var steps = copy.Workflow.OrderBy(step => step.StartedDateTime).ThenBy(step => step.Id).ToList();
        var publishing = version is null ? null : steps.LastOrDefault(step => LegacyWorkflowStates.Step(step) == SubmissionStatus.Approved);
        var awaiting = version is null ? steps.LastOrDefault() : null;
        if (awaiting is not null && LegacyWorkflowStates.Step(awaiting) != SubmissionStatus.Pending)
        {
            awaiting = null;
        }

        for (var index = 0; index < steps.Count; index++)
        {
            var step = steps[index];
            var status = LegacyWorkflowStates.Step(step);
            var next = index + 1 < steps.Count ? steps[index + 1].StartedDateTime : (DateTime?)null;
            DateTime? decidedDateTime;
            if (step == publishing)
            {
                decidedDateTime = LatestAction(step) ?? at;
            }
            else if (step == awaiting)
            {
                decidedDateTime = null;
            }
            else if (status == SubmissionStatus.Pending)
            {
                // Overtaken by a later step or by publishing, so withdrawn when that happened.
                status = SubmissionStatus.Withdrawn;
                decidedDateTime = next ?? at;
                _warnings.Add($"Workflow step {step.Id} (copy {copy.Id}) was never decided; it is kept as withdrawn.");
            }
            else
            {
                if (status == SubmissionStatus.Approved)
                {
                    _warnings.Add($"Workflow step {step.Id} (copy {copy.Id}) approved a copy that a later step decided again, or that wasn't published; it is kept as approved without a version.");
                }

                decidedDateTime = LatestAction(step) ?? next ?? at;
            }

            var submission = new DbSheetSubmission
            {
                Id = 0,
                SheetId = _sheet.Id,
                Note = null,
                SubmittedByUserId = copy.CreatedByUserId,
                SubmittedDateTime = step.StartedDateTime,
                Status = status,
                DecidedDateTime = decidedDateTime,
                SheetVersionId = null
            };
            AddApprovers(submission, step, copy);

            Planned.Add(new PlannedSubmission
            {
                Submission = submission,
                Version = step == publishing ? version : null,
                Revisions = step == publishing || step == awaiting ? [.. revisionsWritten] : []
            });
        }
    }

    private void AddApprovers(DbSheetSubmission submission, LegacyWorkflowStep step, LegacyCopy copy)
    {
        foreach (var actors in step.Actors.Where(actor => actor.UserId is not null).GroupBy(actor => actor.UserId!.Value))
        {
            var ordered = actors.OrderBy(candidate => candidate.Id).ToList();
            if (ordered.Count > 1)
            {
                _warnings.Add($"User {actors.Key} is an actor of workflow step {step.Id} (copy {copy.Id}) more than once; the last is kept.");
            }

            var actor = ordered[^1];
            var decision = LegacyWorkflowStates.Actor(actor);
            var reason = string.IsNullOrWhiteSpace(actor.Comment) ? null : actor.Comment.Trim();
            if (decision == ApproverDecision.Rejected && reason is null)
            {
                reason = DbSheetSubmissionApprover.NoReasonGiven;
            }

            submission.Approvers.Add(new DbSheetSubmissionApprover
            {
                SheetSubmissionId = 0,
                UserId = actors.Key,
                Decision = decision,
                DecidedDateTime = decision == ApproverDecision.Pending ? null : actor.ActionedDateTime ?? submission.DecidedDateTime ?? step.StartedDateTime,
                Reason = reason
            });
        }
    }

    private static DateTime? LatestAction(LegacyWorkflowStep step)
    {
        return step.Actors.Max(actor => actor.ActionedDateTime);
    }
}
