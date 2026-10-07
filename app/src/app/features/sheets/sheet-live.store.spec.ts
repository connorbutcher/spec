import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationRef, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MessageService } from 'primeng/api';
import { CurrentUserStore } from '../../core/auth/current-user.store';
import { RowTakeover } from './models/row-takeover.model';
import { SheetConnectionState } from './models/sheet-connection-state';
import { SheetHubHandlers } from './models/sheet-hub-handlers.model';
import { SheetLiveState } from './models/sheet-live-state.model';
import { SheetHub } from './sheet-hub';
import { SheetLiveStore } from './sheet-live.store';
import { SheetStore } from './sheet.store';

const ME = 1;
const SHEET_ID = 5;

function takeover(change: Partial<RowTakeover> = {}): RowTakeover {
  return {
    id: 'a',
    sheetId: SHEET_ID,
    rowId: 10,
    requesterUserId: 2,
    requesterName: 'Engineer Two',
    holderUserId: ME,
    holderName: 'Developer',
    requestedAtUtc: '2026-10-07T12:00:00Z',
    expiresAtUtc: '2026-10-07T12:01:00Z',
    status: 'Pending',
    ...change,
  };
}

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
  let http: HttpTestingController;
  let refreshes: number;
  let messages: MessageService;
  const sheetError = signal<string | null>(null);

  async function settle(): Promise<void> {
    TestBed.inject(ApplicationRef).tick();
    await new Promise((resolve) => setTimeout(resolve));
    TestBed.inject(ApplicationRef).tick();
  }

  beforeEach(async () => {
    hub = new FakeSheetHub();
    refreshes = 0;
    sheetError.set(null);
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        MessageService,
        SheetLiveStore,
        { provide: SheetHub, useValue: hub },
        {
          provide: SheetStore,
          useValue: {
            sheet: signal({ id: SHEET_ID }),
            error: sheetError,
            refresh: () => refreshes++,
          },
        },
        { provide: CurrentUserStore, useValue: { user: signal({ id: ME }) } },
      ],
    });
    live = TestBed.inject(SheetLiveStore);
    http = TestBed.inject(HttpTestingController);
    messages = TestBed.inject(MessageService);
    vi.spyOn(messages, 'add');
    await settle();
  });

  afterEach(() => {
    http.verify();
    TestBed.resetTestingModule();
  });

  it('joins the open sheet once connected, and takes in who is there', async () => {
    expect(hub.joined).toEqual([]);

    hub.onJoin = {
      users: [{ userId: ME, displayName: 'Developer', connectionCount: 1 }],
      takeovers: [takeover()],
    };
    hub.state.set('connected');
    await settle();

    expect(hub.joined).toEqual([SHEET_ID]);
    expect(live.users().map((user) => user.displayName)).toEqual(['Developer']);
    expect(live.incoming().map((request) => request.id)).toEqual(['a']);
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

  it('lists a request for a row of mine until it is settled, then says what happened', () => {
    hub.handlers?.takeoverChanged(takeover());
    expect(live.incoming()).toHaveLength(1);
    expect(live.outgoingByRow().size).toBe(0);

    hub.handlers?.takeoverChanged(takeover({ status: 'GrantedOnTimeout' }));

    expect(live.incoming()).toHaveLength(0);
    expect(messages.add).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'warn', summary: 'Row taken over', sticky: true }),
    );
  });

  it('announces a settled request once, however many times it hears of it', async () => {
    const mine = takeover({ requesterUserId: ME, holderUserId: 2, holderName: 'Engineer Two' });

    const asking = live.request(mine.rowId);
    http.expectOne('/api/sheet-rows/10/takeover-requests').flush(mine);
    await asking;
    expect(live.outgoingByRow().get(10)?.id).toBe('a');

    hub.handlers?.takeoverChanged({ ...mine, status: 'Approved' });
    hub.handlers?.takeoverChanged({ ...mine, status: 'Approved' });

    expect(live.outgoingByRow().size).toBe(0);
    expect(messages.add).toHaveBeenCalledTimes(1);
  });

  it('shows why a request could not be made', async () => {
    const asking = live.request(10);
    http
      .expectOne('/api/sheet-rows/10/takeover-requests')
      .flush(
        { detail: 'Engineer Three has already asked to take over this row.' },
        { status: 409, statusText: 'Conflict' },
      );
    await asking;

    expect(sheetError()).toBe('Engineer Three has already asked to take over this row.');
  });

  it('drops a request that had already gone when it was answered', async () => {
    hub.handlers?.takeoverChanged(takeover());

    const answering = live.approve(live.incoming()[0]);
    http
      .expectOne('/api/row-takeovers/a/approve')
      .flush(
        { detail: 'That request is no longer open.' },
        { status: 404, statusText: 'Not Found' },
      );
    await answering;

    expect(live.incoming()).toHaveLength(0);
  });

  it('counts down to the moment an unanswered request is granted', () => {
    live.now.set(Date.parse('2026-10-07T12:00:18.200Z'));

    expect(live.secondsLeft(takeover())).toBe(42);

    live.now.set(Date.parse('2026-10-07T12:02:00Z'));
    expect(live.secondsLeft(takeover())).toBe(0);
  });
});
