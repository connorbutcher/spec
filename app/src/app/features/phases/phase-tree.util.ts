import { TreeNode } from 'primeng/api';
import { Phase } from '../../core/models/phase.model';

/** Sibling order: display order, then code. */
export function comparePhases(a: Phase, b: Phase): number {
  if (a.displayOrder !== b.displayOrder) {
    return a.displayOrder - b.displayOrder;
  }
  return a.code.localeCompare(b.code, undefined, { numeric: true });
}

/**
 * Builds the PrimeNG tree nodes from the flat phase list, expanded by default. A phase whose parent
 * isn't in the list is treated as a top-level phase so it never disappears from view.
 */
export function buildPhaseTree(phases: readonly Phase[]): TreeNode<Phase>[] {
  const ids = new Set(phases.map((phase) => phase.id));
  const childrenByParent = new Map<number | null, Phase[]>();

  for (const phase of phases) {
    const parentId =
      phase.parentPhaseId !== null && ids.has(phase.parentPhaseId) ? phase.parentPhaseId : null;
    const siblings = childrenByParent.get(parentId) ?? [];
    siblings.push(phase);
    childrenByParent.set(parentId, siblings);
  }

  const toNodes = (parentId: number | null): TreeNode<Phase>[] =>
    [...(childrenByParent.get(parentId) ?? [])].sort(comparePhases).map((phase) => {
      const children = toNodes(phase.id);
      return {
        key: String(phase.id),
        label: phase.code,
        data: phase,
        children,
        leaf: children.length === 0,
        expanded: true,
      };
    });

  return toNodes(null);
}

/** Every node in the tree, keyed by phase id. */
export function indexTreeNodes(nodes: readonly TreeNode<Phase>[]): Map<number, TreeNode<Phase>> {
  const index = new Map<number, TreeNode<Phase>>();
  const visit = (node: TreeNode<Phase>): void => {
    if (node.data) {
      index.set(node.data.id, node);
    }
    node.children?.forEach(visit);
  };
  nodes.forEach(visit);
  return index;
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
