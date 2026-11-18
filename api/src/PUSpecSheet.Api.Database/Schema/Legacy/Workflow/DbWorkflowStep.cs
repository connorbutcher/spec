using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Api.Database.Schema.Legacy.Workflow;

public record DbWorkflowStep
{
    public required int Id { get; init; }

    public required int SpecSheetId { get; init; }

    public required DateTime StartedDateTime { get; init; }

    public required WorkflowState State { get; set; }

    public required virtual ICollection<DbWorkflowStepActor> Actors { get; init; } = [];
}
