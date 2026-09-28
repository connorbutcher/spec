import { Phase } from '../../core/models/phase.model';
import { PhaseTreeNode } from '../../core/models/phase-tree-node.model';

/** Sibling order: display order, then code. */
export function comparePhases(a: Phase, b: Phase): number {
  if (a.displayOrder !== b.displayOrder) {
    return a.displayOrder - b.displayOrder;
  }
  return a.code.localeCompare(b.code, undefined, { numeric: true });
}

/**
 * Builds the phase tree from the flat list. A phase whose parent isn't in the list is treated as a
 * top-level phase so it never disappears from view.
 */
export function buildPhaseTree(phases: readonly Phase[]): PhaseTreeNode[] {
  const ids = new Set(phases.map((phase) => phase.id));
  const childrenByParent = new Map<number | null, Phase[]>();

  for (const phase of phases) {
    const parentId =
      phase.parentPhaseId !== null && ids.has(phase.parentPhaseId) ? phase.parentPhaseId : null;
    const siblings = childrenByParent.get(parentId) ?? [];
    siblings.push(phase);
    childrenByParent.set(parentId, siblings);
  }

  const toNodes = (parentId: number | null): PhaseTreeNode[] =>
    [...(childrenByParent.get(parentId) ?? [])]
      .sort(comparePhases)
      .map((phase) => ({ phase, children: toNodes(phase.id) }));

  return toNodes(null);
}

/**
 * Keeps the nodes whose code or description matches `query`, plus the ancestors needed to reach
 * them. A matching node keeps all of its children.
 */
export function filterPhaseTree(nodes: readonly PhaseTreeNode[], query: string): PhaseTreeNode[] {
  const term = query.trim().toLowerCase();
  if (!term) {
    return [...nodes];
  }

  const result: PhaseTreeNode[] = [];
  for (const node of nodes) {
    if (phaseMatches(node.phase, term)) {
      result.push(node);
      continue;
    }

    const children = filterPhaseTree(node.children, term);
    if (children.length > 0) {
      result.push({ phase: node.phase, children });
    }
  }
  return result;
}

/** The chain of ancestors from the top level down to (but not including) `phaseId`. */
export function phaseAncestors(phaseId: number, phasesById: ReadonlyMap<number, Phase>): Phase[] {
  const ancestors: Phase[] = [];
  const seen = new Set<number>([phaseId]);
  let parentId = phasesById.get(phaseId)?.parentPhaseId ?? null;

  while (parentId !== null && !seen.has(parentId)) {
    const parent = phasesById.get(parentId);
    if (!parent) {
      break;
    }
    ancestors.unshift(parent);
    seen.add(parentId);
    parentId = parent.parentPhaseId;
  }
  return ancestors;
}

function phaseMatches(phase: Phase, term: string): boolean {
  return (
    phase.code.toLowerCase().includes(term) ||
    (phase.description?.toLowerCase().includes(term) ?? false)
  );
}
