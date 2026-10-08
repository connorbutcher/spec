import { ApplicationRef, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { CurrentUserStore } from '../../core/auth/current-user.store';
import { RowCheckout } from './models/row-checkout.model';
import { RowTakeover } from './models/row-takeover.model';
import { SheetConnectionState } from './models/sheet-connection-state';
import { SheetHubHandlers } from './models/sheet-hub-handlers.model';
import { SheetIndex } from './models/sheet-index.model';
import { SheetLiveState } from './models/sheet-live-state.model';
import { RowCheckoutStore } from './row-checkout.store';
import { fixtureTakeover } from './row-takeover.fixture';
import { RowTakeoverStore } from './row-takeover.store';
import { SheetHub } from './sheet-hub';
import { buildSheetIndex } from './sheet-index.util';
import { SheetLiveStore } from './sheet-live.store';
import { fixtureCell, fixtureRow } from './sheet-structure.fixture';
import { SheetStore } from './sheet.store';

const ME = 1;
const SHEET_ID = 5;
/** Longer than the pause before a row is let go. */
const AFTER_RELEASE_DELAY_MS = 200;

/** Stands in for the SignalR connection: the test plays the server. */
class FakeSheetHub {
  public readonly state = signal<SheetConnectionState>('disconnected');
  public handlers: SheetHubHandlers | null = null;
  public joined: number[] = [];
  public onJoin: SheetLiveState = { users: [], checkouts: [], takeovers: [] };
  /** Every row call made, in order: a row id for a checkout, 'release' for letting go. */
  public rowCalls: (number | 'release')[] = [];
  /** Set to make the next checkout fail the way the server refuses one. */
  public refusal: string | null = null;

  public start(handlers: SheetHubHandlers): void {
    this.handlers = handlers;
  }

  public join(sheetId: number): Promise<SheetLiveState> {
    this.joined.push(sheetId);
    return Promise.resolve(this.onJoin);
  }

  public checkOut(rowId: number): Promise<void> {
    this.rowCalls.push(rowId);
    return this.refusal === null
      ? Promise.resolve()
      : Promise.reject(new Error(`An unexpected error occurred. HubException: ${this.refusal}`));
  }

  public release(): Promise<void> {
    this.rowCalls.push('release');
    return Promise.resolve();
  }

  public stop(): Promise<void> {
    return Promise.resolve();
  }
}

describe('SheetLiveStore', () => {
  let live: SheetLiveStore;
  let hub: FakeSheetHub;
  let refreshes: number;
  let resets: RowTakeover[][];
  let applied: RowTakeover[];
  let checkouts: RowCheckout[][];

  const firstRow = fixtureRow([fixtureCell(1), fixtureCell(2)]);
  const secondRow = fixtureRow([fixtureCell(1)]);
  const index: SheetIndex = {
    ...buildSheetIndex(null),
    rows: new Map([
      [firstRow.id, firstRow],
      [secondRow.id, secondRow],
    ]),
  };
  const editingCellId = signal<number | null>(null);
  const sheetError = signal<string | null>(null);

  async function settle(waitMs = 0): Promise<void> {
    TestBed.inject(ApplicationRef).tick();
    await new Promise((resolve) => setTimeout(resolve, waitMs));
    TestBed.inject(ApplicationRef).tick();
  }

  async function connect(): Promise<void> {
    hub.state.set('connected');
    await settle();
  }

  beforeEach(async () => {
    hub = new FakeSheetHub();
    refreshes = 0;
    resets = [];
    applied = [];
    checkouts = [];
    editingCellId.set(null);
    sheetError.set(null);
    TestBed.configureTestingModule({
      providers: [
        SheetLiveStore,
        { provide: SheetHub, useValue: hub },
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
    await settle();
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('joins the open sheet once connected, and takes in who is there', async () => {
    expect(hub.joined).toEqual([]);

    const waiting = fixtureTakeover();
    const inARow = { rowId: secondRow.id, userId: 2, userName: 'Engineer Two' };
    hub.onJoin = {
      users: [{ userId: ME, displayName: 'Developer', connectionCount: 1 }],
      checkouts: [inARow],
      takeovers: [waiting],
    };
    await connect();

    expect(hub.joined).toEqual([SHEET_ID]);
    expect(live.users().map((user) => user.displayName)).toEqual(['Developer']);
    expect(checkouts.at(-1)).toEqual([inARow]);
    expect(resets.at(-1)).toEqual([waiting]);
    expect(live.viewerId()).toBe(ME);
  });

  it('shows nobody while the connection is down', async () => {
    hub.onJoin = {
      users: [{ userId: ME, displayName: 'Developer', connectionCount: 1 }],
      checkouts: [{ rowId: secondRow.id, userId: 2, userName: 'Engineer Two' }],
      takeovers: [],
    };
    await connect();

    hub.state.set('reconnecting');
    await settle();

    expect(live.users()).toEqual([]);
    expect(live.connectionState()).toBe('reconnecting');
    expect(checkouts.at(-1)).toEqual([]);
    expect(resets.at(-1)).toEqual([]);
  });

  it('joins again after a reconnection and catches up on the sheet', async () => {
    await connect();
    expect(refreshes).toBe(0);

    hub.state.set('reconnecting');
    await settle();
    await connect();

    expect(hub.joined).toEqual([SHEET_ID, SHEET_ID]);
    expect(refreshes).toBe(1);
  });

  it('reads the sheet again when the server says it changed', () => {
    hub.handlers?.sheetChanged();

    expect(refreshes).toBe(1);
  });

  it('takes in who is there, and which rows they are in, as people come and go', () => {
    hub.handlers?.presenceChanged([{ userId: 2, displayName: 'Engineer Two', connectionCount: 2 }]);
    hub.handlers?.checkoutsChanged([{ rowId: firstRow.id, userId: 2, userName: 'Engineer Two' }]);

    expect(live.users()).toEqual([{ userId: 2, displayName: 'Engineer Two', connectionCount: 2 }]);
    expect(checkouts.at(-1)).toEqual([{ rowId: firstRow.id, userId: 2, userName: 'Engineer Two' }]);
  });

  it('hands takeover requests to the takeover store', () => {
    const takeover = fixtureTakeover();

    hub.handlers?.takeoverChanged(takeover);

    expect(applied).toEqual([takeover]);
  });

  it('checks a row out when the viewer clicks into one of its cells', async () => {
    await connect();

    editingCellId.set(firstRow.cells[0].id);
    await settle();

    expect(hub.rowCalls).toEqual([firstRow.id]);
  });

  it('keeps hold of the row while the viewer moves between its cells', async () => {
    await connect();
    editingCellId.set(firstRow.cells[0].id);
    await settle();

    // Leaving one cell and entering the next are two steps, with no cell in between.
    editingCellId.set(null);
    await settle();
    editingCellId.set(firstRow.cells[1].id);
    await settle(AFTER_RELEASE_DELAY_MS);

    expect(hub.rowCalls).toEqual([firstRow.id]);
  });

  it('lets the row go a moment after the viewer leaves it', async () => {
    await connect();
    editingCellId.set(firstRow.cells[0].id);
    await settle();

    editingCellId.set(null);
    await settle(AFTER_RELEASE_DELAY_MS);

    expect(hub.rowCalls).toEqual([firstRow.id, 'release']);
  });

  it('moves straight to another row without letting go first', async () => {
    await connect();
    editingCellId.set(firstRow.cells[0].id);
    await settle();

    editingCellId.set(secondRow.cells[0].id);
    await settle(AFTER_RELEASE_DELAY_MS);

    expect(hub.rowCalls).toEqual([firstRow.id, secondRow.id]);
  });

  it('checks the row out again after a reconnection, as the server forgot it', async () => {
    await connect();
    editingCellId.set(firstRow.cells[0].id);
    await settle();

    hub.state.set('reconnecting');
    await settle();
    await connect();

    expect(hub.rowCalls).toEqual([firstRow.id, firstRow.id]);
  });

  it('asks for nothing while there is no connection', async () => {
    editingCellId.set(firstRow.cells[0].id);
    await settle();

    expect(hub.rowCalls).toEqual([]);
  });

  it('says why and takes the viewer out of the cell when someone else got there first', async () => {
    await connect();
    hub.refusal = 'This row is being edited by Engineer Two.';

    editingCellId.set(firstRow.cells[0].id);
    await settle();

    expect(sheetError()).toBe('This row is being edited by Engineer Two.');
    expect(editingCellId()).toBeNull();
  });
});
