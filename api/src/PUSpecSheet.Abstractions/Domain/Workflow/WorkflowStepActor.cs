using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Abstractions.Domain.Workflow;

public record WorkflowStepActor
{
    public required int Id { get; init; }

    public required int WorkflowStepId { get; init; }

    public required WorkflowState State { get; init; }

    public required string? Comment { get; init; }

    public required DateTime? ActionedDateTime { get; init; }

    public required User? Actor { get; init; }
}
