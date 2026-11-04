import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationRef, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { CurrentUserStore } from '../../core/auth/current-user.store';
import { Sheet } from './models/sheet.model';
import { RowCheckoutStore } from './checkouts/row-checkout.store';
import { SheetStructureFixture } from './data/sheet-structure.fixture';
import { SheetStore } from './sheet.store';
import { SheetResponseFixture } from './data/sheet-response.fixture';

const SHEET_URL = '/api/phases/1/sheets/2';

class SheetStoreWithRowCheckoutsPage {
  public async settle(): Promise<void> {
    TestBed.inject(ApplicationRef).tick();
    await new Promise((resolve) => setTimeout(resolve));
    TestBed.inject(ApplicationRef).tick();
  }
}

/** How the store shows rows people are in but have not changed, which the server's sheet leaves out. */
describe('SheetStore with row checkouts', () => {
  let page: SheetStoreWithRowCheckoutsPage;

  beforeEach(() => {
    page = new SheetStoreWithRowCheckoutsPage();
  });

  let store: SheetStore;
  let checkouts: RowCheckoutStore;
  let sheet: Sheet;
  beforeEach(async () => {
    sheet = {
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
      tables: [SheetStructureFixture.fixtureTable()],
      availableTemplates: [],
      linkedSources: [],
    };
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        RowCheckoutStore,
        SheetStore,
        { provide: CurrentUserStore, useValue: { user: signal({ id: 1 }) } },
        {
          provide: ActivatedRoute,
          useValue: { paramMap: of(convertToParamMap({ phaseId: '1', sheetTypeId: '2' })) },
        },
      ],
    });
    store = TestBed.inject(SheetStore);
    checkouts = TestBed.inject(RowCheckoutStore);
    const http = TestBed.inject(HttpTestingController);

    await page.settle();
    http.expectOne('/api/cell-types').flush([]);
    http.expectOne(SHEET_URL).flush(SheetResponseFixture.sheetResponse(sheet));
    await page.settle();
  });

  it('shows the sheet as the server sent it while nobody is in a row', () => {
    expect(store.sheet()).toEqual(sheet);
  });

  it('shows a row someone else is in as locked to them, in the sheet and in the index', () => {
    const row = sheet.tables[0].sections[1].rows[0];

    checkouts.set([{ rowId: row.id, userId: 2, userName: 'Engineer Two' }]);

    const lock = { userId: 2, userName: 'Engineer Two', isMine: false, isCheckout: true };
    expect(store.sheet()?.tables[0].sections[1].rows[0].lock).toEqual(lock);
    expect(store.index().rows.get(row.id)?.lock).toEqual(lock);
  });

  it('only re-makes the row that changed hands, and puts it back when they leave', () => {
    const before = store.sheet()!;
    const header = before.tables[0].sections[0];
    const row = before.tables[0].sections[1].rows[0];

    checkouts.set([{ rowId: row.id, userId: 2, userName: 'Engineer Two' }]);
    const during = store.sheet()!;

    expect(during.tables[0].sections[0]).toBe(header);
    expect(during.tables[0].sections[1].rows[0].cells).toBe(row.cells);

    checkouts.set([]);
    expect(store.sheet()?.tables[0].sections[1].rows[0].lock).toBeNull();
  });

  it('keeps the same objects when the server repeats the same list', () => {
    const row = sheet.tables[0].sections[1].rows[0];
    checkouts.set([{ rowId: row.id, userId: 2, userName: 'Engineer Two' }]);
    const first = store.sheet();

    checkouts.set([{ rowId: row.id, userId: 2, userName: 'Engineer Two' }]);

    expect(store.sheet()).toBe(first);
  });
});
