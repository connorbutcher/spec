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
  };
}

function leaf(name: string, rows: TemplateCell[][]): TemplateSection {
  return {
    id: nextId++,
    parentSectionId: null,
    name,
    displayOrder: 1,
    sections: [],
    rows: rows.map((cells, index) => ({ id: nextId++, displayOrder: index + 1, cells })),
  };
}

function parent(name: string, sections: TemplateSection[]): TemplateSection {
  return { id: nextId++, parentSectionId: null, name, displayOrder: 1, sections, rows: [] };
}

function template(
  orientation: 'Horizontal' | 'Vertical',
  sections: TemplateSection[],
): TableTemplate {
  return { id: 1, sheetTypeId: 1, name: 'T', displayOrder: 1, orientation, sections };
}

describe('layoutTemplate', () => {
  it('lays horizontal sections side by side with shared data rows under the headers', () => {
    const a = leaf('A', [[cell(1), cell(2)], [cell(1, 2)]]);
    const b = parent('B', [leaf('B1', [[cell(1)]]), leaf('B2', [[cell(1)], [cell(1)], [cell(1)]])]);

    const layout = layoutTemplate(template('Horizontal', [a, b]));

    // A is 2 columns wide, B is 1 + 1; two header levels, then the tallest leaf's 3 rows.
    expect(layout.style['grid-template-columns']).toBe('repeat(4, minmax(88px, 1fr))');
    expect(layout.style['grid-template-rows']).toBe(
      'repeat(2, auto) repeat(3, minmax(30px, auto))',
    );

    const [aLayout, bLayout] = layout.sections;
    expect(aLayout.style).toEqual({ 'grid-column': '1 / span 2', 'grid-row': '1 / -1' });
    // A is a leaf at depth 1, so its header fills both header rows and its data starts at row 3.
    expect(aLayout.headerStyle['grid-row']).toBe('1 / span 2');
    expect(aLayout.cells[0].style).toEqual({
      'grid-row': '3 / span 1',
      'grid-column': '1 / span 1',
    });
    expect(aLayout.cells[2].style).toEqual({
      'grid-row': '4 / span 1',
      'grid-column': '1 / span 2',
    });

    expect(bLayout.style['grid-column']).toBe('3 / span 2');
    const [b1, b2] = bLayout.children;
    expect(b1.style).toEqual({ 'grid-column': '1 / span 1', 'grid-row': '2 / -1' });
    expect(b2.style['grid-column']).toBe('2 / span 1');
    // B1 is at depth 2: one header row of its own, then data from its local row 2 (global row 3).
    expect(b1.headerStyle['grid-row']).toBe('1 / span 1');
    expect(b1.cells[0].style['grid-row']).toBe('2 / span 1');
  });

  it('transposes for vertical tables: sections stack and share data columns', () => {
    const a = leaf('A', [[cell(1), cell(2, 2)]]);
    const b = leaf('B', [[cell(1)], [cell(1, 1, 2)]]);

    const layout = layoutTemplate(template('Vertical', [a, b]));

    expect(layout.style['grid-template-columns']).toBe(
      'repeat(1, minmax(88px, max-content)) repeat(3, minmax(88px, 1fr))',
    );
    // A takes 1 row; B takes 3 (its second row's cell spans down one more).
    expect(layout.style['grid-template-rows']).toBe('repeat(4, minmax(30px, auto))');
    expect(layout.sections[1].style).toEqual({ 'grid-row': '2 / span 3', 'grid-column': '1 / -1' });
    expect(layout.sections[0].cells[1].style).toEqual({
      'grid-row': '1 / span 1',
      'grid-column': '3 / span 2',
    });
  });

  it('gives an empty leaf a filler in place of its data', () => {
    const layout = layoutTemplate(template('Horizontal', [leaf('Empty', [])]));

    expect(layout.sections[0].cells).toEqual([]);
    expect(layout.sections[0].emptyStyle).toEqual({
      'grid-row': '2 / -1',
      'grid-column': '1 / -1',
    });
  });
});
