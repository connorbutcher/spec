using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Phases;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Phases;

public sealed class PhaseService(PuSpecSheetDbContext db) : IPhaseService
{
    public async Task<IReadOnlyList<PhaseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var phases = await db.Phases
            .AsNoTracking()
            .Include(phase => phase.SheetTypes)
            .OrderBy(phase => phase.DisplayOrder)
            .ThenBy(phase => phase.Code)
            .ToListAsync(cancellationToken);

        return phases.Select(phase => phase.ToDto()).ToList();
    }

    public async Task<PhaseDto> GetAsync(int id, CancellationToken cancellationToken)
    {
        var phase = await db.Phases
            .AsNoTracking()
            .Include(candidate => candidate.SheetTypes)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (phase is null)
        {
            throw new NotFoundException($"Phase {id} was not found.");
        }

        return phase.ToDto();
    }
}
