import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { CellLayout } from '../../templates/models/cell-layout.model';
import { CellType } from '../../templates/models/cell-type.model';
import { SheetRow } from '../models/sheet-row.model';
import { Sheet } from '../models/sheet.model';
import { SheetIndexUtil } from '../layout/sheet-index.util';
import { SheetLayoutUtil } from '../layout/sheet-layout.util';
import { SheetStructureFixture } from '../data/sheet-structure.fixture';
import { SheetStore } from '../sheet.store';
import { SheetGridCell } from './sheet-grid-cell';
import { SheetResponseFixture } from '../data/sheet-response.fixture';

const TEXT_TYPE: CellType = {
  id: 1,
  name: 'Text',
  kind: 'Text',
  description: null,
  displayOrder: 1,
  configuration: { kind: 'Text' },
  style: {},
  options: [],
  usageCount: 0,
};

class SheetGridCellPage {
  public fixture!: ComponentFixture<SheetGridCell>;

  public host(): HTMLElement {
    return this.fixture.nativeElement as HTMLElement;
  }

  public box(): HTMLTextAreaElement | null {
    return this.host().querySelector('textarea');
  }

  /** Renders the first cell of the fixture table's header row, after `change` has adjusted the sheet. */
  public async render(change: (sheet: Sheet, row: SheetRow) => void = () => undefined): Promise<void> {
    const table = SheetStructureFixture.fixtureTable();
    const header = table.sections.find((section) => section.role === 'Header');
    const row = header?.rows[0] as SheetRow;
    row.cells[0].value = 'P-1001';
    const sheet: Sheet = {
      id: 5,
      publicId: 'sheet-5',
      phaseId: 1,
      sheetTypeId: 2,
      isLive: true,
      viewedVersionNumber: null,
      viewedAsOfUtc: null,
      latestVersionNumber: null,
      myDraftCount: 0,
      otherDrafts: [],
      versions: [],
      tables: [table],
      availableTemplates: [],
      linkedSources: [],
    };
    change(sheet, row);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        SheetStore,
        {
          provide: ActivatedRoute,
          useValue: {
            paramMap: of(convertToParamMap({ phaseId: '1', sheetTypeId: '2' })),
            queryParamMap: of(convertToParamMap({})),
          },
        },
      ],
    });
    const http = TestBed.inject(HttpTestingController);
    const layout = SheetLayoutUtil.layoutSheetTable(table).sections[0].cells[0] as CellLayout;

    this.fixture = TestBed.createComponent(SheetGridCell);
    this.fixture.componentRef.setInput('layout', layout);
    this.fixture.detectChanges();
    http.expectOne('/api/cell-types').flush([TEXT_TYPE]);
    http.expectOne('/api/phases/1/sheets/2').flush(SheetResponseFixture.sheetResponse(sheet));
    await this.settle();
    this.handDown(layout);
    await this.settle();
  }

  /** Gives the cell its sheet cell, row and tone, as the section it sits in does. */
  public handDown(layout: CellLayout): void {
    const index = TestBed.inject(SheetStore).index();
    this.fixture.componentRef.setInput('cell', index.cells.get(layout.cell.id) ?? null);
    this.fixture.componentRef.setInput('row', index.rows.get(layout.rowId) ?? null);
    this.fixture.componentRef.setInput('tone', SheetIndexUtil.sectionTone(index, layout.rowId));
  }

  public async settle(): Promise<void> {
    await new Promise((resolve) => setTimeout(resolve));
    await this.fixture.whenStable();
  }
}

describe('SheetGridCell', () => {
  let page: SheetGridCellPage;

  beforeEach(() => {
    page = new SheetGridCellPage();
  });

  let outside: HTMLButtonElement;
  beforeEach(() => {
    outside = document.createElement('button');
    document.body.appendChild(outside);
  });

  afterEach(() => {
    outside.remove();
  });

  it('shows its value as text, with no control, and can be tabbed to', async () => {
    await page.render();

    expect(page.host().textContent?.trim()).toBe('P-1001');
    expect(page.box()).toBeNull();
    expect(page.host().getAttribute('tabindex')).toBe('0');
  });

  it('swaps in its control and moves into it when it is focused', async () => {
    await page.render();

    page.host().focus();
    await page.settle();

    expect(page.box()?.value).toBe('P-1001');
    expect(document.activeElement).toBe(page.box());
    expect(page.host().getAttribute('tabindex')).toBeNull();
  });

  it('swaps in its control when it is pressed', async () => {
    await page.render();

    page.host().dispatchEvent(new Event('pointerdown', { bubbles: true }));
    await page.settle();

    expect(page.box()).not.toBeNull();
  });

  it('goes back to text as soon as focus moves somewhere else', async () => {
    await page.render();
    page.host().focus();
    await page.settle();

    outside.focus();
    await page.settle();

    expect(page.box()).toBeNull();
    expect(page.host().textContent?.trim()).toBe('P-1001');
    expect(page.host().getAttribute('tabindex')).toBe('0');
  });

  it('goes back to text when something else on the page is pressed', async () => {
    await page.render();
    page.host().focus();
    await page.settle();

    outside.dispatchEvent(new Event('pointerdown', { bubbles: true }));
    await page.settle();

    expect(page.box()).toBeNull();
  });

  it('keeps its control while a panel the control opened is being used', async () => {
    await page.render();
    page.host().focus();
    await page.settle();
    const panel = document.createElement('div');
    panel.className = 'p-select-overlay';
    panel.appendChild(outside);
    document.body.appendChild(panel);

    outside.dispatchEvent(new Event('pointerdown', { bubbles: true }));
    await page.settle();

    expect(page.box()).not.toBeNull();
    panel.remove();
  });

  it('stays as text in a row someone else is editing', async () => {
    await page.render((_sheet, row) => {
      row.lock = { userId: 2, userName: 'A. Smith', isMine: false };
    });

    page.host().focus();
    await page.settle();

    expect(page.box()).toBeNull();
    expect(page.host().classList.contains('theirs')).toBe(true);
  });

  it('stays as text in a past version', async () => {
    await page.render((sheet) => {
      sheet.isLive = false;
    });

    page.host().focus();
    await page.settle();

    expect(page.box()).toBeNull();
    expect(page.host().getAttribute('tabindex')).toBeNull();
  });
});
