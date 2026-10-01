import { SectionRole } from './models/section-role';
import { TableTemplate } from './models/table-template.model';
import { TemplateCell } from './models/template-cell.model';
import { TemplateSection } from './models/template-section.model';
import { layoutTemplate } from './template-layout.util';

let nextId = 1;

function cell(column: number, columnSpan = 1, rowSpan = 1): TemplateCell {
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
  };
}

function section(
  name: string,
  role: SectionRole,
  sections: TemplateSection[],
  rows: TemplateCell[][],
): TemplateSection {
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

function template(
  orientation: 'Horizontal' | 'Vertical',
  sections: TemplateSection[],
): TableTemplate {
  return {
    id: 1,
    sheetTypeId: 1,
    name: 'T',
    displayOrder: 1,
    versionId: 1,
    versionNumber: 1,
    orientation,
    isEditable: true,
    versions: [{ id: 1, versionNumber: 1, createdAtUtc: '2026-09-29T00:00:00Z', isInUse: false }],
    sections,
  };
}

describe('layoutTemplate', () => {
  it('stacks a vertical table top to bottom: header first, then each section below it', () => {
    const data = section('Data', 'Addable', [], [[cell(1), cell(2, 2)], [cell(1)]]);
    const header = section('Header', 'Header', [], [[cell(1), cell(2), cell(3)]]);

    const layout = layoutTemplate(template('Vertical', [data, header]));

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

  it('runs a horizontal table left to right: header first, then each section beside it', () => {
    const header = section('Header', 'Header', [], [[cell(1), cell(2), cell(3)]]);
    const data = section('Data', 'Addable', [], [[cell(1)], [cell(1)]]);

    const layout = layoutTemplate(template('Horizontal', [header, data]));

    expect(layout.style).toEqual({
      'grid-template-rows': 'repeat(2, minmax(30px, auto))',
      'grid-template-columns': 'repeat(4, minmax(112px, 1fr))',
    });
    expect(layout.sections[0].style).toEqual({
      'grid-column': '1 / span 3',
      'grid-row': '1 / span 2',
    });
    expect(layout.sections[1].style).toEqual({
      'grid-column': '4 / span 1',
      'grid-row': '1 / span 2',
    });
  });

  it('stacks a section own rows and sub-sections down a vertical table', () => {
    const left = section('Left', 'Addable', [], [[cell(1)], [cell(1)]]);
    const right = section('Right', 'Addable', [], [[cell(1), cell(2)]]);
    const group = section('Group', 'Addable', [left, right], [[cell(1, 3)]]);

    const layout = layoutTemplate(template('Vertical', [group]));

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

  it('runs a section sub-sections side by side in a horizontal table, beneath its own rows', () => {
    const left = section('Left', 'Addable', [], [[cell(1)], [cell(1)]]);
    const right = section('Right', 'Addable', [], [[cell(1), cell(2)]]);
    const group = section('Group', 'Addable', [left, right], [[cell(1, 3)]]);

    const layout = layoutTemplate(template('Horizontal', [group]));

    expect(layout.style['grid-template-rows']).toBe('repeat(3, minmax(30px, auto))');
    expect(layout.style['grid-template-columns']).toBe('repeat(3, minmax(112px, 1fr))');
    const [groupLayout] = layout.sections;
    expect(groupLayout.cells[0].style).toEqual({
      'grid-row': '1 / span 1',
      'grid-column': '1 / span 3',
    });
    expect(groupLayout.children[0].style).toEqual({
      'grid-column': '1 / span 1',
      'grid-row': '2 / span 2',
    });
    expect(groupLayout.children[1].style).toEqual({
      'grid-column': '2 / span 2',
      'grid-row': '2 / span 2',
    });
  });

  it('gives an empty section a placeholder so it can still be selected', () => {
    const layout = layoutTemplate(template('Vertical', [section('Header', 'Header', [], [])]));

    expect(layout.style['grid-template-rows']).toBe('repeat(1, minmax(30px, auto))');
    expect(layout.sections[0].emptyStyle).toEqual({
      'grid-row': '1 / -1',
      'grid-column': '1 / -1',
    });
  });
});
