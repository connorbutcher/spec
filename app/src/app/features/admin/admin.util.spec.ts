import { Phase } from '../../core/models/phase.model';
import { SheetType } from '../../core/models/sheet-type.model';
import { buildPhaseTree } from '../phases/phase-tree.util';
import { phaseLocation, phaseOptions, sameIds, sheetTypeUsage } from './admin.util';

function phase(
  id: number,
  code: string,
  parentPhaseId: number | null,
  displayOrder: number,
  sheetTypeIds: number[] = [],
): Phase {
  return { id, code, description: null, displayOrder, parentPhaseId, sheetTypeIds };
}

const tree = buildPhaseTree([
  phase(1, 'V6', null, 1, [1, 2]),
  phase(2, 'A3', 1, 2, [2]),
  phase(3, '01-A2', 1, 1, [1]),
  phase(4, 'SC', null, 2),
  phase(5, 'Rig', 3, 1, [2]),
]);

const sheetTypes: SheetType[] = [
  { id: 1, name: 'Specification', displayOrder: 1 },
  { id: 2, name: 'Parts', displayOrder: 2 },
  { id: 3, name: 'PFKs', displayOrder: 3 },
];

describe('phaseOptions', () => {
  it('lists every phase in tree order with its path', () => {
    expect(phaseOptions(tree)).toEqual([
      { id: 1, label: 'V6' },
      { id: 3, label: 'V6 › 01-A2' },
      { id: 5, label: 'V6 › 01-A2 › Rig' },
      { id: 2, label: 'V6 › A3' },
      { id: 4, label: 'SC' },
    ]);
  });

  it('is empty when there are no phases', () => {
    expect(phaseOptions([])).toEqual([]);
  });
});

describe('sheetTypeUsage', () => {
  it('lists the phases that have each sheet type, in tree order', () => {
    const usage = sheetTypeUsage(sheetTypes, tree);
    expect(usage.map((entry) => entry.sheetType.name)).toEqual(['Specification', 'Parts', 'PFKs']);
    expect(usage.map((entry) => entry.phaseCodes)).toEqual([
      ['V6', '01-A2'],
      ['V6', 'Rig', 'A3'],
      [],
    ]);
  });
});

describe('phaseLocation', () => {
  it('finds a top-level phase', () => {
    expect(phaseLocation(tree, 4)).toEqual({ parentPhaseId: null, position: 2 });
  });

  it('finds a nested phase under its parent', () => {
    expect(phaseLocation(tree, 2)).toEqual({ parentPhaseId: 1, position: 2 });
    expect(phaseLocation(tree, 5)).toEqual({ parentPhaseId: 3, position: 1 });
  });

  it('follows the nodes as they stand after one is moved', () => {
    const moved = buildPhaseTree([
      phase(1, 'V6', null, 1),
      phase(2, 'A3', 1, 1),
      phase(4, 'SC', null, 2),
    ]);
    const [a3] = moved[0].children!.splice(0, 1);
    moved[1].children = [a3];

    expect(phaseLocation(moved, 2)).toEqual({ parentPhaseId: 4, position: 1 });
  });

  it('is null for a phase the tree does not hold', () => {
    expect(phaseLocation(tree, 99)).toBeNull();
  });
});

describe('sameIds', () => {
  it('ignores order', () => {
    expect(sameIds([1, 2, 3], [3, 1, 2])).toBe(true);
  });

  it('spots an added, removed or swapped id', () => {
    expect(sameIds([1, 2], [1, 2, 3])).toBe(false);
    expect(sameIds([1, 2, 3], [1, 2])).toBe(false);
    expect(sameIds([1, 2], [1, 3])).toBe(false);
  });
});
