using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// What the State columns of WorkflowSteps and WorkflowStepActors mean, as the app's <see cref="WorkflowState"/>
/// numbers them. A state not listed stops the sheet with its value, rather than guessing.
/// </summary>
internal static class LegacyWorkflowStates
{
    private static readonly Dictionary<int, SubmissionStatus> _steps = new()
    {
        [(int)WorkflowState.Pending] = SubmissionStatus.Pending,
        [(int)WorkflowState.Approved] = SubmissionStatus.Approved,
        [(int)WorkflowState.Rejected] = SubmissionStatus.Rejected
    };

    private static readonly Dictionary<int, ApproverDecision> _actors = new()
    {
        [(int)WorkflowState.Pending] = ApproverDecision.Pending,
        [(int)WorkflowState.Approved] = ApproverDecision.Approved,
        [(int)WorkflowState.Rejected] = ApproverDecision.Rejected
    };

    public static SubmissionStatus Step(LegacyWorkflowStep step)
    {
        return _steps.TryGetValue(step.State, out var status)
            ? status
            : throw new MigrationException($"Workflow step {step.Id} has state {step.State}, which LegacyWorkflowStates doesn't know.");
    }

    public static ApproverDecision Actor(LegacyWorkflowActor actor)
    {
        return _actors.TryGetValue(actor.State, out var decision)
            ? decision
            : throw new MigrationException($"Workflow step actor {actor.Id} has state {actor.State}, which LegacyWorkflowStates doesn't know.");
    }
}
