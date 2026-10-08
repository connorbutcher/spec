import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { Sheet } from './models/sheet.model';
import { fixtureTable } from './sheet-structure.fixture';
import { SheetStore } from './sheet.store';

const SHEET_URL = '/api/phases/1/sheets/2';

function fixtureSheet(): Sheet {
  return {
    id: 5,
    publicId: 'sheet-5',
    phaseId: 1,
    sheetTypeId: 2,
    isLive: true,
    viewedVersionNumber: null,
    viewedAsOfUtc: null,
    latestVersionNumber: 2,
    myDraftCount: 0,
    otherDrafts: [],
    versions: [
      {
        versionNumber: 1,
        publishedAtUtc: '2026-09-01T09:00:00Z',
        publishedByUserId: 1,
        publishedByName: 'A',
        note: null,
      },
      {
        versionNumber: 2,
        publishedAtUtc: '2026-09-02T09:00:00Z',
        publishedByUserId: 1,
        publishedByName: 'A',
        note: null,
      },
    ],
    tables: [fixtureTable()],
    availableTemplates: [],
    linkedSources: [],
  };
}

/** A copy as the API would send it back: equal data, all new objects. */
function fromServer(sheet: Sheet, change: (copy: Sheet) => void = () => undefined): Sheet {
  const copy = structuredClone(sheet);
  change(copy);
  return copy;
}

