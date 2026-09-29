import { CellLayout } from './models/cell-layout.model';
import { GridStyle } from './models/grid-style';
import { SectionLayout } from './models/section-layout.model';
import { TableTemplate } from './models/table-template.model';
import { TemplateLayout } from './models/template-layout.model';
import { TemplateSection } from './models/template-section.model';

/**
 * Lays a template out as one CSS grid with every section as a nested subgrid. Sections draw nothing
 * themselves; only their rows and cells show, and the subgrids keep cells lined up across sections.
 *
 * Horizontal: the header section comes first, then each top-level section stacks below it, all on the
 * same columns so data lines up under the header. Inside a section, child sections sit side by side and
 * share the section's rows.
 *
 * Vertical is the same layout transposed: a template row runs down a grid column, the header is on the
 * left and top-level sections follow to the right.
 */
export function layoutTemplate(template: TableTemplate): TemplateLayout {
  const horizontal = template.orientation === 'Horizontal';
  const sections = topLevelOrder(template.sections);

  const acrossTracks = Math.max(1, ...sections.map(acrossExtent));
  const alongTracks = Math.max(1, sum(sections.map(alongExtent)));

  // "Along" is the direction template rows run in; "across" is the direction of a row's cells.
  const along = `repeat(${alongTracks}, minmax(30px, auto))`;
  const across = `repeat(${acrossTracks}, minmax(88px, 1fr))`;
  const style: GridStyle = horizontal
    ? { 'grid-template-rows': along, 'grid-template-columns': across }
    : {
        'grid-template-columns': `repeat(${alongTracks}, minmax(88px, 1fr))`,
        'grid-template-rows': `repeat(${acrossTracks}, minmax(30px, auto))`,
      };

  let start = 1;
  const layouts = sections.map((section) => {
    const layout = layoutSection(
      section,
      place(horizontal, `${start} / span ${alongExtent(section)}`, '1 / -1'),
      horizontal,
    );
    start += alongExtent(section);
    return layout;
  });

  return { style, sections: layouts };
}

/** The header (the top-level fixed section) first, then the rest in list order. */
export function topLevelOrder(sections: TemplateSection[]): TemplateSection[] {
  return [
    ...sections.filter((section) => section.role === 'Fixed'),
    ...sections.filter((section) => section.role !== 'Fixed'),
  ];
}

function layoutSection(
  section: TemplateSection,
  style: GridStyle,
  horizontal: boolean,
): SectionLayout {
  const isLeaf = section.sections.length === 0;
  const cells = isLeaf ? layoutCells(section, horizontal) : [];

  let start = 1;
  const children = section.sections.map((child) => {
    const layout = layoutSection(
      child,
      place(horizontal, '1 / -1', `${start} / span ${acrossExtent(child)}`),
      horizontal,
    );
    start += acrossExtent(child);
    return layout;
  });

  return {
    section,
    isLeaf,
    style,
    emptyStyle:
      isLeaf && cells.length === 0 ? { 'grid-row': '1 / -1', 'grid-column': '1 / -1' } : null,
    children,
    cells,
  };
}

function layoutCells(section: TemplateSection, horizontal: boolean): CellLayout[] {
  return section.rows.flatMap((row, index) =>
    row.cells.map((cell) => ({
      cell,
      rowId: row.id,
      style: place(
        horizontal,
        `${index + 1} / span ${cell.rowSpan}`,
        `${cell.column} / span ${cell.columnSpan}`,
      ),
    })),
  );
}

/** Grid placement from a position along the rows' direction and one across it. */
function place(horizontal: boolean, along: string, across: string): GridStyle {
  return horizontal
    ? { 'grid-row': along, 'grid-column': across }
    : { 'grid-column': along, 'grid-row': across };
}

/** How many tracks a section takes in the direction its rows run: its tallest child, or its rows. */
function alongExtent(section: TemplateSection): number {
  if (section.sections.length > 0) {
    return Math.max(1, ...section.sections.map(alongExtent));
  }
  return Math.max(1, rowExtent(section));
}

/** How many tracks a section takes across its rows: its children side by side, or its cells. */
function acrossExtent(section: TemplateSection): number {
  if (section.sections.length > 0) {
    return sum(section.sections.map(acrossExtent));
  }
  return Math.max(1, columnExtent(section));
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
