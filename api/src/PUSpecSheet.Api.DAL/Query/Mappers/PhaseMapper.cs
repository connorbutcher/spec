using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Query.Models.Phase;

namespace PUSpecSheet.Api.DAL.Query.Mappers;

internal static class PhaseMapper
{
    public static SlimSpecSheetPhaseWithChildren MapToPhase(this SpecSheetPhase phase)
    {
        return new SlimSpecSheetPhaseWithChildren
        {
            Id = phase.Id,
            ParentSpecSheetPhaseId = phase.ParentSpecSheetPhaseId,
            Phase = phase.Phase,
            DisplayOrder = phase.DisplayOrder,
            IsObsolete = phase.IsObsolete,
            Phases = [.. phase.Phases.MapToPhases()],
            TypesSupportedForPhase = [.. phase.Types.MapToSlimTypes()]
        };
    }

    public static IEnumerable<SlimSpecSheetPhaseWithChildren> MapToPhases(this IEnumerable<SpecSheetPhase> phases)
    {
        return phases.Select(MapToPhase);
    }

    public static SlimPUSpecSheetPhase MapToSlimPhase(this SpecSheetPhase phase)
    {
        return new SlimPUSpecSheetPhase
        {
            Id = phase.Id,
            ParentSpecSheetPhaseId = phase.ParentSpecSheetPhaseId,
            Phase = phase.Phase,
            DisplayOrder = phase.DisplayOrder,
            IsObsolete = phase.IsObsolete,
        };
    }
}
