import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { SheetRow } from '../models/sheet-row.model';
import { Sheet } from '../models/sheet.model';
import { fixtureTable } from '../sheet-structure.fixture';
import { SheetStore } from '../sheet.store';
import { SheetRowStatusCell } from './sheet-row-status-cell';
import { sheetResponse } from '../sheet-response.fixture';

describe('SheetRowStatusCell', () => {
  let fixture: ComponentFixture<SheetRowStatusCell>;
  let store: SheetStore;
  let http: HttpTestingController;
  let sheet: Sheet;
  let row: SheetRow;

  function host(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function text(): string {
    return [...host().querySelectorAll('.version, p-tag, .state')]
      .map((part) => (part.textContent ?? '').replace(/\s+/g, ' ').trim())
      .join(' ');
  }

  async function settle(): Promise<void> {
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  /** Renders the status of the fixture table's limits row, after `change` has adjusted it. */
  async function render(change: (row: SheetRow) => void = () => undefined): Promise<void> {
    const table = fixtureTable();
    row = table.sections[0].sections[0].rows[0];
    row.lastChange = { versionNumber: 7, atUtc: '2026-09-12T10:00:00Z', userName: 'A. Smith' };
    change(row);
    sheet = {
      id: 5,
      publicId: 'sheet-5',
      phaseId: 1,
      sheetTypeId: 2,
      isLive: true,
      viewedVersionNumber: null,
      viewedAsOfUtc: null,
      latestVersionNumber: 9,
      myDraftCount: 0,
      otherDrafts: [],
      versions: [],
      tables: [table],
      availableTemplates: [],
      linkedSources: [],
    };

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
    http = TestBed.inject(HttpTestingController);
    store = TestBed.inject(SheetStore);

    fixture = TestBed.createComponent(SheetRowStatusCell);
    fixture.componentRef.setInput('rowId', row.id);
    fixture.detectChanges();
    http.expectOne('/api/cell-types').flush([]);
    http.expectOne('/api/phases/1/sheets/2').flush(sheetResponse(sheet));
    await settle();
  }

  it('shows only the version of a row nobody has', async () => {
    await render();

    expect(text()).toBe('v7');
    expect(host().getAttribute('aria-label')).toBe('Status: v7 · Published');
    expect(host().classList.contains('mine')).toBe(false);
    expect(host().classList.contains('theirs')).toBe(false);
  });

  it('says whose draft a changed row is, and tints it as theirs', async () => {
    await render((changed) => {
      changed.lock = { userId: 2, userName: 'A. Smith', isMine: false };
    });

    expect(text()).toBe('v7 Draft · A. Smith');
    expect(host().querySelector('[data-tone="theirs"] .pi-pencil')).not.toBeNull();
    expect(host().classList.contains('theirs')).toBe(true);
  });

  it('says a row someone is only in is checked out', async () => {
    await render((changed) => {
      changed.lock = { userId: 2, userName: 'A. Smith', isMine: false, isCheckout: true };
    });

    expect(text()).toBe('v7 Checked out · A. Smith');
  });

  it('marks a row that has never been published as New, and the viewer as You', async () => {
    await render((changed) => {
      changed.isPending = true;
      changed.lastChange = null;
      changed.lock = { userId: 1, userName: 'Me Myself', isMine: true };
    });

    expect(text()).toBe('New Draft · You');
    expect(host().classList.contains('mine')).toBe(true);
  });

  it('writes the version in bold when the row changed after the compared version', async () => {
    await render();
    expect(host().querySelector('.version.changed')).toBeNull();

    store.setChangesSince(6);
    await settle();
    expect(host().querySelector('.version.changed')).not.toBeNull();

    store.setChangesSince(7);
    await settle();
    expect(host().querySelector('.version.changed')).toBeNull();
  });

  it('shows my drafts as Publishing for as long as a publish of them runs', async () => {
    await render((changed) => {
      changed.lock = { userId: 1, userName: 'Me Myself', isMine: true };
    });

    const publishing = store.publish(null);
    await settle();
    expect(text()).toBe('v7 Publishing');

    const published = structuredClone(sheet);
    published.tables[0].sections[0].sections[0].rows[0].lock = null;
    http.expectOne('/api/sheets/5/publish').flush(sheetResponse(published));
    await publishing;
    await settle();
    expect(store.publishing()).toBeNull();
    expect(text()).toBe('v7');
  });

  it('selects its row when clicked', async () => {
    await render();

    host().click();

    expect(store.selection()?.rowId).toBe(row.id);
  });
});
