namespace PUSpecSheet.Abstractions.Requests.Workflow;

public record ReassignWorkflowStepActorsRequest
{
    public required int SpecSheetId { get; init; }

    public required int WorkflowStepId { get; init; }

    public required IEnumerable<int> CreateWorkflowStepActorRequests { get; init; }

    public required IEnumerable<int> DeleteWorkflowStepActorRequests { get; init; }
}
