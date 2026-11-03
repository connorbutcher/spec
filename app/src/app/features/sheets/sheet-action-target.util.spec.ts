import { SheetSelection } from './models/sheet-selection.model';
import { SheetTable } from './models/sheet-table.model';
import { addableSection, sheetActionTarget } from './sheet-action-target.util';
import { buildSheetIndex } from './sheet-index.util';
import { fixtureCell, fixtureRow, fixtureTable } from './sheet-structure.fixture';

function indexOf(tables: SheetTable[]): ReturnType<typeof buildSheetIndex> {
  return buildSheetIndex({ tables } as Parameters<typeof buildSheetIndex>[0]);
}

describe('sheet action target', () => {
  it('has no target when nothing is selected', () => {
    const table = fixtureTable();

    expect(sheetActionTarget(indexOf([table]), [table], null)).toBeNull();
  });

  it('acts on the table when only the table is selected', () => {
    const first = fixtureTable();
    const second = { ...fixtureTable(), id: 2 };
    const tables = [first, second];
    const selection: SheetSelection = { tableId: 2, sectionId: null, rowId: null };

    const target = sheetActionTarget(indexOf(tables), tables, selection);

    expect(target?.kind).toBe('table');
    expect(target?.canMoveUp).toBe(true);
    expect(target?.canMoveDown).toBe(false);
    expect(target?.canRemove).toBe(true);
  });

  it('leaves a table alone while someone else has a change to it', () => {
    const table = { ...fixtureTable(), lock: { userName: 'A. Smith', isMine: false } };
    const tables = [table as SheetTable];
    const selection: SheetSelection = { tableId: 1, sectionId: null, rowId: null };

    expect(sheetActionTarget(indexOf(tables), tables, selection)?.canRemove).toBe(false);
  });

  it('acts on the section when its only row is selected', () => {
    const table = fixtureTable();
    const group = table.sections[0];
    const selection = { tableId: 1, sectionId: group.id, rowId: group.rows[0].id };

    const target = sheetActionTarget(indexOf([table]), [table], selection);

    expect(target?.kind).toBe('section');
    expect(target?.id).toBe(group.id);
  });

  it('acts on the row when its section has several', () => {
    const table = fixtureTable();
    const group = table.sections[0];
    group.rows.push(fixtureRow([fixtureCell(1, 5)]));
    const selection = { tableId: 1, sectionId: group.id, rowId: group.rows[1].id };

    const target = sheetActionTarget(indexOf([table]), [table], selection);

    expect(target?.kind).toBe('row');
    expect(target?.canMoveUp).toBe(true);
    expect(target?.canMoveDown).toBe(false);
  });

  it('has no target in the header, which stays where it is', () => {
    const table = fixtureTable();
    const header = table.sections[1];
    const selection = { tableId: 1, sectionId: header.id, rowId: header.rows[0].id };

    expect(sheetActionTarget(indexOf([table]), [table], selection)).toBeNull();
  });

  it('adds into the nearest section around the selection that can take sections', () => {
    const table = fixtureTable();
    const group = table.sections[0];
    const limits = group.sections[0];
    group.addableSections = [
      {
        templateSectionId: 7,
        name: 'Limits',
        count: 1,
        minInstances: 0,
        maxInstances: null,
        canAdd: true,
      },
    ];
    const index = indexOf([table]);

    expect(addableSection(index, { tableId: 1, sectionId: limits.id, rowId: null })).toBe(group);
    expect(addableSection(index, { tableId: 1, sectionId: null, rowId: null })).toBeNull();
  });
});
