import { layoutSheetTable } from './sheet-layout.util';
import { fixtureTable } from './sheet-structure.fixture';

describe('layoutSheetTable', () => {
  it('puts the header first and the group after it, whatever order they arrive in', () => {
    const layout = layoutSheetTable(fixtureTable());
    expect(layout.sections.map((section) => section.section.name)).toEqual(['Header', 'Group']);
  });

  it('stacks sections top to bottom in a vertical table, on five shared columns', () => {
    const layout = layoutSheetTable(fixtureTable());
    expect(layout.style['grid-template-columns']).toContain('repeat(5,');
    // Header: 1 row; group: its own row, then the limits row.
    expect(layout.style['grid-template-rows']).toContain('repeat(3,');
    expect(layout.sections[1].style['grid-row']).toBe('2 / span 2');
  });

  it('places a spanning cell across all five columns', () => {
    const layout = layoutSheetTable(fixtureTable());
    const groupCell = layout.sections[1].cells[0];
    expect(groupCell.style['grid-column']).toBe('1 / span 5');
  });

  it('keeps the sheet ids on what it lays out, so cells can be matched back', () => {
    const table = fixtureTable();
    const layout = layoutSheetTable(table);
    const group = table.sections.find((section) => section.name === 'Group');
    expect(layout.sections[1].section.id).toBe(group?.id);
    expect(layout.sections[1].cells[0].cell.id).toBe(group?.rows[0].cells[0].id);
    expect(layout.sections[1].cells[0].rowId).toBe(group?.rows[0].id);
  });
});
