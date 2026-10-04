import { ColumnPlan } from '../templates/models/column-plan.model';
import { SheetColumnBlock } from './models/sheet-column-block.model';
import { SheetTable } from './models/sheet-table.model';
import { fixtureTable } from './sheet-structure.fixture';
import { STICKY_COLUMN_WIDTH, stickyColumnLefts, stickyColumnSizes } from './sheet-sticky.util';

function block(id: number, stickyColumnCount: number): SheetColumnBlock {
  return {
    id,
    publicId: `block-${id}`,
    templateColumnBlockId: 1,
    name: 'Part',
    minInstances: 0,
    maxInstances: null,
    initialInstances: 1,
    stickyColumnCount,
    displayOrder: id,
    lock: null,
    isPending: false,
    canRemove: true,
    lastChange: null,
  };
}

/** A stub column, then two copies of a two-column block (columns 2-3 and 4-5). */
function table(stickyColumnCount: number, blockSticky: number): SheetTable {
  return {
    ...fixtureTable(),
    stickyColumnCount,
    columnBlocks: [block(10, blockSticky), block(11, blockSticky)],
  };
}

const plan: ColumnPlan = {
  total: 5,
  blockStarts: new Map([
    [10, 2],
    [11, 4],
  ]),
  blockWidths: new Map([
    [10, 2],
    [11, 2],
  ]),
};

describe('stickyColumnLefts', () => {
  it('pins nothing when no counts are set', () => {
    expect(stickyColumnLefts(table(0, 0), plan).size).toBe(0);
  });

  it('pins the stub column at the left edge and leaves the value columns free', () => {
    const lefts = stickyColumnLefts(table(1, 0), plan);
    expect([...lefts]).toEqual([[1, 0]]);
  });

  it('stacks pinned columns side by side in order', () => {
    const lefts = stickyColumnLefts(
      { ...table(2, 0), stickyColumnCount: 2 },
      {
        ...plan,
        total: 6,
        blockStarts: new Map([
          [10, 3],
          [11, 5],
        ]),
      },
    );
    expect(lefts.get(1)).toBe(0);
    expect(lefts.get(2)).toBe(STICKY_COLUMN_WIDTH);
  });

  it('pins the first columns of each block copy just after the stub', () => {
    const lefts = stickyColumnLefts(table(1, 1), plan);
    expect(lefts.get(1)).toBe(0);
    expect(lefts.get(2)).toBe(STICKY_COLUMN_WIDTH);
    expect(lefts.get(4)).toBe(STICKY_COLUMN_WIDTH);
    expect(lefts.has(3)).toBe(false);
  });

  it('cuts a count down to the columns that exist', () => {
    const lefts = stickyColumnLefts(table(9, 0), plan);
    expect([...lefts.keys()]).toEqual([1]);
  });
});

describe('stickyColumnSizes', () => {
  it('gives pinned columns a fixed width and the rest the flexible one', () => {
    const sizes = stickyColumnSizes(plan, new Map([[1, 0]]));
    expect(sizes).toBe(
      `${STICKY_COLUMN_WIDTH}px minmax(112px, 1fr) minmax(112px, 1fr) minmax(112px, 1fr) minmax(112px, 1fr)`,
    );
  });
});
