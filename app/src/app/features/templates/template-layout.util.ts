import { CellLayout } from './models/cell-layout.model';
import { GridStyle } from './models/grid-style';
import { SectionLayout } from './models/section-layout.model';
import { TableTemplate } from './models/table-template.model';
import { TemplateLayout } from './models/template-layout.model';
import { TemplateSection } from './models/template-section.model';

/**
 * Lays a template out as one CSS grid with every section as a nested subgrid, so cells line up across
 * sections.
 *
 * Horizontal: sections sit side by side along the columns. The first `depth` grid rows hold the section
 * headers (a leaf section's header stretches down to the deepest header row), then every leaf's rows
 * share the same data rows.
 *
 * Vertical is the same layout transposed: sections stack down the rows, the first `depth` grid columns
 * hold the headers, and every leaf's columns share the same data columns.
 */
export function layoutTemplate(template: TableTemplate): TemplateLayout {
  const horizontal = template.orientation === 'Horizontal';
  const headerTracks = maxDepth(template.sections);
  const dataTracks = Math.max(
    1,
    ...allLeaves(template.sections).map((leaf) => crossExtent(leaf, horizontal)),
  );
  const flowTracks = Math.max(
    1,
    sum(template.sections.map((section) => flowExtent(section, horizontal))),
  );

  const style: GridStyle = horizontal
    ? {
        'grid-template-columns': `repeat(${flowTracks}, minmax(88px, 1fr))`,
        'grid-template-rows': `repeat(${headerTracks}, auto) repeat(${dataTracks}, minmax(30px, auto))`,
      }
    : {
        'grid-template-columns': `repeat(${headerTracks}, minmax(88px, max-content)) repeat(${dataTracks}, minmax(88px, 1fr))`,
        'grid-template-rows': `repeat(${flowTracks}, minmax(30px, auto))`,
      };

  const context: LayoutContext = { horizontal, headerTracks };
  return { style, sections: layoutSiblings(template.sections, 1, context) };
}

interface LayoutContext {
  horizontal: boolean;
  headerTracks: number;
}

function layoutSiblings(
  sections: TemplateSection[],
  depth: number,
  context: LayoutContext,
): SectionLayout[] {
  let start = 1;
  return sections.map((section) => {
    const layout = layoutSection(section, depth, start, context);
    start += flowExtent(section, context.horizontal);
    return layout;
  });
}

function layoutSection(
  section: TemplateSection,
  depth: number,
  start: number,
  context: LayoutContext,
): SectionLayout {
  const { horizontal, headerTracks } = context;
  const isLeaf = section.sections.length === 0;
  const flow = `${start} / span ${flowExtent(section, horizontal)}`;
  const across = depth === 1 ? '1 / -1' : '2 / -1';

  // Header tracks this section's own header takes; a leaf's reaches down to where the data starts.
  const headerSpan = isLeaf ? headerTracks - depth + 1 : 1;
  const cells = isLeaf ? layoutCells(section, headerSpan, horizontal) : [];

  return {
    section,
    isLeaf,
    style: horizontal
      ? { 'grid-column': flow, 'grid-row': across }
      : { 'grid-row': flow, 'grid-column': across },
    headerStyle: horizontal
      ? { 'grid-column': '1 / -1', 'grid-row': `1 / span ${headerSpan}` }
      : { 'grid-row': '1 / -1', 'grid-column': `1 / span ${headerSpan}` },
    emptyStyle:
      isLeaf && cells.length === 0
        ? horizontal
          ? { 'grid-row': `${headerSpan + 1} / -1`, 'grid-column': '1 / -1' }
          : { 'grid-row': '1 / -1', 'grid-column': `${headerSpan + 1} / -1` }
        : null,
    children: layoutSiblings(section.sections, depth + 1, context),
    cells,
  };
}

function layoutCells(section: TemplateSection, offset: number, horizontal: boolean): CellLayout[] {
  return section.rows.flatMap((row, index) =>
    row.cells.map((cell) => {
      const rowTrack = `${(horizontal ? offset : 0) + index + 1} / span ${cell.rowSpan}`;
      const columnTrack = `${(horizontal ? 0 : offset) + cell.column} / span ${cell.columnSpan}`;
      return { cell, rowId: row.id, style: { 'grid-row': rowTrack, 'grid-column': columnTrack } };
    }),
  );
}

/** How many tracks a section takes along the direction sections flow. */
function flowExtent(section: TemplateSection, horizontal: boolean): number {
  if (section.sections.length > 0) {
    return sum(section.sections.map((child) => flowExtent(child, horizontal)));
  }
  return Math.max(1, horizontal ? columnExtent(section) : rowExtent(section));
}

/** How many data tracks a leaf section needs across the flow. */
function crossExtent(leaf: TemplateSection, horizontal: boolean): number {
  return horizontal ? rowExtent(leaf) : columnExtent(leaf);
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

function maxDepth(sections: TemplateSection[]): number {
  return sections.length === 0
    ? 0
    : 1 + Math.max(...sections.map((section) => maxDepth(section.sections)));
}

function allLeaves(sections: TemplateSection[]): TemplateSection[] {
  return sections.flatMap((section) =>
    section.sections.length === 0 ? [section] : allLeaves(section.sections),
  );
}

function sum(values: number[]): number {
  return values.reduce((total, value) => total + value, 0);
}
