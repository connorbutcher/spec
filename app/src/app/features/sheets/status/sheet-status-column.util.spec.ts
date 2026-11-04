import { TemplateLayout } from '../../templates/models/template-layout.model';
import { SheetLayoutUtil } from '../layout/sheet-layout.util';
import {
  STATUS_COLUMN_WIDTH, SheetStatusColumnUtil } from './sheet-status-column.util';
import { SheetStructureFixture } from '../data/sheet-structure.fixture';

describe('addStatusColumn', () => {
  it('puts a fixed column in front of the table columns', () => {
    const layout = SheetLayoutUtil.layoutSheetTable(SheetStructureFixture.fixtureTable());
    expect(layout.style['grid-template-columns']).toBe(
      `${STATUS_COLUMN_WIDTH}px repeat(5, minmax(112px, 1fr))`,
    );
  });

  it('moves every cell one column right and keeps its span, in nested sections too', () => {
    const layout = SheetLayoutUtil.layoutSheetTable(SheetStructureFixture.fixtureTable());
    const group = layout.sections[1];
    expect(layout.sections[0].cells.map((cell) => cell.style['grid-column'])).toEqual([
      '2 / span 1',
      '3 / span 1',
      '4 / span 1',
      '5 / span 1',
      '6 / span 1',
    ]);
    expect(group.cells[0].style['grid-column']).toBe('2 / span 5');
    expect(group.children[0].cells[0].style['grid-column']).toBe('2 / span 1');
  });

  it('leaves sections spanning every column, so each takes in the status column', () => {
    const layout = SheetLayoutUtil.layoutSheetTable(SheetStructureFixture.fixtureTable());
    expect(layout.sections.map((section) => section.style['grid-column'])).toEqual([
      '1 / -1',
      '1 / -1',
    ]);
  });

  it('keeps pinned columns pinned just right of the status column', () => {
    const layout = SheetLayoutUtil.layoutSheetTable({ ...SheetStructureFixture.fixtureTable(), stickyColumnCount: 2 });
    const [first, second, third] = layout.sections[0].cells;
    expect(first.style['left']).toBe(`${STATUS_COLUMN_WIDTH}px`);
    expect(second.style['left']).toBe(`${STATUS_COLUMN_WIDTH + 160}px`);
    expect(third.style['position']).toBeUndefined();
    expect(layout.style['grid-template-columns']).toMatch(
      new RegExp(`^${STATUS_COLUMN_WIDTH}px 160px 160px minmax`),
    );
  });

  it('moves a column block along with the cells in it', () => {
    const layout: TemplateLayout = {
      style: { 'grid-template-columns': 'repeat(4, minmax(112px, 1fr))' },
      sections: [],
      columnBlocks: [
        {
          block: {
            id: 1,
            name: 'Supplier',
            displayOrder: 1,
            minInstances: 0,
            maxInstances: null,
            initialInstances: 1,
            stickyColumnCount: 0,
          },
          style: { 'grid-row': '1 / -1', 'grid-column': '3 / span 2' },
        },
      ],
    };
    SheetStatusColumnUtil.addStatusColumn(layout);
    expect(layout.columnBlocks[0].style['grid-column']).toBe('4 / span 2');
  });

  it('places each row status on its own line of the status column, and the heading down the header', () => {
    expect(SheetStatusColumnUtil.rowStatusStyle(0)).toEqual({ 'grid-row': '1', 'grid-column': '1' });
    expect(SheetStatusColumnUtil.rowStatusStyle(2)).toEqual({ 'grid-row': '3', 'grid-column': '1' });
    expect(SheetStatusColumnUtil.statusHeadingStyle()).toEqual({ 'grid-row': '1 / -1', 'grid-column': '1' });
  });
});
