using PUSpecSheet.Application.Common;
using PUSpecSheet.Domain.Phases;

namespace PUSpecSheet.Application.Phases;

/// <summary>The rules for moving a phase within the tree, worked on phases already loaded.</summary>
internal static class PhaseMover
{
    /// <summary>
    /// Moves <paramref name="phase"/> to a 1-based <paramref name="position"/> under
    /// <paramref name="newParentId"/> (the top level when null) and renumbers the siblings it joins, and
    /// the ones it leaves, 1..n. <paramref name="all"/> is every phase, including the one moved. A phase
    /// can't go under itself or anything beneath it.
    /// </summary>
    public static void Move(IReadOnlyCollection<Phase> all, Phase phase, int? newParentId, int position)
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

    /// <summary>Whether <paramref name="candidateId"/> is <paramref name="rootId"/> or sits anywhere beneath it.</summary>
    public static bool IsInSubtree(IReadOnlyCollection<Phase> all, int rootId, int candidateId)
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

    /// <summary>The phases under a parent as they are listed: by display order, then code.</summary>
    private static List<Phase> SiblingsUnder(IReadOnlyCollection<Phase> all, int? parentId)
    {
        return all
            .Where(candidate => candidate.ParentPhaseId == parentId)
            .OrderBy(candidate => candidate.DisplayOrder)
            .ThenBy(candidate => candidate.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int GetOrder(Phase phase)
    {
        return phase.DisplayOrder;
    }

    private static void SetOrder(Phase phase, int order)
    {
        phase.DisplayOrder = order;
    }
}
