import { Phase } from '../../core/models/phase.model';
import { PhaseTreeUtil } from './phase-tree.util';

class SpecHelpers {
  public static phase(id: number, code: string, parentPhaseId: number | null, displayOrder = 1): Phase {
    return { id, code, description: null, displayOrder, parentPhaseId, sheetTypeIds: [] };
  }
}

const phases: Phase[] = [
  SpecHelpers.phase(1, 'V6', null, 1),
  SpecHelpers.phase(2, 'A3', 1, 2),
  SpecHelpers.phase(3, '01-A2', 1, 1),
  SpecHelpers.phase(4, 'SC', null, 2),
  SpecHelpers.phase(5, 'Orphan', 99, 3),
];

describe('buildPhaseTree', () => {
  it('nests children under their parent in display order', () => {
    const tree = PhaseTreeUtil.buildPhaseTree(phases);
    expect(tree.map((node) => node.label)).toEqual(['V6', 'SC', 'Orphan']);
    expect(tree[0].children?.map((node) => node.label)).toEqual(['01-A2', 'A3']);
  });

  it('marks nodes without children as leaves and expands everything', () => {
    const tree = PhaseTreeUtil.buildPhaseTree(phases);
    expect(tree[0].leaf).toBe(false);
    expect(tree[1].leaf).toBe(true);
    expect(tree.every((node) => node.expanded)).toBe(true);
  });

  it('keeps a phase with a missing parent at the top level', () => {
    const tree = PhaseTreeUtil.buildPhaseTree(phases);
    expect(tree.some((node) => node.label === 'Orphan')).toBe(true);
  });
});

describe('indexTreeNodes', () => {
  it('finds nested nodes by phase id', () => {
    const index = PhaseTreeUtil.indexTreeNodes(PhaseTreeUtil.buildPhaseTree(phases));
    expect(index.get(3)?.label).toBe('01-A2');
    expect(index.size).toBe(5);
  });
});

describe('phaseAncestors', () => {
  it('returns the path from the top level down to the parent', () => {
    const byId = new Map(phases.map((p) => [p.id, p]));
    expect(PhaseTreeUtil.phaseAncestors(3, byId).map((p) => p.code)).toEqual(['V6']);
    expect(PhaseTreeUtil.phaseAncestors(1, byId)).toEqual([]);
  });
});
