using PUSpecSheet.Abstractions.Phases;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.SheetTypes;
using PUSpecSheet.Database.Mappers.Phases;
using PUSpecSheet.Database.Models.Phases;

namespace PUSpecSheet.DataAccess.Phases;

public sealed class PhasesService : IPhasesService
{
    private readonly IPhaseDataService phases;
    private readonly ISheetTypeDataService sheetTypes;

    public PhasesService(IPhaseDataService phases, ISheetTypeDataService sheetTypes)
    {
        this.phases = phases;
        this.sheetTypes = sheetTypes;
    }

    public async Task<IReadOnlyList<PhaseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var all = await phases.ListAsync(cancellationToken);
        return all.Select(phase => phase.ToDto()).ToList();
    }

    public async Task<PhaseDto> CreateAsync(CreatePhaseRequest request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();
        if (await phases.CodeExistsAsync(code, cancellationToken))
        {
            throw new ConflictException($"A phase with the code \"{code}\" already exists.");
        }

        var parentId = request.ParentPhaseId;
        if (parentId is { } id && !await phases.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Parent phase {parentId} was not found.");
        }

        var sheetTypeIds = await CheckSheetTypesExistAsync(request.SheetTypeIds ?? [], cancellationToken);
        var lastOrder = await phases.GetLastDisplayOrderAsync(parentId, cancellationToken) ?? 0;

        var phase = new DbPhase
        {
            Code = code,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            ParentPhaseId = parentId,
            DisplayOrder = lastOrder + 1,
            SheetTypes = sheetTypeIds.Select(sheetTypeId => new DbPhaseSheetType { SheetTypeId = sheetTypeId }).ToList(),
        };

        phases.Add(phase);
        await phases.SaveAsync(cancellationToken);
        return phase.ToDto();
    }

    public async Task<PhaseDto> SetSheetTypesAsync(int id, SetPhaseSheetTypesRequest request, CancellationToken cancellationToken)
    {
        var phase = await phases.FindForEditAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Phase {id} was not found.");

        var wanted = await CheckSheetTypesExistAsync(request.SheetTypeIds, cancellationToken);
        var removed = phase.SheetTypes.Where(link => !wanted.Contains(link.SheetTypeId)).ToList();

        // A sheet type can't leave a phase once its sheet there has tables: they would be stranded.
        if (removed.Count > 0)
        {
            var inUse = await phases.FindSheetTypeWithTablesAsync(phase.Id, removed.Select(link => link.SheetTypeId).ToList(), cancellationToken);
            if (inUse is not null)
            {
                throw new ConflictException($"The {inUse} sheet of {phase.Code} already has tables, so {inUse} can't be removed from this phase.");
            }
        }

        foreach (var link in removed)
        {
            phase.SheetTypes.Remove(link);
        }

        var current = phase.SheetTypes.Select(link => link.SheetTypeId).ToHashSet();
        foreach (var sheetTypeId in wanted.Where(sheetTypeId => !current.Contains(sheetTypeId)))
        {
            phase.SheetTypes.Add(new DbPhaseSheetType { SheetTypeId = sheetTypeId });
        }

        await phases.SaveAsync(cancellationToken);
        return phase.ToDto();
    }

    public async Task<IReadOnlyList<PhaseDto>> MoveAsync(int id, MovePhaseRequest request, CancellationToken cancellationToken)
    {
        // Every phase is loaded: the move renumbers two groups of siblings and checks the whole chain of parents.
        var all = await phases.ListForEditAsync(cancellationToken);
        var phase = all.SingleOrDefault(candidate => candidate.Id == id)
            ?? throw new NotFoundException($"Phase {id} was not found.");

        PhaseMover.Move(all, phase, request.ParentPhaseId, request.Position);
        await phases.SaveAsync(cancellationToken);

        return all
            .OrderBy(candidate => candidate.DisplayOrder)
            .ThenBy(candidate => candidate.Code)
            .Select(candidate => candidate.ToDto())
            .ToList();
    }

    private async Task<HashSet<int>> CheckSheetTypesExistAsync(IReadOnlyList<int> sheetTypeIds, CancellationToken cancellationToken)
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
}
