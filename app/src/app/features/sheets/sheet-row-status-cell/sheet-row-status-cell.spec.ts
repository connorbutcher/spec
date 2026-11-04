import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { SheetRow } from '../models/sheet-row.model';
import { Sheet } from '../models/sheet.model';
import { SheetStructureFixture } from '../sheet-structure.fixture';
import { SheetStore } from '../sheet.store';
import { SheetRowStatusCell } from './sheet-row-status-cell';
import { SheetResponseFixture } from '../sheet-response.fixture';

class SheetRowStatusCellPage {
  public fixture!: ComponentFixture<SheetRowStatusCell>;
  public store!: SheetStore;
  public http!: HttpTestingController;
  public sheet!: Sheet;
  public row!: SheetRow;

  public host(): HTMLElement {
    return this.fixture.nativeElement as HTMLElement;
  }

  public text(): string {
    return [...this.host().querySelectorAll('.version, p-tag, .state')]
      .map((part) => (part.textContent ?? '').replace(/\s+/g, ' ').trim())
      .join(' ');
  }

  public async settle(): Promise<void> {
    await new Promise((resolve) => setTimeout(resolve));
    await this.fixture.whenStable();
  }

  /** Renders the status of the fixture table's limits row, after `change` has adjusted it. */
  public async render(change: (row: SheetRow) => void = () => undefined): Promise<void> {
    const table = SheetStructureFixture.fixtureTable();
    this.row = table.sections[0].sections[0].rows[0];
    this.row.lastChange = { versionNumber: 7, atUtc: '2026-09-12T10:00:00Z', userName: 'A. Smith' };
    change(this.row);
    this.sheet = {
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
    this.http = TestBed.inject(HttpTestingController);
    this.store = TestBed.inject(SheetStore);

    this.fixture = TestBed.createComponent(SheetRowStatusCell);
    this.fixture.componentRef.setInput('rowId', this.row.id);
    this.fixture.detectChanges();
    this.http.expectOne('/api/cell-types').flush([]);
    this.http.expectOne('/api/phases/1/sheets/2').flush(SheetResponseFixture.sheetResponse(this.sheet));
    await this.settle();
  }
}

describe('SheetRowStatusCell', () => {
  let page: SheetRowStatusCellPage;

  beforeEach(() => {
    page = new SheetRowStatusCellPage();
  });

  it('shows only the version of a row nobody has', async () => {
    await page.render();

    expect(page.text()).toBe('v7');
    expect(page.host().getAttribute('aria-label')).toBe('Status: v7 · Published');
    expect(page.host().classList.contains('mine')).toBe(false);
    expect(page.host().classList.contains('theirs')).toBe(false);
  });

  it('says whose draft a changed row is, and tints it as theirs', async () => {
    await page.render((changed) => {
      changed.lock = { userId: 2, userName: 'A. Smith', isMine: false };
    });

    expect(page.text()).toBe('v7 Draft · A. Smith');
    expect(page.host().querySelector('[data-tone="theirs"] .pi-pencil')).not.toBeNull();
    expect(page.host().classList.contains('theirs')).toBe(true);
  });

  it('says a row someone is only in is checked out', async () => {
    await page.render((changed) => {
      changed.lock = { userId: 2, userName: 'A. Smith', isMine: false, isCheckout: true };
    });

    expect(page.text()).toBe('v7 Checked out · A. Smith');
  });

  it('marks a row that has never been published as New, and the viewer as You', async () => {
    await page.render((changed) => {
      changed.isPending = true;
      changed.lastChange = null;
      changed.lock = { userId: 1, userName: 'Me Myself', isMine: true };
    });

    expect(page.text()).toBe('New Draft · You');
    expect(page.host().classList.contains('mine')).toBe(true);
  });

  it('writes the version in bold when the row changed after the compared version', async () => {
    await page.render();
    expect(page.host().querySelector('.version.changed')).toBeNull();

    page.store.setChangesSince(6);
    await page.settle();
    expect(page.host().querySelector('.version.changed')).not.toBeNull();

    page.store.setChangesSince(7);
    await page.settle();
    expect(page.host().querySelector('.version.changed')).toBeNull();
  });

  it('shows my drafts as Publishing for as long as a publish of them runs', async () => {
    await page.render((changed) => {
      changed.lock = { userId: 1, userName: 'Me Myself', isMine: true };
    });

    const publishing = page.store.publish(null);
    await page.settle();
    expect(page.text()).toBe('v7 Publishing');

    const published = structuredClone(page.sheet);
    published.tables[0].sections[0].sections[0].rows[0].lock = null;
    page.http.expectOne('/api/sheets/5/publish').flush(SheetResponseFixture.sheetResponse(published));
    await publishing;
    await page.settle();
    expect(page.store.publishing()).toBeNull();
    expect(page.text()).toBe('v7');
  });

  it('selects its row when clicked', async () => {
    await page.render();

    page.host().click();

    expect(page.store.selection()?.rowId).toBe(page.row.id);
  });
});
