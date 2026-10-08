import { SheetLock } from './models/sheet-lock.model';
import { Sheet } from './models/sheet.model';
import { hubRefusalMessage, rowIdOfCell, withRowCheckouts } from './row-checkout.util';
import { buildSheetIndex } from './sheet-index.util';
import { fixtureTable } from './sheet-structure.fixture';

const THEIRS: SheetLock = { userId: 2, userName: 'Engineer Two', isMine: false };

function fixtureSheet(): Sheet {
  return {
    id: 5,
    publicId: 'sheet-5',
    phaseId: 1,
    sheetTypeId: 2,
    isLive: true,
    viewedVersionNumber: null,
    viewedAsOfUtc: null,
    latestVersionNumber: 1,
    myDraftCount: 0,
    otherDrafts: [],
    versions: [],
    tables: [fixtureTable()],
    availableTemplates: [],
  };
}

describe('withRowCheckouts', () => {
  it('returns the sheet itself when nobody is in a row', () => {
    const sheet = fixtureSheet();

    expect(withRowCheckouts(sheet, new Map())).toBe(sheet);
  });

  it('shows a row someone is in as locked to them, and leaves the other rows alone', () => {
    const sheet = fixtureSheet();
    const [header, group] = sheet.tables[0].sections;
    const row = group.rows[0];

    const shown = withRowCheckouts(sheet, new Map([[row.id, THEIRS]]));

    expect(shown.tables[0].sections[1].rows[0]).toEqual({ ...row, lock: THEIRS });
    expect(shown.tables[0].sections[0].rows[0]).toBe(header.rows[0]);
    expect(row.lock).toBeNull();
  });

  it('keeps the lock of a row someone has already changed', () => {
    const sheet = fixtureSheet();
    const row = sheet.tables[0].sections[1].rows[0];
    const changedBy: SheetLock = { userId: 3, userName: 'Engineer Three', isMine: false };
    row.lock = changedBy;

    const shown = withRowCheckouts(sheet, new Map([[row.id, THEIRS]]));

    expect(shown.tables[0].sections[1].rows[0].lock).toBe(changedBy);
  });
});

describe('rowIdOfCell', () => {
  it('finds the row a cell is in', () => {
    const sheet = fixtureSheet();
    const row = sheet.tables[0].sections[1].rows[0];
    const index = buildSheetIndex(sheet);

    expect(rowIdOfCell(index, row.cells[0].id)).toBe(row.id);
    expect(rowIdOfCell(index, -1)).toBeNull();
  });
});

describe('hubRefusalMessage', () => {
  it("takes the server's reason out of the sentence SignalR wraps it in", () => {
    const failure = new Error(
      "An unexpected error occurred invoking 'CheckOutRow' on the server. HubException: This row is being edited by Sam.",
    );

    expect(hubRefusalMessage(failure)).toBe('This row is being edited by Sam.');
  });

  it('falls back to a plain message for anything else', () => {
    expect(hubRefusalMessage(new Error('socket closed'))).toBe(
      "This row couldn't be checked out to you.",
    );
    expect(hubRefusalMessage('nope')).toBe("This row couldn't be checked out to you.");
  });
});
