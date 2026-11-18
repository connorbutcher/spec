using PUSpecSheet.Abstractions.Domain.Legacy.Workflow;
using PUSpecSheet.Api.Database.Schema.Legacy.Workflow;

namespace PUSpecSheet.Api.Database.Mappers.Legacy.Workflow;

internal static class WorkflowStepMapper
{
    public static WorkflowStep MapToDomain(DbWorkflowStep dbWorkflowStep)
    {
        return new WorkflowStep
        {
            Id = dbWorkflowStep.Id,
            SpecSheetId = dbWorkflowStep.SpecSheetId,
            StartedDateTime = dbWorkflowStep.StartedDateTime,
            State = dbWorkflowStep.State,
            Actors = [.. dbWorkflowStep.Actors.MapToDomain()],
        };
    }

    public static IEnumerable<WorkflowStep> MapToDomain(this ICollection<DbWorkflowStep> dbWorkflowSteps)
    {
        return dbWorkflowSteps
            .Select(MapToDomain);
    }
}
