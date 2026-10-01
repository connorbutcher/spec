import { CellLayout } from './models/cell-layout.model';
import { ColumnBlockLayout } from './models/column-block-layout.model';
import { ColumnPlan } from './models/column-plan.model';
import { GridStyle } from './models/grid-style';
import { SectionLayout } from './models/section-layout.model';
import { TableTemplate } from './models/table-template.model';
import { TemplateCell } from './models/template-cell.model';
import { TemplateColumnBlock } from './models/template-column-block.model';
import { TemplateLayout } from './models/template-layout.model';
import { TemplateSection } from './models/template-section.model';

/**
 * Lays a template out as one CSS grid with every section as a nested subgrid. Sections draw nothing
 * themselves; only their rows and cells show. Rows always run across the page and cells sit side by
 * side in them.
 *
 * Sections stack top to bottom in both orientations: the header, then the addable sections, each with
 * its own rows first and its sub-sections below them. A horizontal table's column blocks take the
 * columns after the rows' own cells, one copy of each, running through every row.
 */
export function layoutTemplate(template: TableTemplate): TemplateLayout {
  const plan = planColumns(template);
  const sections = topLevelOrder(template.sections);
  const rows = Math.max(1, sum(sections.map(sectionRows)));

  return {
    style: {
      'grid-template-rows': `repeat(${rows}, minmax(30px, auto))`,
      'grid-template-columns': `repeat(${plan.total}, minmax(112px, 1fr))`,
    },
    sections: layoutSiblings(sections, 0, plan),
    columnBlocks: template.columnBlocks.map((block) => layoutColumnBlock(block, plan)),
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
 * Shares the columns out: the rows' own cells take the first columns, then each column block takes as
 * many as its widest row needs (at least one), left to right in block order.
 */
export function planColumns(template: TableTemplate): ColumnPlan {
  const cells = allSections(template.sections).flatMap((section) =>
    section.rows.flatMap((row) => row.cells),
  );
  const ownWidth = extent(cells.filter((cell) => cell.columnBlockId === null));

  const blockStarts = new Map<number, number>();
  const blockWidths = new Map<number, number>();
  let next = ownWidth + 1;
  for (const block of [...template.columnBlocks].sort((a, b) => a.displayOrder - b.displayOrder)) {
    const width = Math.max(1, extent(cells.filter((cell) => cell.columnBlockId === block.id)));
    blockStarts.set(block.id, next);
    blockWidths.set(block.id, width);
    next += width;
  }

  return { total: Math.max(1, next - 1), blockStarts, blockWidths };
}

/** The table column a cell starts on: its own column, moved along to its block's columns if it has one. */
export function tableColumn(cell: TemplateCell, plan: ColumnPlan): number {
  return cell.columnBlockId === null
    ? cell.column
    : (plan.blockStarts.get(cell.columnBlockId) ?? 1) + cell.column - 1;
}

/** Places sections one under another, below the `rowOffset` rows their parent's own rows take. */
function layoutSiblings(
  sections: TemplateSection[],
  rowOffset: number,
  plan: ColumnPlan,
): SectionLayout[] {
  let start = 1;
  return sections.map((section) => {
    const rows = sectionRows(section);
    const style: GridStyle = {
      'grid-row': `${rowOffset + start} / span ${rows}`,
      'grid-column': '1 / -1',
    };
    start += rows;
    return layoutSection(section, style, plan);
  });
}

function layoutSection(
  section: TemplateSection,
  style: GridStyle,
  plan: ColumnPlan,
): SectionLayout {
  const ownRows = rowExtent(section);
  const childRows = sum(section.sections.map(sectionRows));

  return {
    section,
    isLeaf: section.sections.length === 0,
    style,
    emptyStyle: emptyStyle(ownRows + childRows),
    children: layoutSiblings(section.sections, ownRows, plan),
    cells: layoutCells(section, plan),
  };
}

function layoutColumnBlock(block: TemplateColumnBlock, plan: ColumnPlan): ColumnBlockLayout {
  return {
    block,
    style: {
      'grid-row': '1 / -1',
      'grid-column': `${plan.blockStarts.get(block.id)} / span ${plan.blockWidths.get(block.id)}`,
    },
  };
}

/**
 * Where the placeholder sits for a section with nothing in it yet (no rows and no sub-sections), so it
 * can still be selected and right-clicked. Sections with content have none.
 */
function emptyStyle(contentRows: number): GridStyle | null {
  return contentRows === 0 ? { 'grid-row': '1 / -1', 'grid-column': '1 / -1' } : null;
}

function layoutCells(section: TemplateSection, plan: ColumnPlan): CellLayout[] {
  return section.rows.flatMap((row, index) =>
    row.cells.map((cell) => ({
      cell,
      rowId: row.id,
      style: {
        'grid-row': `${index + 1} / span ${cell.rowSpan}`,
        'grid-column': `${tableColumn(cell, plan)} / span ${cell.columnSpan}`,
      },
    })),
  );
}

/** The rows a section needs: its own rows, its sub-sections below them, or one for its placeholder. */
function sectionRows(section: TemplateSection): number {
  return Math.max(1, rowExtent(section) + sum(section.sections.map(sectionRows)));
}

/** The last column any of `cells` reaches, counted from the first column of the cells' block. */
function extent(cells: TemplateCell[]): number {
  return Math.max(0, ...cells.map((cell) => cell.column + cell.columnSpan - 1));
}

/** The last row any cell in the section's own rows reaches, counting row spans. */
function rowExtent(section: TemplateSection): number {
  const spans = section.rows.flatMap((row, index) => row.cells.map((cell) => index + cell.rowSpan));
  return Math.max(section.rows.length, ...spans);
}

function allSections(sections: TemplateSection[]): TemplateSection[] {
  return sections.flatMap((section) => [section, ...allSections(section.sections)]);
}

function sum(values: number[]): number {
  return values.reduce((total, value) => total + value, 0);
}
