import { SectionRole } from './models/section-role';
import { TableTemplate } from './models/table-template.model';
import { TemplateCell } from './models/template-cell.model';
import { TemplateColumnBlock } from './models/template-column-block.model';
import { TemplateSection } from './models/template-section.model';
import { TemplateLayoutUtil } from './template-layout.util';

class SpecHelpers {
  public static cell(column: number, columnSpan = 1, rowSpan = 1, columnBlockId: number | null = null): TemplateCell {
    return {
      id: nextId++,
      cellTypeId: 2,
      column,
      columnSpan,
      rowSpan,
      caption: null,
      isRequired: false,
      configurationOverride: null,
      styleOverride: null,
      columnBlockId,
    };
  }

  public static section(name: string, role: SectionRole, sections: TemplateSection[], rows: TemplateCell[][]): TemplateSection {
    return {
      id: nextId++,
      parentSectionId: null,
      name,
      displayOrder: 1,
      role,
      minInstances: role === 'Header' ? 1 : 0,
      maxInstances: role === 'Header' ? 1 : null,
      initialInstances: role === 'Header' ? 1 : 0,
      sections,
      rows: rows.map((cells, index) => ({ id: nextId++, displayOrder: index + 1, cells })),
    };
  }

  public static block(id: number, name: string, displayOrder: number): TemplateColumnBlock {
    return {
      id,
      name,
      displayOrder,
      minInstances: 0,
      maxInstances: null,
      initialInstances: 1,
      stickyColumnCount: 0,
    };
  }

  public static template(orientation: 'Horizontal' | 'Vertical', sections: TemplateSection[], columnBlocks: TemplateColumnBlock[] = []): TableTemplate {
    return {
      id: 1,
      sheetTypeId: 1,
      name: 'T',
      displayOrder: 1,
      versionId: 1,
      versionNumber: 1,
      orientation,
      stickyColumnCount: 0,
      isEditable: true,
      versions: [{ id: 1, versionNumber: 1, createdAtUtc: '2026-09-29T00:00:00Z', isInUse: false }],
      sections,
      columnBlocks,
    };
  }
}

let nextId = 1;

const PART = 900;
const NOTE = 901;
describe('layoutTemplate', () => {
  it('stacks a vertical table top to bottom: header first, then each section below it', () => {
    const data = SpecHelpers.section('Data', 'Addable', [], [[SpecHelpers.cell(1), SpecHelpers.cell(2, 2)], [SpecHelpers.cell(1)]]);
    const header = SpecHelpers.section('Header', 'Header', [], [[SpecHelpers.cell(1), SpecHelpers.cell(2), SpecHelpers.cell(3)]]);

    const layout = TemplateLayoutUtil.layoutTemplate(SpecHelpers.template('Vertical', [data, header]));

    expect(layout.style).toEqual({
      'grid-template-rows': 'repeat(3, minmax(30px, auto))',
      'grid-template-columns': 'repeat(3, minmax(112px, 1fr))',
    });
    expect(layout.sections.map((entry) => entry.section.name)).toEqual(['Header', 'Data']);
    expect(layout.sections[0].style).toEqual({ 'grid-row': '1 / span 1', 'grid-column': '1 / -1' });
    expect(layout.sections[1].style).toEqual({ 'grid-row': '2 / span 2', 'grid-column': '1 / -1' });
    expect(layout.sections[0].emptyStyle).toBeNull();
    // A cell's row and column are the same in both orientations.
    expect(layout.sections[1].cells[1].style).toEqual({
      'grid-row': '1 / span 1',
      'grid-column': '2 / span 2',
    });
  });

  it('stacks a horizontal table down too, with each column block after the rows own cells', () => {
    const header = SpecHelpers.section(
      'Header',
      'Header',
      [],
      [[SpecHelpers.cell(1), SpecHelpers.cell(1, 1, 1, PART), SpecHelpers.cell(2, 1, 1, PART), SpecHelpers.cell(1, 1, 1, NOTE)]],
    );
    const data = SpecHelpers.section(
      'Data',
      'Addable',
      [],
      [[SpecHelpers.cell(1, 2), SpecHelpers.cell(1, 2, 1, PART), SpecHelpers.cell(1, 1, 1, NOTE)]],
    );

    const layout = TemplateLayoutUtil.layoutTemplate(
      SpecHelpers.template('Horizontal', [header, data], [SpecHelpers.block(PART, 'Part', 1), SpecHelpers.block(NOTE, 'Note', 2)]),
    );

    // Own cells take 2 columns, the Part block 2, the Note block 1.
    expect(layout.style).toEqual({
      'grid-template-rows': 'repeat(2, minmax(30px, auto))',
      'grid-template-columns': 'repeat(5, minmax(112px, 1fr))',
    });
    expect(layout.sections[1].style).toEqual({ 'grid-row': '2 / span 1', 'grid-column': '1 / -1' });
    expect(layout.sections[0].cells.map((entry) => entry.style['grid-column'])).toEqual([
      '1 / span 1',
      '3 / span 1',
      '4 / span 1',
      '5 / span 1',
    ]);
    expect(layout.sections[1].cells[1].style['grid-column']).toBe('3 / span 2');
    expect(layout.columnBlocks.map((entry) => entry.style)).toEqual([
      { 'grid-row': '1 / -1', 'grid-column': '3 / span 2' },
      { 'grid-row': '1 / -1', 'grid-column': '5 / span 1' },
    ]);
  });

  it('gives a column block with no cells yet one column', () => {
    const header = SpecHelpers.section('Header', 'Header', [], [[SpecHelpers.cell(1)]]);

    const layout = TemplateLayoutUtil.layoutTemplate(SpecHelpers.template('Horizontal', [header], [SpecHelpers.block(PART, 'Part', 1)]));

    expect(layout.style['grid-template-columns']).toBe('repeat(2, minmax(112px, 1fr))');
    expect(layout.columnBlocks[0].style['grid-column']).toBe('2 / span 1');
  });

  it('stacks a section own rows and sub-sections down a vertical table', () => {
    const left = SpecHelpers.section('Left', 'Addable', [], [[SpecHelpers.cell(1)], [SpecHelpers.cell(1)]]);
    const right = SpecHelpers.section('Right', 'Addable', [], [[SpecHelpers.cell(1), SpecHelpers.cell(2)]]);
    const group = SpecHelpers.section('Group', 'Addable', [left, right], [[SpecHelpers.cell(1, 3)]]);

    const layout = TemplateLayoutUtil.layoutTemplate(SpecHelpers.template('Vertical', [group]));

    // Title row, then left (2 rows) and right (1 row) stacked below it.
    expect(layout.style['grid-template-rows']).toBe('repeat(4, minmax(30px, auto))');
    expect(layout.style['grid-template-columns']).toBe('repeat(3, minmax(112px, 1fr))');
    const [groupLayout] = layout.sections;
    expect(groupLayout.children[0].style).toEqual({
      'grid-row': '2 / span 2',
      'grid-column': '1 / -1',
    });
    expect(groupLayout.children[1].style).toEqual({
      'grid-row': '4 / span 1',
      'grid-column': '1 / -1',
    });
  });

  it('gives an empty section a placeholder so it can still be selected', () => {
    const layout = TemplateLayoutUtil.layoutTemplate(SpecHelpers.template('Vertical', [SpecHelpers.section('Header', 'Header', [], [])]));

    expect(layout.style['grid-template-rows']).toBe('repeat(1, minmax(30px, auto))');
    expect(layout.sections[0].emptyStyle).toEqual({
      'grid-row': '1 / -1',
      'grid-column': '1 / -1',
    });
  });
});
