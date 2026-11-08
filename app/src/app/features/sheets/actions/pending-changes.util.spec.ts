import { PendingChangesUtil } from './pending-changes.util';
import { SheetLock } from '../models/sheet-lock.model';
import { SheetRow } from '../models/sheet-row.model';
import { SheetSection } from '../models/sheet-section.model';
import { SheetTable } from '../models/sheet-table.model';
import { Sheet } from '../models/sheet.model';

class SpecHelpers {
  public static row(publicId: string, text: string, lock: SheetLock | null, isPending = false): SheetRow {
    return {
      id: 0,
      publicId,
      templateRowId: 0,
      displayOrder: 0,
      lock,
      isPending,
      canRemove: true,
      cells: [{ textValue: text } as SheetRow['cells'][number]],
      lastChange: null,
    };
  }

  public static section(publicId: string, name: string, rows: SheetRow[], lock: SheetLock | null = null): SheetSection {
    return { publicId, name, rows, sections: [], lock, isPending: false } as unknown as SheetSection;
  }

  public static sheet(sections: SheetSection[], lock: SheetLock | null = null): Sheet {
    const table = { publicId: 't1', title: 'Short block', templateName: 'Engine', lock, isPending: false, sections } as unknown as SheetTable;
    return { tables: [table] } as unknown as Sheet;
  }
}

const MINE: SheetLock = { userId: 1, userName: 'Me Myself', isMine: true };
const THEIRS: SheetLock = { userId: 2, userName: 'A. Smith', isMine: false };

describe('pendingChanges', () => {
  const sheet = SpecHelpers.sheet([
    SpecHelpers.section('s1', 'Crankshaft', [
      SpecHelpers.row('r1', 'End float', MINE),
      SpecHelpers.row('r2', 'Main journal', THEIRS, true),
      SpecHelpers.row('r3', 'Crankpin', { ...MINE, isCheckout: true }),
      SpecHelpers.row('r4', 'Flywheel bolt', null),
    ]),
  ]);

  it('lists only the viewer\'s saved changes when publishing their own', () => {
    expect(PendingChangesUtil.pendingChanges(sheet, 'Mine')).toEqual([
      { key: 'row-r1', path: 'Short block › Crankshaft · End float › End float', kind: 'Changed', holder: null },
    ]);
  });

  it('lists everyone\'s saved changes, naming whose, when publishing everything', () => {
    const changes = PendingChangesUtil.pendingChanges(sheet, 'All');
    expect(changes.map((change) => change.key)).toEqual(['row-r1', 'row-r2']);
    expect(changes[1]).toEqual(expect.objectContaining({ kind: 'New', holder: 'A. Smith' }));
  });

  it('lists a changed section once, without the rows inside it', () => {
    const changed = SpecHelpers.sheet([SpecHelpers.section('s1', 'Crankshaft', [SpecHelpers.row('r1', 'End float', MINE)], MINE)]);
    expect(PendingChangesUtil.pendingChanges(changed, 'Mine').map((change) => change.key)).toEqual(['section-s1']);
  });

  it('lists nothing without a sheet', () => {
    expect(PendingChangesUtil.pendingChanges(null, 'All')).toEqual([]);
  });
});
