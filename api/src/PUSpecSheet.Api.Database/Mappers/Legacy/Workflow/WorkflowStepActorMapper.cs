using PUSpecSheet.Abstractions.Domain.Legacy.Workflow;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Schema.Legacy.Workflow;

namespace PUSpecSheet.Api.Database.Mappers.Legacy.Workflow;

internal static class WorkflowStepActorMapper
{
    public static WorkflowStepActor MapToDomain(DbWorkflowStepActor dbWorkflowStepActor)
    {
        return new WorkflowStepActor
        {
            Id = dbWorkflowStepActor.Id,
            WorkflowStepId = dbWorkflowStepActor.WorkflowStepId,
            State = dbWorkflowStepActor.State,
            ActionedDateTime = dbWorkflowStepActor.ActionedDateTime,
            Comment = dbWorkflowStepActor.Comment,
            Actor = dbWorkflowStepActor.ActorUser?.MapToDomain(),
        };
    }

    public static IEnumerable<WorkflowStepActor> MapToDomain(this ICollection<DbWorkflowStepActor> dbWorkflowStepActors)
    {
        return dbWorkflowStepActors
            .Select(MapToDomain);
    }
}
