using PUSpecSheet.Abstractions.Approvals;

namespace PUSpecSheet.Migrator.Legacy;

// What the State columns of WorkflowSteps and WorkflowStepActors mean. If the legacy app numbered them differently,
// change them here and nothing else. A state not listed stops the sheet with its value, rather than guessing.
internal static class LegacyWorkflowStates
{
    private static readonly Dictionary<int, SubmissionStatus> _steps = new()
    {
        [0] = SubmissionStatus.Pending,
        [1] = SubmissionStatus.Approved,
        [2] = SubmissionStatus.Rejected,
    };

    private static readonly Dictionary<int, ApprovalDecision> _actors = new()
    {
        [0] = ApprovalDecision.Pending,
        [1] = ApprovalDecision.Approved,
        [2] = ApprovalDecision.Rejected,
    };

    public static SubmissionStatus Step(LegacyWorkflowStep step)
    {
        return _steps.TryGetValue(step.State, out var status)
            ? status
            : throw new MigrationException($"Workflow step {step.Id} has state {step.State}, which LegacyWorkflowStates doesn't know.");
    }

    public static ApprovalDecision Actor(LegacyWorkflowActor actor)
    {
        return _actors.TryGetValue(actor.State, out var decision)
            ? decision
            : throw new MigrationException($"Workflow step actor {actor.Id} has state {actor.State}, which LegacyWorkflowStates doesn't know.");
    }
}
