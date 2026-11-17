using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Abstractions.Requests.Workflow;

public record UpdateWorkflowStepActorRequest
{
    public required int WorkflowStepActorId { get; init; }

    public required int WorkflowStepId { get; init; }

    public required WorkflowState State { get; init; }

    public required string? Comment { get; init; }
}
