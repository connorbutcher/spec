import { ApplicationRef, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { CurrentUserStore } from '../../core/auth/current-user.store';
import { RowTakeover } from './models/row-takeover.model';
import { SheetConnectionState } from './models/sheet-connection-state';
import { SheetHubHandlers } from './models/sheet-hub-handlers.model';
import { SheetLiveState } from './models/sheet-live-state.model';
import { fixtureTakeover } from './row-takeover.fixture';
import { RowTakeoverStore } from './row-takeover.store';
import { SheetHub } from './sheet-hub';
import { SheetLiveStore } from './sheet-live.store';
import { SheetStore } from './sheet.store';

const ME = 1;
const SHEET_ID = 5;

/** Stands in for the SignalR connection: the test plays the server. */
class FakeSheetHub {
  public readonly state = signal<SheetConnectionState>('disconnected');
  public handlers: SheetHubHandlers | null = null;
  public joined: number[] = [];
  public onJoin: SheetLiveState = { users: [], takeovers: [] };

  public start(handlers: SheetHubHandlers): void {
    this.handlers = handlers;
  }

  public join(sheetId: number): Promise<SheetLiveState> {
    this.joined.push(sheetId);
    return Promise.resolve(this.onJoin);
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

  async function settle(): Promise<void> {
    TestBed.inject(ApplicationRef).tick();
    await new Promise((resolve) => setTimeout(resolve));
    TestBed.inject(ApplicationRef).tick();
  }

  beforeEach(async () => {
    hub = new FakeSheetHub();
    refreshes = 0;
    resets = [];
    applied = [];
    TestBed.configureTestingModule({
      providers: [
        SheetLiveStore,
        { provide: SheetHub, useValue: hub },
        {
          provide: SheetStore,
          useValue: { sheet: signal({ id: SHEET_ID }), refresh: () => refreshes++ },
        },
        {
          provide: RowTakeoverStore,
          useValue: {
            reset: (waiting: RowTakeover[]) => resets.push(waiting),
            apply: (takeover: RowTakeover) => applied.push(takeover),
          },
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
    hub.onJoin = {
      users: [{ userId: ME, displayName: 'Developer', connectionCount: 1 }],
      takeovers: [waiting],
    };
    hub.state.set('connected');
    await settle();

    expect(hub.joined).toEqual([SHEET_ID]);
    expect(live.users().map((user) => user.displayName)).toEqual(['Developer']);
    expect(resets.at(-1)).toEqual([waiting]);
    expect(live.viewerId()).toBe(ME);
  });

  it('shows nobody while the connection is down', async () => {
    hub.onJoin = {
      users: [{ userId: ME, displayName: 'Developer', connectionCount: 1 }],
      takeovers: [],
    };
    hub.state.set('connected');
    await settle();

    hub.state.set('reconnecting');
    await settle();

    expect(live.users()).toEqual([]);
    expect(live.connectionState()).toBe('reconnecting');
    expect(resets.at(-1)).toEqual([]);
  });

  it('joins again after a reconnection and catches up on the sheet', async () => {
    hub.state.set('connected');
    await settle();
    expect(refreshes).toBe(0);

    hub.state.set('reconnecting');
    await settle();
    hub.state.set('connected');
    await settle();

    expect(hub.joined).toEqual([SHEET_ID, SHEET_ID]);
    expect(refreshes).toBe(1);
  });

  it('reads the sheet again when the server says it changed', () => {
    hub.handlers?.sheetChanged();

    expect(refreshes).toBe(1);
  });

  it('takes in who is there as people come and go', () => {
    hub.handlers?.presenceChanged([{ userId: 2, displayName: 'Engineer Two', connectionCount: 2 }]);

    expect(live.users()).toEqual([{ userId: 2, displayName: 'Engineer Two', connectionCount: 2 }]);
  });

  it('hands takeover requests to the takeover store', () => {
    const takeover = fixtureTakeover();

    hub.handlers?.takeoverChanged(takeover);

    expect(applied).toEqual([takeover]);
  });
});
