import { CellLayout } from './models/cell-layout.model';
import { GridStyle } from './models/grid-style';
import { SectionLayout } from './models/section-layout.model';
import { TableTemplate } from './models/table-template.model';
import { TemplateLayout } from './models/template-layout.model';
import { TemplateSection } from './models/template-section.model';

/** Which way a set of sections is laid out next to each other. */
type Flow = 'down' | 'across';

/**
 * Lays a template out as one CSS grid with every section as a nested subgrid. Sections draw nothing
 * themselves; only their rows and cells show. Rows always run across the page and cells always sit
 * side by side in them; orientation only decides where sections go.
 *
 * Horizontal: the header comes first and the other top-level sections stack below it, all on the same
 * columns so data lines up under the header. Child sections inside a section sit side by side.
 *
 * Vertical: the header is on the left and the other top-level sections sit to its right, all on the
 * same rows so data lines up beside the header. Child sections inside a section stack.
 */
export function layoutTemplate(template: TableTemplate): TemplateLayout {
  const topFlow: Flow = template.orientation === 'Horizontal' ? 'down' : 'across';
  const childFlow: Flow = topFlow === 'down' ? 'across' : 'down';
  const sections = topLevelOrder(template.sections);

  const size = (section: TemplateSection): Size => sectionSize(section, childFlow);
  const rows = combine(
    sections.map((section) => size(section).rows),
    topFlow === 'down',
  );
  const columns = combine(
    sections.map((section) => size(section).columns),
    topFlow === 'across',
  );

  return {
    style: {
      'grid-template-rows': `repeat(${rows}, minmax(30px, auto))`,
      'grid-template-columns': `repeat(${columns}, minmax(88px, 1fr))`,
    },
    sections: layoutSiblings(sections, topFlow, childFlow),
  };
}

/** The header (the top-level fixed section) first, then the rest in list order. */
export function topLevelOrder(sections: TemplateSection[]): TemplateSection[] {
  return [
    ...sections.filter((section) => section.role === 'Fixed'),
    ...sections.filter((section) => section.role !== 'Fixed'),
  ];
}

interface Size {
  rows: number;
  columns: number;
}

/** Places sections one after another in `flow`, each filling the parent the other way. */
function layoutSiblings(sections: TemplateSection[], flow: Flow, childFlow: Flow): SectionLayout[] {
  let start = 1;
  return sections.map((section) => {
    const size = sectionSize(section, childFlow);
    const span = flow === 'down' ? size.rows : size.columns;
    const style: GridStyle =
      flow === 'down'
        ? { 'grid-row': `${start} / span ${span}`, 'grid-column': '1 / -1' }
        : { 'grid-column': `${start} / span ${span}`, 'grid-row': '1 / -1' };
    start += span;
    return layoutSection(section, style, childFlow);
  });
}

function layoutSection(section: TemplateSection, style: GridStyle, childFlow: Flow): SectionLayout {
  const isLeaf = section.sections.length === 0;
  const cells = isLeaf ? layoutCells(section) : [];

  return {
    section,
    isLeaf,
    style,
    emptyStyle:
      isLeaf && cells.length === 0 ? { 'grid-row': '1 / -1', 'grid-column': '1 / -1' } : null,
    children: layoutSiblings(section.sections, childFlow, childFlow),
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

/** The rows and columns a section needs, with its child sections laid out in `childFlow`. */
function sectionSize(section: TemplateSection, childFlow: Flow): Size {
  if (section.sections.length === 0) {
    return { rows: Math.max(1, rowExtent(section)), columns: Math.max(1, columnExtent(section)) };
  }
  const sizes = section.sections.map((child) => sectionSize(child, childFlow));
  return {
    rows: combine(
      sizes.map((size) => size.rows),
      childFlow === 'down',
    ),
    columns: combine(
      sizes.map((size) => size.columns),
      childFlow === 'across',
    ),
  };
}

/** Sections placed one after another add up; sections alongside each other take the largest. */
function combine(values: number[], inSequence: boolean): number {
  return inSequence ? Math.max(1, sum(values)) : Math.max(1, ...values);
}

/** The last column any cell in the section reaches. */
function columnExtent(section: TemplateSection): number {
  return Math.max(
    0,
    ...section.rows.flatMap((row) => row.cells.map((cell) => cell.column + cell.columnSpan - 1)),
  );
}

/** The last row any cell in the section reaches, counting row spans. */
function rowExtent(section: TemplateSection): number {
  const spans = section.rows.flatMap((row, index) => row.cells.map((cell) => index + cell.rowSpan));
  return Math.max(section.rows.length, ...spans);
}

function sum(values: number[]): number {
  return values.reduce((total, value) => total + value, 0);
}
