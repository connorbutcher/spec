using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Phases;
using PUSpecSheet.Application.Persistence.SheetTypes;
using PUSpecSheet.Contracts.Phases;
using PUSpecSheet.Database.Entities.Phases;

namespace PUSpecSheet.Application.Phases;

public sealed class PhaseService : IPhaseService
{
    private readonly IPhaseRepository phases;
    private readonly ISheetTypeRepository sheetTypes;
    private readonly IUnitOfWork unitOfWork;

    public PhaseService(IPhaseRepository phases, ISheetTypeRepository sheetTypes, IUnitOfWork unitOfWork)
    {
        this.phases = phases;
        this.sheetTypes = sheetTypes;
        this.unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<PhaseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var all = await phases.ListWithSheetTypesAsync(cancellationToken);

        return all.Select(phase => phase.ToDto()).ToList();
    }

    public async Task<PhaseDto> GetAsync(int id, CancellationToken cancellationToken)
    {
        var phase = await phases.FindWithSheetTypesAsync(id, cancellationToken);

        if (phase is null)
        {
            throw new NotFoundException($"Phase {id} was not found.");
        }

        return phase.ToDto();
    }

    public async Task<PhaseDto> CreateAsync(CreatePhaseRequest request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();
        if (await phases.CodeExistsAsync(code, cancellationToken))
        {
            throw new ConflictException($"A phase with the code \"{code}\" already exists.");
        }

        var parentId = request.ParentPhaseId;
        if (parentId is not null && !await phases.ExistsAsync(parentId.Value, cancellationToken))
        {
            throw new NotFoundException($"Parent phase {parentId} was not found.");
        }

        var sheetTypeIds = await ExistingSheetTypeIdsAsync(request.SheetTypeIds ?? [], cancellationToken);

        var lastOrder = await phases.GetLastDisplayOrderAsync(parentId, cancellationToken);

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

        phases.Add(phase);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return phase.ToDto();
    }

    public async Task<PhaseDto> SetSheetTypesAsync(int id, SetPhaseSheetTypesRequest request, CancellationToken cancellationToken)
    {
        var phase = await phases.FindForEditAsync(id, cancellationToken);

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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return phase.ToDto();
    }

    public async Task<IReadOnlyList<PhaseDto>> MoveAsync(int id, MovePhaseRequest request, CancellationToken cancellationToken)
    {
        // Every phase is loaded: the move renumbers two groups of siblings and checks the whole chain of parents.
        var all = await phases.ListForEditAsync(cancellationToken);

        var phase = all.SingleOrDefault(candidate => candidate.Id == id);
        if (phase is null)
        {
            throw new NotFoundException($"Phase {id} was not found.");
        }

        PhaseMover.Move(all, phase, request.ParentPhaseId, request.Position);

        // One save, so the move and both renumberings are stored together or not at all.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return all
            .OrderBy(candidate => candidate.DisplayOrder)
            .ThenBy(candidate => candidate.Code)
            .Select(candidate => candidate.ToDto())
            .ToList();
    }

    /// <summary>The distinct ids asked for, once every one is known to be a sheet type.</summary>
    private async Task<HashSet<int>> ExistingSheetTypeIdsAsync(IReadOnlyList<int> sheetTypeIds, CancellationToken cancellationToken)
    {
        var wanted = sheetTypeIds.ToHashSet();
        var existing = await sheetTypes.GetExistingIdsAsync(wanted, cancellationToken);

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
        var inUse = await phases.FindSheetTypeWithTablesAsync(phase.Id, removedIds, cancellationToken);

        if (inUse is not null)
        {
            throw new ConflictException(
                $"The {inUse} sheet of {phase.Code} already has tables, so {inUse} can't be removed from this phase.");
        }
    }
}
