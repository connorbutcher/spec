import { Phase } from '../../core/models/phase.model';
import { SheetType } from '../../core/models/sheet-type.model';
import { buildPhaseTree } from '../phases/phase-tree.util';
import { phaseOptions, sameIds, sheetTypeUsage } from './admin.util';

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
