import { SheetSelection } from '../models/sheet-selection.model';
import { SheetTable } from '../models/sheet-table.model';
import { SheetActionTargetUtil } from './sheet-action-target.util';
import { SheetIndexUtil } from '../layout/sheet-index.util';
import { SheetStructureFixture } from '../data/sheet-structure.fixture';

class SpecHelpers {
  public static indexOf(tables: SheetTable[]): ReturnType<typeof SheetIndexUtil.buildSheetIndex> {
    return SheetIndexUtil.buildSheetIndex({ tables } as Parameters<typeof SheetIndexUtil.buildSheetIndex>[0]);
  }
}
describe('sheet action target', () => {
  it('has no target when nothing is selected', () => {
    const table = SheetStructureFixture.fixtureTable();

    expect(SheetActionTargetUtil.sheetActionTarget(SpecHelpers.indexOf([table]), [table], null)).toBeNull();
  });

  it('acts on the table when only the table is selected', () => {
    const first = SheetStructureFixture.fixtureTable();
    const second = { ...SheetStructureFixture.fixtureTable(), id: 2 };
    const tables = [first, second];
    const selection: SheetSelection = { tableId: 2, sectionId: null, rowId: null };

    const target = SheetActionTargetUtil.sheetActionTarget(SpecHelpers.indexOf(tables), tables, selection);

    expect(target?.kind).toBe('table');
    expect(target?.canMoveUp).toBe(true);
    expect(target?.canMoveDown).toBe(false);
    expect(target?.canRemove).toBe(true);
  });

  it('leaves a table alone while someone else has a change to it', () => {
    const table = { ...SheetStructureFixture.fixtureTable(), lock: { userName: 'A. Smith', isMine: false } };
    const tables = [table as SheetTable];
    const selection: SheetSelection = { tableId: 1, sectionId: null, rowId: null };

    expect(SheetActionTargetUtil.sheetActionTarget(SpecHelpers.indexOf(tables), tables, selection)?.canRemove).toBe(false);
  });

  it('acts on the section when its only row is selected', () => {
    const table = SheetStructureFixture.fixtureTable();
    const group = table.sections[0];
    const selection = { tableId: 1, sectionId: group.id, rowId: group.rows[0].id };

    const target = SheetActionTargetUtil.sheetActionTarget(SpecHelpers.indexOf([table]), [table], selection);

    expect(target?.kind).toBe('section');
    expect(target?.id).toBe(group.id);
  });

  it('leaves a section alone while someone else has a change to it', () => {
    const table = SheetStructureFixture.fixtureTable();
    const group = { ...table.sections[0], lock: { userId: 2, userName: 'A. Smith', isMine: false, isCheckout: false } };
    const locked = { ...table, sections: [group, ...table.sections.slice(1)] };
    const selection = { tableId: 1, sectionId: group.id, rowId: group.rows[0].id };

    const target = SheetActionTargetUtil.sheetActionTarget(SpecHelpers.indexOf([locked]), [locked], selection);

    expect(target?.kind).toBe('section');
    expect(target?.canMoveUp).toBe(false);
    expect(target?.canMoveDown).toBe(false);
    expect(target?.canRemove).toBe(false);
  });

  it('acts on the row when its section has several', () => {
    const table = SheetStructureFixture.fixtureTable();
    const group = table.sections[0];
    group.rows.push(SheetStructureFixture.fixtureRow([SheetStructureFixture.fixtureCell(1, 5)]));
    const selection = { tableId: 1, sectionId: group.id, rowId: group.rows[1].id };

    const target = SheetActionTargetUtil.sheetActionTarget(SpecHelpers.indexOf([table]), [table], selection);

    expect(target?.kind).toBe('row');
    expect(target?.canMoveUp).toBe(true);
    expect(target?.canMoveDown).toBe(false);
  });

  it('has no target in the header, which stays where it is', () => {
    const table = SheetStructureFixture.fixtureTable();
    const header = table.sections[1];
    const selection = { tableId: 1, sectionId: header.id, rowId: header.rows[0].id };

    expect(SheetActionTargetUtil.sheetActionTarget(SpecHelpers.indexOf([table]), [table], selection)).toBeNull();
  });

  it('adds into the nearest section around the selection that can take sections', () => {
    const table = SheetStructureFixture.fixtureTable();
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
    const index = SpecHelpers.indexOf([table]);

    expect(SheetActionTargetUtil.addableSection(index, { tableId: 1, sectionId: limits.id, rowId: null })).toBe(group);
    expect(SheetActionTargetUtil.addableSection(index, { tableId: 1, sectionId: null, rowId: null })).toBeNull();
  });
});
