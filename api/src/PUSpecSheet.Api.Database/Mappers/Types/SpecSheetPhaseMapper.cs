using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Mappers.Types;

public static class SpecSheetPhaseMapper
{
    public static SpecSheetPhase MapToDomain(this DbSpecSheetPhase dbSpecSheetPhase)
    {
        return new SpecSheetPhase
        {
            Id = dbSpecSheetPhase.Id,
            ParentSpecSheetPhaseId = dbSpecSheetPhase.ParentSpecSheetPhaseId,
            Phase = dbSpecSheetPhase.Phase,
            PhaseShort = dbSpecSheetPhase.PhaseShort,
            IsObsolete = dbSpecSheetPhase.IsObsolete,
            Types = [.. dbSpecSheetPhase.SpecSheetTypes.MapToDomain()],
            PhaseObservers = [.. dbSpecSheetPhase.PhaseObservers.MapToDomain()],
            DisplayOrder = dbSpecSheetPhase.DisplayOrder,
            Phases = [],
        };
    }

    public static IEnumerable<SpecSheetPhase> MapToDomain(this ICollection<DbSpecSheetPhase> dbSpecSheetPhases)
    {
        return dbSpecSheetPhases.Select(MapToDomain);
    }

    public static IQueryable<SpecSheetPhase> MapToDomain(this IQueryable<DbSpecSheetPhase> dbSpecSheetPhases)
    {
        return dbSpecSheetPhases.Select(dbSpecSheetType => dbSpecSheetType.MapToDomain());
    }

    public static IReadOnlyList<SpecSheetPhase> MapToHierarchy(this List<SpecSheetPhase> specSheetPhases)
    {
        var phaseDictionary = specSheetPhases.ToDictionary(phase => phase.Id);
        foreach (var phase in specSheetPhases)
        {
            if (phase.ParentSpecSheetPhaseId.HasValue &&
                phaseDictionary.TryGetValue(phase.ParentSpecSheetPhaseId.Value, out var parentPhase))
            {
                parentPhase.Phases.Add(phase);
            }
        }

        // Return only root phases (those without a parent) and order them and the children by DisplayOrder            
        return phaseDictionary.Values
            .Where(phase => !phase.ParentSpecSheetPhaseId.HasValue)
            .OrderBy(phase => phase.DisplayOrder)
            .Select(phase =>
            {
                phase.Phases = [.. phase.Phases.OrderBy(child => child.DisplayOrder)];
                return phase;
            })
            .ToList();
    }
}
