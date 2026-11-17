using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.Database.Models.Phases;

namespace PUSpecSheet.DataAccess.Phases;

internal static class PhaseMover
{
    public static void Move(IReadOnlyCollection<DbPhase> all, DbPhase phase, int? newParentId, int position)
    {
        if (newParentId is { } parentId)
        {
            if (all.All(candidate => candidate.Id != parentId))
            {
                throw new NotFoundException($"Parent phase {parentId} was not found.");
            }

            if (IsInSubtree(all, phase.Id, parentId))
            {
                throw new InvalidRequestException($"{phase.Code} can't be moved under itself or one of the phases beneath it.");
            }
        }

        var oldParentId = phase.ParentPhaseId;
        phase.ParentPhaseId = newParentId;
        DisplayOrdering.Move(SiblingsUnder(all, newParentId), phase, position, GetOrder, SetOrder);

        if (oldParentId != newParentId)
        {
            DisplayOrdering.Renumber(SiblingsUnder(all, oldParentId), GetOrder, SetOrder);
        }
    }

    public static bool IsInSubtree(IReadOnlyCollection<DbPhase> all, int rootId, int candidateId)
    {
        var parents = all.ToDictionary(phase => phase.Id, phase => phase.ParentPhaseId);
        var seen = new HashSet<int>();
        int? current = candidateId;
        while (current is { } id && seen.Add(id))
        {
            if (id == rootId)
            {
                return true;
            }

            current = parents.GetValueOrDefault(id);
        }

        return false;
    }

    private static List<DbPhase> SiblingsUnder(IReadOnlyCollection<DbPhase> all, int? parentId)
    {
        return all
            .Where(candidate => candidate.ParentPhaseId == parentId)
            .OrderBy(candidate => candidate.DisplayOrder)
            .ThenBy(candidate => candidate.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int GetOrder(DbPhase phase)
    {
        return phase.DisplayOrder;
    }

    private static void SetOrder(DbPhase phase, int order)
    {
        phase.DisplayOrder = order;
    }
}