describe('SheetStore', () => {
  let store: SheetStore;
  let http: HttpTestingController;
  let sheet: Sheet;

  /** Lets resources send their requests and promise chains run on. */
  async function settle(): Promise<void> {
    TestBed.inject(ApplicationRef).tick();
    await new Promise((resolve) => setTimeout(resolve));
    TestBed.inject(ApplicationRef).tick();
  }

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        SheetStore,
        {
          provide: ActivatedRoute,
          useValue: { paramMap: of(convertToParamMap({ phaseId: '1', sheetTypeId: '2' })) },
        },
      ],
    });
    store = TestBed.inject(SheetStore);
    http = TestBed.inject(HttpTestingController);
    sheet = fixtureSheet();

    await settle();
    http.expectOne('/api/cell-types').flush([]);
    http.expectOne(SHEET_URL).flush(sheet);
    await settle();
  });

  afterEach(() => {
    http.verify();
  });

  it('loads the live sheet named in the route', () => {
    expect(store.sheet()).toEqual(sheet);
    expect(store.canEdit()).toBe(true);
    expect(store.versions().map((version) => version.versionNumber)).toEqual([2, 1]);
  });

  it('shows no sheet without its cell types, and retries them with the sheet', async () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        SheetStore,
        {
          provide: ActivatedRoute,
          useValue: { paramMap: of(convertToParamMap({ phaseId: '1', sheetTypeId: '2' })) },
        },
      ],
    });
    store = TestBed.inject(SheetStore);
    http = TestBed.inject(HttpTestingController);
    await settle();
    http.expectOne('/api/cell-types').flush(null, { status: 500, statusText: 'Server Error' });
    http.expectOne(SHEET_URL).flush(sheet);
    await settle();

    expect(store.sheet()).toBeNull();
    expect(store.hasError()).toBe(true);

    store.reload();
    await settle();
    http.expectOne('/api/cell-types').flush([]);
    http.expectOne(SHEET_URL).flush(sheet);
    await settle();

    expect(store.sheet()).toEqual(sheet);
    expect(store.hasError()).toBe(false);
  });

  it('reads a past version when one is chosen, and clears the selection', async () => {
    store.selectRow(sheet.tables[0].sections[0].rows[0].id);

    store.setView({ version: 1 });
    await settle();
    http.expectOne(`${SHEET_URL}?version=1`).flush({ ...sheet, isLive: false });
    await settle();

    expect(store.canEdit()).toBe(false);
    expect(store.selection()).toBeNull();
  });

  it('has one cell in its control at a time, and none after the view changes', async () => {
    store.edit(11);
    store.edit(12);
    expect(store.editingCellId()).toBe(12);

    store.stopEditing(11);
    expect(store.editingCellId()).toBe(12);
    store.stopEditing(12);
    expect(store.editingCellId()).toBeNull();
    store.edit(12);

    store.setView({ version: 1 });
    expect(store.editingCellId()).toBeNull();

    await settle();
    http.expectOne(`${SHEET_URL}?version=1`).flush({ ...sheet, isLive: false });
    await settle();
  });

  it('selects a row together with its section and table', () => {
    const section = sheet.tables[0].sections[0];

    store.selectRow(section.rows[0].id);

    expect(store.selection()).toEqual({
      tableId: sheet.tables[0].id,
      sectionId: section.id,
      rowId: section.rows[0].id,
    });
  });

  it('keeps the objects a change did not touch, and replaces the saved cell', async () => {
    const [group, header] = store.sheet()?.tables[0].sections ?? [];
    const row = header.rows[0];
    const cell = row.cells[0];

    const saving = store.saveValues(row.id, [{ sheetCellId: cell.id, text: 'new' }]);
    await settle();
    const request = http.expectOne(`/api/sheet-rows/${row.id}/values`);
    expect(request.request.body).toEqual({ values: [{ sheetCellId: cell.id, text: 'new' }] });
    request.flush(
      fromServer(sheet, (copy) => {
        copy.tables[0].sections[1].rows[0].cells[0].textValue = 'new';
      }),
    );
    await saving;

    const after = store.sheet()?.tables[0].sections ?? [];
    expect(after[0]).toBe(group);
    expect(after[1]).not.toBe(header);
    expect(after[1].rows[0].cells[0].textValue).toBe('new');
    expect(after[1].rows[0].cells[1]).toBe(row.cells[1]);
  });

  it('saves the settings chosen for a cell on the sheet, and takes the cell the server sends back', async () => {
    const row = sheet.tables[0].sections[1].rows[0];
    const cell = row.cells[0];
    const settings = {
      kind: 'LinkedDropdown' as const,
      sourceSheetTableId: 1,
      sourceTemplateCellId: 15,
    };

    const saving = store.saveCellSettings(row.id, [{ sheetCellId: cell.id, settings }]);
    await settle();
    const request = http.expectOne(`/api/sheet-rows/${row.id}/cell-settings`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ settings: [{ sheetCellId: cell.id, settings }] });
    request.flush(
      fromServer(sheet, (copy) => {
        copy.tables[0].sections[1].rows[0].cells[0].settings = settings;
        copy.linkedSources = [{ sheetTableId: 1, templateCellId: 15, options: ['P-1001'] }];
      }),
    );
    await saving;

    const after = store.sheet();
    expect(after?.tables[0].sections[1].rows[0].cells[0].settings).toEqual(settings);
    expect(after?.tables[0].sections[1].rows[0].cells[1]).toBe(row.cells[1]);
    expect(after?.linkedSources[0].options).toEqual(['P-1001']);
  });

  it('replaces a saved cell even when the server sends back the value it had', async () => {
    const row = store.sheet()?.tables[0].sections[1].rows[0];
    const cell = row?.cells[0];

    const saving = store.saveValues(row?.id ?? -1, [{ sheetCellId: cell?.id ?? -1, text: 'x' }]);
    await settle();
    http.expectOne(`/api/sheet-rows/${row?.id}/values`).flush(fromServer(sheet));
    await saving;

    const after = store.sheet()?.tables[0].sections[1].rows[0];
    expect(after?.cells[0]).toEqual(cell);
    expect(after?.cells[0]).not.toBe(cell);
    expect(after?.cells[1]).toBe(row?.cells[1]);
  });

  it('sends changes one at a time, in the order they were made', async () => {
    const table = sheet.tables[0];

    const first = store.setTableTitle(table.id, 'One');
    const second = store.setTableTitle(table.id, 'Two');
    await settle();

    const firstRequest = http.expectOne(`/api/sheet-tables/${table.id}`);
    expect(firstRequest.request.body).toEqual({ title: 'One' });
    expect(store.isBusy()).toBe(true);
    firstRequest.flush(fromServer(sheet, (copy) => (copy.tables[0].title = 'One')));
    await first;
    await settle();

    const secondRequest = http.expectOne(`/api/sheet-tables/${table.id}`);
    expect(secondRequest.request.body).toEqual({ title: 'Two' });
    secondRequest.flush(fromServer(sheet, (copy) => (copy.tables[0].title = 'Two')));
    await second;

    expect(store.sheet()?.tables[0].title).toBe('Two');
    expect(store.isBusy()).toBe(false);
  });

  it('shows why a change failed and reloads the sheet', async () => {
    const table = sheet.tables[0];

    const renaming = store.setTableTitle(table.id, 'Nope');
    await settle();
    http
      .expectOne(`/api/sheet-tables/${table.id}`)
      .flush(
        { detail: 'Someone else is editing this table.' },
        { status: 409, statusText: 'Conflict' },
      );
    await renaming;
    await settle();

    expect(store.error()).toBe('Someone else is editing this table.');
    http.expectOne(SHEET_URL).flush(sheet);
    await settle();

    store.dismissError();
    expect(store.error()).toBeNull();
  });

  it('publishes my own changes, or everything on the sheet when asked', async () => {
    const mine = store.publish('First cut');
    await settle();
    const first = http.expectOne(`/api/sheets/${sheet.id}/publish`);
    expect(first.request.body).toEqual({ note: 'First cut', scope: 'Mine' });
    first.flush(fromServer(sheet));
    expect(await mine).toBe(true);

    const everything = store.publish(null, 'All');
    await settle();
    const second = http.expectOne(`/api/sheets/${sheet.id}/publish`);
    expect(second.request.body).toEqual({ note: null, scope: 'All' });
    second.flush(fromServer(sheet));
    expect(await everything).toBe(true);
  });

  it('selects what was just added', async () => {
    const table = sheet.tables[0];
    const group = table.sections[0];
    const added = { ...structuredClone(group.sections[0]), id: 9001, rows: [], sections: [] };

    const adding = store.addSection(table.id, 77, group.id);
    await settle();
    const request = http.expectOne(`/api/sheet-tables/${table.id}/sections`);
    expect(request.request.body).toEqual({ templateSectionId: 77, parentSheetSectionId: group.id });
    request.flush(fromServer(sheet, (copy) => copy.tables[0].sections[0].sections.push(added)));
    await adding;

    expect(store.selection()).toEqual({ tableId: table.id, sectionId: 9001, rowId: null });
  });

  it('drops a selected row that is no longer on the sheet, keeping its section selected', async () => {
    const section = sheet.tables[0].sections[1];
    const row = section.rows[0];
    store.selectRow(row.id);

    const removing = store.removeTable(-1);
    await settle();
    http
      .expectOne('/api/sheet-tables/-1')
      .flush(fromServer(sheet, (copy) => (copy.tables[0].sections[1].rows = [])));
    await removing;

    expect(store.selection()).toEqual({
      tableId: sheet.tables[0].id,
      sectionId: section.id,
      rowId: null,
    });
  });

  it("refreshes after someone else's change, keeping everything that did not change", async () => {
    const before = store.sheet()!;
    const header = before.tables[0].sections[0];
    const lockedRow = before.tables[0].sections[1].rows[0];

    store.refresh();
    await settle();
    http.expectOne(SHEET_URL).flush(
      fromServer(sheet, (copy) => {
        copy.tables[0].sections[1].rows[0].lock = { userId: 2, userName: 'B', isMine: false };
      }),
    );
    await settle();

    const after = store.sheet()!;
    expect(after.tables[0].sections[1].rows[0].lock?.userName).toBe('B');
    expect(after.tables[0].sections[1].rows[0].cells[0]).toBe(lockedRow.cells[0]);
    expect(after.tables[0].sections[0]).toBe(header);
  });

  it("waits for the user's own change before refreshing, and skips it if another is in flight", async () => {
    const row = sheet.tables[0].sections[1].rows[0];

    const saving = store.saveValues(row.id, [{ sheetCellId: row.cells[0].id, text: 'new' }]);
    store.refresh();
    await settle();

    // Only the save is out: the refresh has not been sent alongside it.
    http.expectOne(`/api/sheet-rows/${row.id}/values`).flush(fromServer(sheet));
    await saving;
    await settle();
    http.expectOne(SHEET_URL).flush(fromServer(sheet));
    await settle();

    expect(store.sheet()).toEqual(sheet);
  });

  it('marks only changes made after the version being compared against', () => {
    const change = { versionNumber: 2, atUtc: '2026-09-02T09:00:00Z', userName: 'A' };

    expect(store.markedChange(change)).toBeNull();

    store.setChangesSince(1);
    expect(store.markedChange(change)).toBe(change);
    expect(store.markedChange(null)).toBeNull();

    store.setChangesSince(2);
    expect(store.markedChange(change)).toBeNull();
  });
});
