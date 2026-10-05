using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Phases;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Phases;

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

    public async Task<PhaseDto> CreateAsync(CreatePhaseRequest request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();
        if (await db.Phases.AnyAsync(phase => phase.Code == code, cancellationToken))
        {
            throw new ConflictException($"A phase with the code \"{code}\" already exists.");
        }

        var parentId = request.ParentPhaseId;
        if (parentId is not null && !await db.Phases.AnyAsync(phase => phase.Id == parentId, cancellationToken))
        {
            throw new NotFoundException($"Parent phase {parentId} was not found.");
        }

        var sheetTypeIds = await ExistingSheetTypeIdsAsync(request.SheetTypeIds ?? [], cancellationToken);

        var lastOrder = await db.Phases
            .Where(phase => phase.ParentPhaseId == parentId)
            .MaxAsync(phase => (int?)phase.DisplayOrder, cancellationToken);

        var phase = new Phase
        {
            Code = code,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            ParentPhaseId = parentId,
            DisplayOrder = (lastOrder ?? 0) + 1,
        };

        foreach (var sheetTypeId in sheetTypeIds)
        {
            phase.SheetTypes.Add(new PhaseSheetType { SheetTypeId = sheetTypeId });
        }

        db.Phases.Add(phase);
        await db.SaveChangesAsync(cancellationToken);

        return phase.ToDto();
    }

    public async Task<PhaseDto> SetSheetTypesAsync(int id, SetPhaseSheetTypesRequest request, CancellationToken cancellationToken)
    {
        var phase = await db.Phases
            .Include(candidate => candidate.SheetTypes)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (phase is null)
        {
            throw new NotFoundException($"Phase {id} was not found.");
        }

        var wanted = await ExistingSheetTypeIdsAsync(request.SheetTypeIds, cancellationToken);
        var removed = phase.SheetTypes.Where(link => !wanted.Contains(link.SheetTypeId)).ToList();
        await EnsureSheetsAreEmptyAsync(phase, removed, cancellationToken);

        foreach (var link in removed)
        {
            phase.SheetTypes.Remove(link);
        }

        var current = phase.SheetTypes.Select(link => link.SheetTypeId).ToHashSet();
        foreach (var sheetTypeId in wanted.Where(sheetTypeId => !current.Contains(sheetTypeId)))
        {
            phase.SheetTypes.Add(new PhaseSheetType { SheetTypeId = sheetTypeId });
        }

        await db.SaveChangesAsync(cancellationToken);

        return phase.ToDto();
    }

    /// <summary>The distinct ids asked for, once every one is known to be a sheet type.</summary>
    private async Task<HashSet<int>> ExistingSheetTypeIdsAsync(IReadOnlyList<int> sheetTypeIds, CancellationToken cancellationToken)
    {
        var wanted = sheetTypeIds.ToHashSet();
        var existing = await db.SheetTypes
            .Where(sheetType => wanted.Contains(sheetType.Id))
            .Select(sheetType => sheetType.Id)
            .ToListAsync(cancellationToken);

        var missing = wanted.Except(existing).Order().ToList();
        if (missing.Count > 0)
        {
            throw new NotFoundException($"Sheet type {missing[0]} was not found.");
        }

        return wanted;
    }

    /// <summary>A sheet type can't be taken away from a phase once its sheet there has tables: they would be stranded.</summary>
    private async Task EnsureSheetsAreEmptyAsync(Phase phase, List<PhaseSheetType> removed, CancellationToken cancellationToken)
    {
        if (removed.Count == 0)
        {
            return;
        }

        var removedIds = removed.Select(link => link.SheetTypeId).ToList();
        var inUse = await db.Sheets
            .Where(sheet => sheet.PhaseId == phase.Id && removedIds.Contains(sheet.SheetTypeId) && sheet.Tables.Any())
            .OrderBy(sheet => sheet.SheetType.DisplayOrder)
            .Select(sheet => sheet.SheetType.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (inUse is not null)
        {
            throw new ConflictException(
                $"The {inUse} sheet of {phase.Code} already has tables, so {inUse} can't be removed from this phase.");
        }
    }
}
