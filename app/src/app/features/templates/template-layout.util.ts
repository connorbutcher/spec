import { CellLayout } from './models/cell-layout.model';
import { GridStyle } from './models/grid-style';
import { SectionLayout } from './models/section-layout.model';
import { TableTemplate } from './models/table-template.model';
import { TemplateLayout } from './models/template-layout.model';
import { TemplateSection } from './models/template-section.model';

/** The direction sections are added in: stacked top to bottom, or side by side left to right. */
type Flow = 'down' | 'across';

interface Size {
  rows: number;
  columns: number;
}

/**
 * Lays a template out as one CSS grid with every section as a nested subgrid. Sections draw nothing
 * themselves; only their rows and cells show. Rows always run across the page and cells sit side by
 * side in them.
 *
 * Orientation is the direction sections are added in, at every level:
 * - Vertical: the header, the addable sections and the sub-sections inside them all stack top to bottom.
 * - Horizontal: they all run left to right.
 *
 * A section's own rows come first, then its sub-sections.
 */
export function layoutTemplate(template: TableTemplate): TemplateLayout {
  const flow: Flow = template.orientation === 'Vertical' ? 'down' : 'across';
  const sections = topLevelOrder(template.sections);
  const sizes = sections.map((section) => sectionSize(section, flow));

  const rows = combine(
    sizes.map((size) => size.rows),
    flow === 'down',
  );
  const columns = combine(
    sizes.map((size) => size.columns),
    flow === 'across',
  );

  return {
    style: {
      'grid-template-rows': `repeat(${rows}, minmax(30px, auto))`,
      'grid-template-columns': `repeat(${columns}, minmax(112px, 1fr))`,
    },
    sections: layoutSiblings(sections, flow, 0, rows),
  };
}

/** The header first, then the addable sections in list order. */
export function topLevelOrder(sections: TemplateSection[]): TemplateSection[] {
  return [
    ...sections.filter((section) => section.role === 'Header'),
    ...sections.filter((section) => section.role !== 'Header'),
  ];
}

/**
 * Places sections one after another in `flow`, each filling the parent the other way. `rowOffset` is
 * how many of the parent's rows its own rows take, and `rows` how many rows the sections share below them.
 */
function layoutSiblings(
  sections: TemplateSection[],
  flow: Flow,
  rowOffset: number,
  rows: number,
): SectionLayout[] {
  let start = 1;
  return sections.map((section) => {
    const size = sectionSize(section, flow);
    const style: GridStyle =
      flow === 'down'
        ? { 'grid-row': `${rowOffset + start} / span ${size.rows}`, 'grid-column': '1 / -1' }
        : {
            'grid-column': `${start} / span ${size.columns}`,
            'grid-row': `${rowOffset + 1} / span ${rows}`,
          };
    start += flow === 'down' ? size.rows : size.columns;
    return layoutSection(section, style, flow);
  });
}

function layoutSection(section: TemplateSection, style: GridStyle, flow: Flow): SectionLayout {
  const ownRows = rowExtent(section);
  const children = childExtent(section, flow);

  return {
    section,
    isLeaf: section.sections.length === 0,
    style,
    emptyStyle: emptyStyle(ownRows + children.rows),
    children: layoutSiblings(section.sections, flow, ownRows, children.rows),
    cells: layoutCells(section),
  };
}

/**
 * Where the placeholder sits for a section with nothing in it yet (no rows and no sub-sections), so it
 * can still be selected and right-clicked. Sections with content have none.
 */
function emptyStyle(contentRows: number): GridStyle | null {
  return contentRows === 0 ? { 'grid-row': '1 / -1', 'grid-column': '1 / -1' } : null;
}

function layoutCells(section: TemplateSection): CellLayout[] {
  return section.rows.flatMap((row, index) =>
    row.cells.map((cell) => ({
      cell,
      rowId: row.id,
      style: {
        'grid-row': `${index + 1} / span ${cell.rowSpan}`,
        'grid-column': `${cell.column} / span ${cell.columnSpan}`,
      },
    })),
  );
}

/** The rows and columns a section's sub-sections take together, laid out in `flow`. */
function childExtent(section: TemplateSection, flow: Flow): Size {
  if (section.sections.length === 0) {
    return { rows: 0, columns: 0 };
  }
  const sizes = section.sections.map((child) => sectionSize(child, flow));
  return {
    rows: combine(
      sizes.map((size) => size.rows),
      flow === 'down',
    ),
    columns: combine(
      sizes.map((size) => size.columns),
      flow === 'across',
    ),
  };
}

/**
 * The rows and columns a section needs: its own rows on top, its sub-sections below them in `flow`, and
 * a row for the placeholder of a section with nothing in it yet.
 */
function sectionSize(section: TemplateSection, flow: Flow): Size {
  const ownRows = rowExtent(section);
  const children = childExtent(section, flow);
  const content = ownRows + children.rows;

  return {
    rows: Math.max(1, content),
    columns: Math.max(1, columnExtent(section), children.columns),
  };
}

/** Sections placed one after another add up; sections alongside each other take the largest. */
function combine(values: number[], inSequence: boolean): number {
  return inSequence ? Math.max(1, sum(values)) : Math.max(1, ...values);
}

/** The last column any cell in the section's own rows reaches. */
function columnExtent(section: TemplateSection): number {
  return Math.max(
    0,
    ...section.rows.flatMap((row) => row.cells.map((cell) => cell.column + cell.columnSpan - 1)),
  );
}

/** The last row any cell in the section's own rows reaches, counting row spans. */
function rowExtent(section: TemplateSection): number {
  const spans = section.rows.flatMap((row, index) => row.cells.map((cell) => index + cell.rowSpan));
  return Math.max(section.rows.length, ...spans);
}

function sum(values: number[]): number {
  return values.reduce((total, value) => total + value, 0);
}
