import { GridStyle } from '../templates/models/grid-style';
import { SectionLayout } from '../templates/models/section-layout.model';
import { TemplateLayout } from '../templates/models/template-layout.model';

/** The width of the status column. Fixed, because the pinned columns after it sit this far from the left. */
export const STATUS_COLUMN_WIDTH = 168;

/**
 * Gives a laid-out sheet table its status column: one more column in front of the others, which every
 * table has whatever its template looks like. Everything already placed moves one column to the right,
 * and what is pinned stays pinned just right of the status column, which is itself pinned at the edge.
 *
 * Sections are untouched: each spans every column, so each takes in the new one.
 */
export function addStatusColumn(layout: TemplateLayout): void {
  layout.style['grid-template-columns'] =
    `${STATUS_COLUMN_WIDTH}px ${layout.style['grid-template-columns']}`;
  for (const section of layout.sections) {
    shiftSection(section);
  }
  for (const block of layout.columnBlocks) {
    shift(block.style);
  }
}

/** Where a row's status sits: the status column, on the row's own line of its section. */
export function rowStatusStyle(rowIndex: number): GridStyle {
  return { 'grid-row': `${rowIndex + 1}`, 'grid-column': '1' };
}

/** Where the column's heading sits: the status column, down every row of the header section. */
export function statusHeadingStyle(): GridStyle {
  return { 'grid-row': '1 / -1', 'grid-column': '1' };
}

function shiftSection(section: SectionLayout): void {
  for (const placed of section.cells) {
    shift(placed.style);
  }
  for (const child of section.children) {
    shiftSection(child);
  }
}

/** Moves "3 / span 2" to "4 / span 2", and a pinned item's distance from the left along with it. */
function shift(style: GridStyle): void {
  const match = /^(\d+) \/ span (\d+)$/.exec(style['grid-column'] ?? '');
  if (match) {
    style['grid-column'] = `${Number(match[1]) + 1} / span ${match[2]}`;
  }
  if (style['position'] === 'sticky' && style['left'] !== undefined) {
    style['left'] = `${parseFloat(style['left']) + STATUS_COLUMN_WIDTH}px`;
  }
}
