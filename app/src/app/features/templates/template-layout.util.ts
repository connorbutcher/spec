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

export class TemplateLayoutUtil {
  /**
   * Lays a template out as one CSS grid with every section as a nested subgrid. Sections draw nothing
   * themselves; only their rows and cells show. Rows always run across the page and cells sit side by
   * side in them.
   *
   * Sections stack top to bottom in both orientations: the header, then the addable sections, each with
   * its own rows first and its sub-sections below them. A horizontal table's column blocks take the
   * columns after the rows' own cells, one copy of each, running through every row.
   */
  public static layoutTemplate(template: TableTemplate): TemplateLayout {
    const plan = TemplateLayoutUtil.planColumns(template);
    const sections = TemplateLayoutUtil.topLevelOrder(template.sections);
    const rows = Math.max(1, TemplateLayoutUtil.sum(sections.map(TemplateLayoutUtil.sectionRows)));

    return {
      style: {
        'grid-template-rows': `repeat(${rows}, minmax(30px, auto))`,
        'grid-template-columns': `repeat(${plan.total}, minmax(112px, 1fr))`,
      },
      sections: TemplateLayoutUtil.layoutSiblings(sections, 0, plan),
      columnBlocks: template.columnBlocks.map((block) => TemplateLayoutUtil.layoutColumnBlock(block, plan)),
    };
  }

  /** The header first, then the addable sections in list order. */
  public static topLevelOrder(sections: TemplateSection[]): TemplateSection[] {
    return [
      ...sections.filter((section) => section.role === 'Header'),
      ...sections.filter((section) => section.role !== 'Header'),
    ];
  }

  /**
   * Shares the columns out: the rows' own cells take the first columns, then each column block takes as
   * many as its widest row needs (at least one), left to right in block order.
   */
  public static planColumns(template: TableTemplate): ColumnPlan {
    const cells = TemplateLayoutUtil.allSections(template.sections).flatMap((section) =>
      section.rows.flatMap((row) => row.cells),
    );
    const ownWidth = TemplateLayoutUtil.extent(cells.filter((cell) => cell.columnBlockId === null));

    const blockStarts = new Map<number, number>();
    const blockWidths = new Map<number, number>();
    let next = ownWidth + 1;
    for (const block of [...template.columnBlocks].sort((a, b) => a.displayOrder - b.displayOrder)) {
      const width = Math.max(1, TemplateLayoutUtil.extent(cells.filter((cell) => cell.columnBlockId === block.id)));
      blockStarts.set(block.id, next);
      blockWidths.set(block.id, width);
      next += width;
    }

    return { total: Math.max(1, next - 1), blockStarts, blockWidths };
  }

  /** The table column a cell starts on: its own column, moved along to its block's columns if it has one. */
  public static tableColumn(cell: TemplateCell, plan: ColumnPlan): number {
    return cell.columnBlockId === null
      ? cell.column
      : (plan.blockStarts.get(cell.columnBlockId) ?? 1) + cell.column - 1;
  }

  /** Places sections one under another, below the `rowOffset` rows their parent's own rows take. */
  private static layoutSiblings(sections: TemplateSection[], rowOffset: number, plan: ColumnPlan): SectionLayout[] {
    let start = 1;
    return sections.map((section) => {
      const rows = TemplateLayoutUtil.sectionRows(section);
      const style: GridStyle = {
        'grid-row': `${rowOffset + start} / span ${rows}`,
        'grid-column': '1 / -1',
      };
      start += rows;
      return TemplateLayoutUtil.layoutSection(section, style, plan);
    });
  }

  private static layoutSection(section: TemplateSection, style: GridStyle, plan: ColumnPlan): SectionLayout {
    const ownRows = TemplateLayoutUtil.rowExtent(section);
    const childRows = TemplateLayoutUtil.sum(section.sections.map(TemplateLayoutUtil.sectionRows));

    return {
      section,
      isLeaf: section.sections.length === 0,
      style,
      emptyStyle: TemplateLayoutUtil.emptyStyle(ownRows + childRows),
      children: TemplateLayoutUtil.layoutSiblings(section.sections, ownRows, plan),
      cells: TemplateLayoutUtil.layoutCells(section, plan),
    };
  }

  private static layoutColumnBlock(block: TemplateColumnBlock, plan: ColumnPlan): ColumnBlockLayout {
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
  private static emptyStyle(contentRows: number): GridStyle | null {
    return contentRows === 0 ? { 'grid-row': '1 / -1', 'grid-column': '1 / -1' } : null;
  }

  private static layoutCells(section: TemplateSection, plan: ColumnPlan): CellLayout[] {
    return section.rows.flatMap((row, index) =>
      row.cells.map((cell) => ({
        cell,
        rowId: row.id,
        style: {
          'grid-row': `${index + 1} / span ${cell.rowSpan}`,
          'grid-column': `${TemplateLayoutUtil.tableColumn(cell, plan)} / span ${cell.columnSpan}`,
        },
      })),
    );
  }

  /** The rows a section needs: its own rows, its sub-sections below them, or one for its placeholder. */
  private static sectionRows(section: TemplateSection): number {
    return Math.max(1, TemplateLayoutUtil.rowExtent(section) + TemplateLayoutUtil.sum(section.sections.map(TemplateLayoutUtil.sectionRows)));
  }

  /** The last column any of `cells` reaches, counted from the first column of the cells' block. */
  private static extent(cells: TemplateCell[]): number {
    return Math.max(0, ...cells.map((cell) => cell.column + cell.columnSpan - 1));
  }

  /** The last row any cell in the section's own rows reaches, counting row spans. */
  private static rowExtent(section: TemplateSection): number {
    const spans = section.rows.flatMap((row, index) => row.cells.map((cell) => index + cell.rowSpan));
    return Math.max(section.rows.length, ...spans);
  }

  private static allSections(sections: TemplateSection[]): TemplateSection[] {
    return sections.flatMap((section) => [section, ...TemplateLayoutUtil.allSections(section.sections)]);
  }

  private static sum(values: number[]): number {
    return values.reduce((total, value) => total + value, 0);
  }
}
