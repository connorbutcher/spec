import { Phase } from '../../core/models/phase.model';
import { buildPhaseTree, filterPhaseTree, phaseAncestors } from './phase-tree.util';

function phase(id: number, code: string, parentPhaseId: number | null, displayOrder = 1): Phase {
  return { id, code, description: null, displayOrder, parentPhaseId, sheetTypeIds: [] };
}

const phases: Phase[] = [
  phase(1, 'V6', null, 1),
  phase(2, 'A3', 1, 2),
  phase(3, '01-A2', 1, 1),
  phase(4, 'SC', null, 2),
  phase(5, 'Orphan', 99, 3),
];

describe('buildPhaseTree', () => {
  it('nests children under their parent in display order', () => {
    const tree = buildPhaseTree(phases);
    expect(tree.map((node) => node.phase.code)).toEqual(['V6', 'SC', 'Orphan']);
    expect(tree[0].children.map((node) => node.phase.code)).toEqual(['01-A2', 'A3']);
  });

  it('keeps a phase with a missing parent at the top level', () => {
    const tree = buildPhaseTree(phases);
    expect(tree.some((node) => node.phase.code === 'Orphan')).toBe(true);
  });
});

describe('filterPhaseTree', () => {
  it('keeps matching phases and the ancestors that lead to them', () => {
    const tree = filterPhaseTree(buildPhaseTree(phases), 'a3');
    expect(tree.map((node) => node.phase.code)).toEqual(['V6']);
    expect(tree[0].children.map((node) => node.phase.code)).toEqual(['A3']);
  });

  it('returns everything for an empty query', () => {
    expect(filterPhaseTree(buildPhaseTree(phases), '  ').length).toBe(3);
  });
});

describe('phaseAncestors', () => {
  it('returns the path from the top level down to the parent', () => {
    const byId = new Map(phases.map((p) => [p.id, p]));
    expect(phaseAncestors(3, byId).map((p) => p.code)).toEqual(['V6']);
    expect(phaseAncestors(1, byId)).toEqual([]);
  });
});
