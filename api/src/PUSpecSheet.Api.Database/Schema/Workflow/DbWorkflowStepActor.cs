using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Schema.Workflow;

public record DbWorkflowStepActor
{
    public required int Id { get; init; }

    public required int WorkflowStepId { get; init; }

    public required int? ActorUserId { get; init; }

    public required WorkflowState State { get; set; }

    public virtual DbUser? ActorUser { get; set; }

    public required string? Comment { get; set; }

    public required DateTime? ActionedDateTime { get; set; }
}
