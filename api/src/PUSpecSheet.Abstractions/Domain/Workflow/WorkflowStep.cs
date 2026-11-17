using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Abstractions.Domain.Workflow;

public record WorkflowStep
{
    public required int Id { get; init; }

    public required int SpecSheetId { get; init; }

    public required DateTime StartedDateTime { get; init; }

    public required WorkflowState State { get; init; }

    public required WorkflowStepActor[] Actors { get; init; } = [];
}
