using PUSpecSheet.Query.Models.Phase;

namespace PUSpecSheet.Query.Helpers;

public static class PUSpecSheetPhaseHelpers
{
    public static SlimSpecSheetPhaseWithChildren? FindPhaseById(this IEnumerable<SlimSpecSheetPhaseWithChildren> phases, int id)
    {
        foreach (var phase in phases)
        {
            if (phase.Id == id)
            {
                return phase;
            }

            var foundInSubPhase = FindPhaseById(phase.Phases, id);
            if (foundInSubPhase != null)
            {
                return foundInSubPhase;
            }
        }

        return null;
    }
}
