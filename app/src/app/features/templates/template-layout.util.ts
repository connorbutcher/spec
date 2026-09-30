import { CellLayout } from './models/cell-layout.model';
import { GridStyle } from './models/grid-style';
import { SectionLayout } from './models/section-layout.model';
import { TableTemplate } from './models/table-template.model';
import { TemplateLayout } from './models/template-layout.model';
import { TemplateSection } from './models/template-section.model';

/** Which way a set of sections is laid out next to each other. */
type Flow = 'down' | 'across';

interface Size {
  rows: number;
  columns: number;
}

/**
 * Lays a template out as one CSS grid with every section as a nested subgrid. Sections draw nothing
 * themselves; only their rows and cells show. Rows always run across the page and cells sit side by
 * side in them; orientation only decides where sections go.
 *
 * A section's own rows come first, and its sub-sections follow below them.
 *
 * Horizontal: the header comes first and the addable sections stack below it, all on the same columns
 * so data lines up under the header. Sub-sections inside a section sit side by side.
 *
 * Vertical: the header is on the left and the addable sections sit to its right, all on the same rows
 * so data lines up beside the header. Sub-sections inside a section stack.
 */
export function layoutTemplate(template: TableTemplate): TemplateLayout {
  const topFlow: Flow = template.orientation === 'Horizontal' ? 'down' : 'across';
  const childFlow: Flow = topFlow === 'down' ? 'across' : 'down';
  const sections = topLevelOrder(template.sections);
  const sizes = sections.map((section) => sectionSize(section, childFlow));

  const rows = combine(
    sizes.map((size) => size.rows),
    topFlow === 'down',
  );
  const columns = combine(
    sizes.map((size) => size.columns),
    topFlow === 'across',
  );

  return {
    style: {
      'grid-template-rows': `repeat(${rows}, minmax(30px, auto))`,
      'grid-template-columns': `repeat(${columns}, minmax(88px, 1fr))`,
    },
    sections: layoutSiblings(sections, topFlow, childFlow, 0),
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
 * how many of the parent's rows are taken by its own rows, which sit above its sub-sections.
 */
function layoutSiblings(
  sections: TemplateSection[],
  flow: Flow,
  childFlow: Flow,
  rowOffset: number,
): SectionLayout[] {
  let start = 1;
  return sections.map((section) => {
    const size = sectionSize(section, childFlow);
    const style: GridStyle =
      flow === 'down'
        ? { 'grid-row': `${rowOffset + start} / span ${size.rows}`, 'grid-column': '1 / -1' }
        : { 'grid-column': `${start} / span ${size.columns}`, 'grid-row': `${rowOffset + 1} / -1` };
    start += flow === 'down' ? size.rows : size.columns;
    return layoutSection(section, style, childFlow);
  });
}

function layoutSection(section: TemplateSection, style: GridStyle, childFlow: Flow): SectionLayout {
  const cells = layoutCells(section);
  const isEmpty = cells.length === 0 && section.sections.length === 0;

  return {
    section,
    isLeaf: section.sections.length === 0,
    style,
    emptyStyle: isEmpty ? { 'grid-row': '1 / -1', 'grid-column': '1 / -1' } : null,
    children: layoutSiblings(section.sections, childFlow, childFlow, rowExtent(section)),
    cells,
  };
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

/**
 * The rows and columns a section needs: its own rows on top, with its sub-sections laid out below them
 * in `childFlow`. A section with nothing in it still takes one cell so it can be selected.
 */
function sectionSize(section: TemplateSection, childFlow: Flow): Size {
  const ownRows = rowExtent(section);
  const ownColumns = columnExtent(section);
  const children = section.sections.map((child) => sectionSize(child, childFlow));

  const childRows =
    children.length > 0
      ? combine(
          children.map((size) => size.rows),
          childFlow === 'down',
        )
      : 0;
  const childColumns =
    children.length > 0
      ? combine(
          children.map((size) => size.columns),
          childFlow === 'across',
        )
      : 0;

  return {
    rows: Math.max(1, ownRows + childRows),
    columns: Math.max(1, ownColumns, childColumns),
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
