import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RowTakeover } from '../models/row-takeover.model';
import { SheetRow } from '../models/sheet-row.model';
import { fixtureTakeover } from '../row-takeover.fixture';
import { RowTakeoverStore } from '../row-takeover.store';
import { fixtureRow } from '../sheet-structure.fixture';
import { SheetTakeoverButton } from './sheet-takeover-button';

describe('SheetTakeoverButton', () => {
  let fixture: ComponentFixture<SheetTakeoverButton>;
  let http: HttpTestingController;
  let row: SheetRow;
  let requested: number[];
  const outgoing = signal<ReadonlyMap<number, RowTakeover>>(new Map());

  function availabilityUrl(): string {
    return `/api/sheet-rows/${row.id}/takeover-availability`;
  }

  function text(): string {
    return (fixture.nativeElement as HTMLElement).textContent.replace(/\s+/g, ' ').trim();
  }

  function button(): HTMLButtonElement | null {
    return (fixture.nativeElement as HTMLElement).querySelector('button');
  }

  /** Lets the resource send its request and what came back reach the screen. */
  async function settle(): Promise<void> {
    fixture.detectChanges();
    await new Promise((resolve) => setTimeout(resolve));
    fixture.detectChanges();
  }

  beforeEach(async () => {
    requested = [];
    outgoing.set(new Map());
    row = { ...fixtureRow([]), lock: { userId: 2, userName: 'Engineer Two', isMine: false } };
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: RowTakeoverStore,
          useValue: {
            outgoingByRow: outgoing,
            secondsLeft: () => 42,
            request: (rowId: number) => {
              requested.push(rowId);
              return Promise.resolve();
            },
          },
        },
      ],
    });
    fixture = TestBed.createComponent(SheetTakeoverButton);
    fixture.componentRef.setInput('row', row);
    http = TestBed.inject(HttpTestingController);
    await settle();
  });

  afterEach(() => {
    http.verify();
  });

  it('offers nothing to press until the server says the row can be asked for', async () => {
    expect(button()?.disabled).toBe(true);

    http.expectOne(availabilityUrl()).flush({ isAvailable: true, reason: null });
    await settle();

    expect(button()?.disabled).toBe(false);
    expect(text()).toContain('Request takeover');
  });

  it('shows the reason in place of the button for a row with unpublished changes', async () => {
    const reason = 'Engineer Two has unpublished changes on this row.';
    http.expectOne(availabilityUrl()).flush({ isAvailable: false, reason });
    await settle();

    expect(button()).toBeNull();
    expect(text()).toBe(reason);
  });

  it('asks again when the row changes, as it may have been published', async () => {
    http.expectOne(availabilityUrl()).flush({ isAvailable: false, reason: 'Has changes.' });
    await settle();

    fixture.componentRef.setInput('row', { ...row, lastChange: null, displayOrder: 99 });
    await settle();
    http.expectOne(availabilityUrl()).flush({ isAvailable: true, reason: null });
    await settle();

    expect(text()).toContain('Request takeover');
  });

  it('asks for the row, then checks again whether it can still be asked for', async () => {
    http.expectOne(availabilityUrl()).flush({ isAvailable: true, reason: null });
    await settle();

    button()?.click();
    await settle();

    expect(requested).toEqual([row.id]);
    http.expectOne(availabilityUrl()).flush({ isAvailable: true, reason: null });
    await settle();
  });

  it('shows the countdown and Withdraw while a request waits', async () => {
    http.expectOne(availabilityUrl()).flush({ isAvailable: true, reason: null });
    outgoing.set(
      new Map([[row.id, fixtureTakeover({ rowId: row.id, holderName: 'Engineer Two' })]]),
    );
    await settle();

    expect(text()).toContain('Asked Engineer Two · yours in 42s if unanswered');
    expect(text()).toContain('Withdraw');
  });
});
