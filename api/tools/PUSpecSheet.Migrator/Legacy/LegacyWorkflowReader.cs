using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// Reads the workflow steps and their actors of a sheet's copies, by copy.
/// </summary>
internal sealed class LegacyWorkflowReader
{
    private readonly PUSpecSheetDbContext _dbContext;

    public LegacyWorkflowReader(PUSpecSheetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
    }

    public async Task<ILookup<int, LegacyWorkflowStep>> ReadAsync(
        IQueryable<DbSpecSheet> copies,
        CancellationToken cancellationToken)
    {
        var steps = _dbContext.WorkflowSteps.AsNoTracking()
            .Where(step => copies.Any(copy => copy.Id == step.SpecSheetId));

        var actors = (await _dbContext.WorkflowStepActors.AsNoTracking()
                .Where(actor => steps.Any(step => step.Id == actor.WorkflowStepId))
                .OrderBy(actor => actor.Id)
                .Select(actor => new { actor.Id, actor.WorkflowStepId, actor.ActorUserId, actor.Comment, actor.State, actor.ActionedDateTime })
                .ToListAsync(cancellationToken))
            .ToLookup(
                actor => actor.WorkflowStepId,
                actor => new LegacyWorkflowActor
                {
                    Id = actor.Id,
                    UserId = actor.ActorUserId,
                    Comment = actor.Comment,
                    State = (int)actor.State,
                    ActionedDateTime = actor.ActionedDateTime
                });

        return (await steps
                .OrderBy(step => step.StartedDateTime)
                .ThenBy(step => step.Id)
                .Select(step => new { step.Id, step.SpecSheetId, step.StartedDateTime, step.State })
                .ToListAsync(cancellationToken))
            .ToLookup(
                step => step.SpecSheetId,
                step => new LegacyWorkflowStep
                {
                    Id = step.Id,
                    StartedDateTime = step.StartedDateTime,
                    State = (int)step.State,
                    Actors = actors[step.Id].ToList()
                });
    }
}
