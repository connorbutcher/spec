import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RowTakeover } from '../models/row-takeover.model';
import { SheetRow } from '../models/sheet-row.model';
import { RowTakeoverFixture } from '../checkouts/row-takeover.fixture';
import { RowTakeoverStore } from '../checkouts/row-takeover.store';
import { SheetStructureFixture } from '../data/sheet-structure.fixture';
import { SheetTakeoverButton } from './sheet-takeover-button';

class SheetTakeoverButtonPage {
  public fixture!: ComponentFixture<SheetTakeoverButton>;
  public row!: SheetRow;

  public availabilityUrl(): string {
    return `/api/sheet-rows/${this.row.id}/takeover-availability`;
  }

  public text(): string {
    return (this.fixture.nativeElement as HTMLElement).textContent.replace(/\s+/g, ' ').trim();
  }

  public button(): HTMLButtonElement | null {
    return (this.fixture.nativeElement as HTMLElement).querySelector('button');
  }

  /** Lets the resource send its request and what came back reach the screen. */
  public async settle(): Promise<void> {
    this.fixture.detectChanges();
    await new Promise((resolve) => setTimeout(resolve));
    this.fixture.detectChanges();
  }
}

describe('SheetTakeoverButton', () => {
  let page: SheetTakeoverButtonPage;

  beforeEach(() => {
    page = new SheetTakeoverButtonPage();
  });

  let http: HttpTestingController;
  let requested: number[];
  const outgoing = signal<ReadonlyMap<number, RowTakeover>>(new Map());
  beforeEach(async () => {
    requested = [];
    outgoing.set(new Map());
    page.row = { ...SheetStructureFixture.fixtureRow([]), lock: { userId: 2, userName: 'Engineer Two', isMine: false } };
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
    page.fixture = TestBed.createComponent(SheetTakeoverButton);
    page.fixture.componentRef.setInput('row', page.row);
    http = TestBed.inject(HttpTestingController);
    await page.settle();
  });

  afterEach(() => {
    http.verify();
  });

  it('offers nothing to press until the server says the row can be asked for', async () => {
    expect(page.button()?.disabled).toBe(true);

    http.expectOne(page.availabilityUrl()).flush({ isAvailable: true, reason: null });
    await page.settle();

    expect(page.button()?.disabled).toBe(false);
    expect(page.text()).toContain('Request takeover');
  });

  it('shows the reason in place of the button for a row with unpublished changes', async () => {
    const reason = 'Engineer Two has unpublished changes on this row.';
    http.expectOne(page.availabilityUrl()).flush({ isAvailable: false, reason });
    await page.settle();

    expect(page.button()).toBeNull();
    expect(page.text()).toBe(reason);
  });

  it('asks again when the row changes, as it may have been published', async () => {
    http.expectOne(page.availabilityUrl()).flush({ isAvailable: false, reason: 'Has changes.' });
    await page.settle();

    page.fixture.componentRef.setInput('row', { ...page.row, lastChange: null, displayOrder: 99 });
    await page.settle();
    http.expectOne(page.availabilityUrl()).flush({ isAvailable: true, reason: null });
    await page.settle();

    expect(page.text()).toContain('Request takeover');
  });

  it('asks for the row, then checks again whether it can still be asked for', async () => {
    http.expectOne(page.availabilityUrl()).flush({ isAvailable: true, reason: null });
    await page.settle();

    page.button()?.click();
    await page.settle();

    expect(requested).toEqual([page.row.id]);
    http.expectOne(page.availabilityUrl()).flush({ isAvailable: true, reason: null });
    await page.settle();
  });

  it('shows the countdown and Withdraw while a request waits', async () => {
    http.expectOne(page.availabilityUrl()).flush({ isAvailable: true, reason: null });
    outgoing.set(
      new Map([[page.row.id, RowTakeoverFixture.fixtureTakeover({ rowId: page.row.id, holderName: 'Engineer Two' })]]),
    );
    await page.settle();

    expect(page.text()).toContain('Asked Engineer Two · yours in 42s if unanswered');
    expect(page.text()).toContain('Withdraw');
  });
});
