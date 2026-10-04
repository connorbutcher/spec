import { ColumnPlan } from '../templates/models/column-plan.model';
import { TemplateLayout } from '../templates/models/template-layout.model';
import { SectionLayout } from '../templates/models/section-layout.model';
import { SheetTable } from './models/sheet-table.model';

/** The width of a pinned column. Fixed, so each pinned column's left offset is the width of those before it. */
export const STICKY_COLUMN_WIDTH = 160;

/** The colour of the line after the last pinned column, which stands in for that cell's own border. */
const DIVIDER_COLOUR = '#94a3b8';

/**
 * The pinned columns of a table: the first `stickyColumnCount` of its own columns, then the first
 * `stickyColumnCount` columns of each column block copy. Each is given the distance from the left edge it
 * stays at (the widths of the pinned columns before it), so pinned columns sit side by side in order.
 * Counts larger than the columns there are are cut down to what exists.
 */
export function stickyColumnLefts(
  table: SheetTable,
  plan: ColumnPlan,
): ReadonlyMap<number, number> {
  const lefts = new Map<number, number>();
  const firstBlockColumn = Math.min(plan.total + 1, ...plan.blockStarts.values());
  const ownColumns = Math.max(0, firstBlockColumn - 1);

  let offset = 0;
  const pin = (column: number): void => {
    lefts.set(column, offset);
    offset += STICKY_COLUMN_WIDTH;
  };

  for (let column = 1; column <= Math.min(table.stickyColumnCount, ownColumns); column++) {
    pin(column);
  }

  const ownPinned = offset;
  for (const block of table.columnBlocks) {
    const start = plan.blockStarts.get(block.id);
    const width = plan.blockWidths.get(block.id) ?? 0;
    if (start === undefined) {
      continue;
    }
    // Every copy of a block stops at the same place, just right of the table's own pinned columns, so
    // a pinned block column replaces the one before it as it scrolls into place.
    offset = ownPinned;
    for (let column = start; column < start + Math.min(block.stickyColumnCount, width); column++) {
      pin(column);
    }
  }

  return lefts;
}

/** The table's column sizes, with a fixed width for each pinned column and the usual flexible one for the rest. */
export function stickyColumnSizes(plan: ColumnPlan, lefts: ReadonlyMap<number, number>): string {
  return Array.from({ length: plan.total }, (_, index) =>
    lefts.has(index + 1) ? `${STICKY_COLUMN_WIDTH}px` : 'minmax(112px, 1fr)',
  ).join(' ');
}

/**
 * Pins the cells that sit wholly inside pinned columns. A cell that also spans scrolling columns (such as
 * a group heading across the whole table) can't stay put without covering them, so it scrolls. The last
 * pinned column gets a darker rule in place of its usual border, so there is one line, not two.
 */
export function applySticky(layout: TemplateLayout, lefts: ReadonlyMap<number, number>): void {
  if (lefts.size === 0) {
    return;
  }

  const lastPinned = Math.max(...lefts.keys());
  for (const section of layout.sections) {
    pinSection(section, lefts, lastPinned);
  }
}

function pinSection(
  section: SectionLayout,
  lefts: ReadonlyMap<number, number>,
  lastPinned: number,
): void {
  for (const placed of section.cells) {
    const [start, span] = parseColumn(placed.style['grid-column']);
    const left = lefts.get(start);
    const wholly =
      left !== undefined &&
      Array.from({ length: span }, (_, index) => start + index).every((column) =>
        lefts.has(column),
      );
    if (left === undefined || !wholly) {
      continue;
    }

    placed.style['position'] = 'sticky';
    placed.style['left'] = `${left}px`;
    placed.style['z-index'] = '4';
    if (start + span - 1 === lastPinned) {
      placed.style['border-right-color'] = DIVIDER_COLOUR;
    }
  }

  for (const child of section.children) {
    pinSection(child, lefts, lastPinned);
  }
}

/** Reads "3 / span 2" into its start column and span. */
function parseColumn(value: string | undefined): [number, number] {
  const match = /^(\d+) \/ span (\d+)$/.exec(value ?? '');
  return match ? [Number(match[1]), Number(match[2])] : [0, 1];
}
