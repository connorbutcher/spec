import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MessageService } from 'primeng/api';
import { CurrentUserStore } from '../../core/auth/current-user.store';
import { fixtureTakeover } from './row-takeover.fixture';
import { RowTakeoverStore } from './row-takeover.store';
import { SheetStore } from './sheet.store';

const ME = 1;
const SHEET_ID = 5;

describe('RowTakeoverStore', () => {
  let takeovers: RowTakeoverStore;
  let http: HttpTestingController;
  let messages: MessageService;
  let refreshes: number;
  const sheetError = signal<string | null>(null);

  beforeEach(() => {
    refreshes = 0;
    sheetError.set(null);
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        MessageService,
        RowTakeoverStore,
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
    takeovers = TestBed.inject(RowTakeoverStore);
    http = TestBed.inject(HttpTestingController);
    messages = TestBed.inject(MessageService);
    vi.spyOn(messages, 'add');
  });

  afterEach(() => {
    http.verify();
    TestBed.resetTestingModule();
  });

  it('starts from what the server says is waiting', () => {
    takeovers.reset([fixtureTakeover()]);

    expect(takeovers.incoming().map((request) => request.id)).toEqual(['a']);

    takeovers.reset([]);
    expect(takeovers.incoming()).toEqual([]);
  });

  it('lists a request for a row of mine until it is settled, then says what happened', () => {
    takeovers.apply(fixtureTakeover());
    expect(takeovers.incoming()).toHaveLength(1);
    expect(takeovers.outgoingByRow().size).toBe(0);

    takeovers.apply(fixtureTakeover({ status: 'GrantedOnTimeout' }));

    expect(takeovers.incoming()).toHaveLength(0);
    expect(refreshes).toBe(1);
    expect(messages.add).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'warn', summary: 'Row taken over', sticky: true }),
    );
  });

  it('ignores a request for another sheet', () => {
    takeovers.apply(fixtureTakeover({ sheetId: 99 }));

    expect(takeovers.incoming()).toEqual([]);
  });

  it('announces a settled request once, however many times it hears of it', async () => {
    const mine = fixtureTakeover({
      requesterUserId: ME,
      holderUserId: 2,
      holderName: 'Engineer Two',
    });

    const asking = takeovers.request(mine.rowId);
    http.expectOne('/api/sheet-rows/10/takeover-requests').flush(mine);
    await asking;
    expect(takeovers.outgoingByRow().get(10)?.id).toBe('a');

    takeovers.apply({ ...mine, status: 'Approved' });
    takeovers.apply({ ...mine, status: 'Approved' });

    expect(takeovers.outgoingByRow().size).toBe(0);
    expect(messages.add).toHaveBeenCalledTimes(1);
  });

  it('shows why a request could not be made', async () => {
    const asking = takeovers.request(10);
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
    takeovers.apply(fixtureTakeover());

    const answering = takeovers.approve(takeovers.incoming()[0]);
    http
      .expectOne('/api/row-takeovers/a/approve')
      .flush(
        { detail: 'That request is no longer open.' },
        { status: 404, statusText: 'Not Found' },
      );
    await answering;

    expect(takeovers.incoming()).toHaveLength(0);
  });

  it('takes a request off the list once I keep the row, with no notice to myself', async () => {
    takeovers.apply(fixtureTakeover());

    const answering = takeovers.deny(takeovers.incoming()[0]);
    http.expectOne('/api/row-takeovers/a/deny').flush(fixtureTakeover({ status: 'Denied' }));
    await answering;

    expect(takeovers.incoming()).toHaveLength(0);
    expect(messages.add).not.toHaveBeenCalled();
  });

  it('counts down from the time it holds', () => {
    takeovers.now.set(Date.parse('2026-10-07T12:00:18.200Z'));

    expect(takeovers.secondsLeft(fixtureTakeover())).toBe(42);
  });
});
