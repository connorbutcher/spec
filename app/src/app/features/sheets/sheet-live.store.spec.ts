import { ApplicationRef, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { CurrentUserStore } from '../../core/auth/current-user.store';
import { RowCheckout } from './models/row-checkout.model';
import { RowTakeover } from './models/row-takeover.model';
import { SheetIndex } from './models/sheet-index.model';
import { FakeSheetHub } from './fake-sheet-hub.fixture';
import { RowCheckoutStore } from './row-checkout.store';
import { RowTakeoverFixture } from './row-takeover.fixture';
import { RowTakeoverStore } from './row-takeover.store';
import { SheetHub } from './sheet-hub';
import { SheetIndexUtil } from './sheet-index.util';
import { SheetLiveStore } from './sheet-live.store';
import { SheetStructureFixture } from './sheet-structure.fixture';
import { SheetStore } from './sheet.store';

const ME = 1;
const SHEET_ID = 5;
/** Longer than the pause before a row is let go. */
const AFTER_RELEASE_DELAY_MS = 200;

class SheetLiveStorePage {
  public hub!: FakeSheetHub;

  public async settle(waitMs = 0): Promise<void> {
    TestBed.inject(ApplicationRef).tick();
    await new Promise((resolve) => setTimeout(resolve, waitMs));
    TestBed.inject(ApplicationRef).tick();
  }

  public async connect(): Promise<void> {
    this.hub.state.set('connected');
    await this.settle();
  }
}

describe('SheetLiveStore', () => {
  let page: SheetLiveStorePage;

  beforeEach(() => {
    page = new SheetLiveStorePage();
  });

  let live: SheetLiveStore;
  let refreshes: number;
  let resets: RowTakeover[][];
  let applied: RowTakeover[];
  let checkouts: RowCheckout[][];

  const firstRow = SheetStructureFixture.fixtureRow([SheetStructureFixture.fixtureCell(1), SheetStructureFixture.fixtureCell(2)]);
  const secondRow = SheetStructureFixture.fixtureRow([SheetStructureFixture.fixtureCell(1)]);
  const index: SheetIndex = {
    ...SheetIndexUtil.buildSheetIndex(null),
    rows: new Map([
      [firstRow.id, firstRow],
      [secondRow.id, secondRow],
    ]),
    cellRow: new Map([
      ...firstRow.cells.map((cell): [number, number] => [cell.id, firstRow.id]),
      ...secondRow.cells.map((cell): [number, number] => [cell.id, secondRow.id]),
    ]),
  };
  const editingCellId = signal<number | null>(null);
  const sheetError = signal<string | null>(null);
  beforeEach(async () => {
    page.hub = new FakeSheetHub();
    refreshes = 0;
    resets = [];
    applied = [];
    checkouts = [];
    editingCellId.set(null);
    sheetError.set(null);
    TestBed.configureTestingModule({
      providers: [
        SheetLiveStore,
        { provide: SheetHub, useValue: page.hub },
        {
          provide: SheetStore,
          useValue: {
            sheet: signal({ id: SHEET_ID }),
            index: signal(index),
            editingCellId,
            error: sheetError,
            refresh: () => refreshes++,
            stopEditing: (cellId: number) => {
              if (editingCellId() === cellId) {
                editingCellId.set(null);
              }
            },
          },
        },
        {
          provide: RowTakeoverStore,
          useValue: {
            reset: (waiting: RowTakeover[]) => resets.push(waiting),
            apply: (takeover: RowTakeover) => applied.push(takeover),
          },
        },
        {
          provide: RowCheckoutStore,
          useValue: { set: (list: RowCheckout[]) => checkouts.push(list) },
        },
        { provide: CurrentUserStore, useValue: { user: signal({ id: ME }) } },
      ],
    });
    live = TestBed.inject(SheetLiveStore);
    await page.settle();
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('joins the open sheet once connected, and takes in who is there', async () => {
    expect(page.hub.joined).toEqual([]);

    const waiting = RowTakeoverFixture.fixtureTakeover();
    const inARow = { rowId: secondRow.id, userId: 2, userName: 'Engineer Two' };
    page.hub.onJoin = {
      users: [{ userId: ME, displayName: 'Developer', connectionCount: 1 }],
      checkouts: [inARow],
      takeovers: [waiting],
    };
    await page.connect();

    expect(page.hub.joined).toEqual([SHEET_ID]);
    expect(live.users().map((user) => user.displayName)).toEqual(['Developer']);
    expect(checkouts.at(-1)).toEqual([inARow]);
    expect(resets.at(-1)).toEqual([waiting]);
    expect(live.viewerId()).toBe(ME);
  });

  it('shows nobody while the connection is down', async () => {
    page.hub.onJoin = {
      users: [{ userId: ME, displayName: 'Developer', connectionCount: 1 }],
      checkouts: [{ rowId: secondRow.id, userId: 2, userName: 'Engineer Two' }],
      takeovers: [],
    };
    await page.connect();

    page.hub.state.set('reconnecting');
    await page.settle();

    expect(live.users()).toEqual([]);
    expect(live.connectionState()).toBe('reconnecting');
    expect(checkouts.at(-1)).toEqual([]);
    expect(resets.at(-1)).toEqual([]);
  });

  it('joins again after a reconnection and catches up on the sheet', async () => {
    await page.connect();
    expect(refreshes).toBe(0);

    page.hub.state.set('reconnecting');
    await page.settle();
    await page.connect();

    expect(page.hub.joined).toEqual([SHEET_ID, SHEET_ID]);
    expect(refreshes).toBe(1);
  });

  it('reads the sheet again when the server says it changed', () => {
    page.hub.handlers?.sheetChanged();

    expect(refreshes).toBe(1);
  });

  it('takes in who is there, and which rows they are in, as people come and go', () => {
    page.hub.handlers?.presenceChanged([{ userId: 2, displayName: 'Engineer Two', connectionCount: 2 }]);
    page.hub.handlers?.checkoutsChanged([{ rowId: firstRow.id, userId: 2, userName: 'Engineer Two' }]);

    expect(live.users()).toEqual([{ userId: 2, displayName: 'Engineer Two', connectionCount: 2 }]);
    expect(checkouts.at(-1)).toEqual([{ rowId: firstRow.id, userId: 2, userName: 'Engineer Two' }]);
  });

  it('hands takeover requests to the takeover store', () => {
    const takeover = RowTakeoverFixture.fixtureTakeover();

    page.hub.handlers?.takeoverChanged(takeover);

    expect(applied).toEqual([takeover]);
  });

  it('checks a row out when the viewer clicks into one of its cells', async () => {
    await page.connect();

    editingCellId.set(firstRow.cells[0].id);
    await page.settle();

    expect(page.hub.rowCalls).toEqual([firstRow.id]);
  });

  it('keeps hold of the row while the viewer moves between its cells', async () => {
    await page.connect();
    editingCellId.set(firstRow.cells[0].id);
    await page.settle();

    // Leaving one cell and entering the next are two steps, with no cell in between.
    editingCellId.set(null);
    await page.settle();
    editingCellId.set(firstRow.cells[1].id);
    await page.settle(AFTER_RELEASE_DELAY_MS);

    expect(page.hub.rowCalls).toEqual([firstRow.id]);
  });

  it('lets the row go a moment after the viewer leaves it', async () => {
    await page.connect();
    editingCellId.set(firstRow.cells[0].id);
    await page.settle();

    editingCellId.set(null);
    await page.settle(AFTER_RELEASE_DELAY_MS);

    expect(page.hub.rowCalls).toEqual([firstRow.id, 'release']);
  });

  it('moves straight to another row without letting go first', async () => {
    await page.connect();
    editingCellId.set(firstRow.cells[0].id);
    await page.settle();

    editingCellId.set(secondRow.cells[0].id);
    await page.settle(AFTER_RELEASE_DELAY_MS);

    expect(page.hub.rowCalls).toEqual([firstRow.id, secondRow.id]);
  });

  it('checks the row out again after a reconnection, as the server forgot it', async () => {
    await page.connect();
    editingCellId.set(firstRow.cells[0].id);
    await page.settle();

    page.hub.state.set('reconnecting');
    await page.settle();
    await page.connect();

    expect(page.hub.rowCalls).toEqual([firstRow.id, firstRow.id]);
  });

  it('asks for nothing while there is no connection', async () => {
    editingCellId.set(firstRow.cells[0].id);
    await page.settle();

    expect(page.hub.rowCalls).toEqual([]);
  });

  it('says why and takes the viewer out of the cell when someone else got there first', async () => {
    await page.connect();
    page.hub.refusal = 'This row is being edited by Engineer Two.';

    editingCellId.set(firstRow.cells[0].id);
    await page.settle();

    expect(sheetError()).toBe('This row is being edited by Engineer Two.');
    expect(editingCellId()).toBeNull();
  });
});
