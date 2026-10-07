import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { CellLayout } from '../../templates/models/cell-layout.model';
import { CellType } from '../../templates/models/cell-type.model';
import { SheetRow } from '../models/sheet-row.model';
import { Sheet } from '../models/sheet.model';
import { layoutSheetTable } from '../sheet-layout.util';
import { fixtureTable } from '../sheet-structure.fixture';
import { SheetStore } from '../sheet.store';
import { SheetGridCell } from './sheet-grid-cell';

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

describe('SheetGridCell', () => {
  let fixture: ComponentFixture<SheetGridCell>;
  let outside: HTMLButtonElement;

  function host(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function box(): HTMLInputElement | null {
    return host().querySelector('input');
  }

  /** Renders the first cell of the fixture table's header row, after `change` has adjusted the sheet. */
  async function render(
    change: (sheet: Sheet, row: SheetRow) => void = () => undefined,
  ): Promise<void> {
    const table = fixtureTable();
    const header = table.sections.find((section) => section.role === 'Header');
    const row = header?.rows[0] as SheetRow;
    row.cells[0].textValue = 'P-1001';
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
      versions: [],
      tables: [table],
      availableTemplates: [],
    };
    change(sheet, row);

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
    const http = TestBed.inject(HttpTestingController);
    const layout = layoutSheetTable(table).sections[0].cells[0] as CellLayout;

    fixture = TestBed.createComponent(SheetGridCell);
    fixture.componentRef.setInput('layout', layout);
    fixture.detectChanges();
    http.expectOne('/api/cell-types').flush([TEXT_TYPE]);
    http.expectOne('/api/phases/1/sheets/2').flush(sheet);
    await settle();
  }

  async function settle(): Promise<void> {
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  beforeEach(() => {
    outside = document.createElement('button');
    document.body.appendChild(outside);
  });

  afterEach(() => {
    outside.remove();
  });

  it('shows its value as text, with no control, and can be tabbed to', async () => {
    await render();

    expect(host().textContent?.trim()).toBe('P-1001');
    expect(box()).toBeNull();
    expect(host().getAttribute('tabindex')).toBe('0');
  });

  it('swaps in its control and moves into it when it is focused', async () => {
    await render();

    host().focus();
    await settle();

    expect(box()?.value).toBe('P-1001');
    expect(document.activeElement).toBe(box());
    expect(host().getAttribute('tabindex')).toBeNull();
  });

  it('swaps in its control when it is pressed', async () => {
    await render();

    host().dispatchEvent(new Event('pointerdown', { bubbles: true }));
    await settle();

    expect(box()).not.toBeNull();
  });

  it('goes back to text as soon as focus moves somewhere else', async () => {
    await render();
    host().focus();
    await settle();

    outside.focus();
    await settle();

    expect(box()).toBeNull();
    expect(host().textContent?.trim()).toBe('P-1001');
    expect(host().getAttribute('tabindex')).toBe('0');
  });

  it('goes back to text when something else on the page is pressed', async () => {
    await render();
    host().focus();
    await settle();

    outside.dispatchEvent(new Event('pointerdown', { bubbles: true }));
    await settle();

    expect(box()).toBeNull();
  });

  it('keeps its control while a panel the control opened is being used', async () => {
    await render();
    host().focus();
    await settle();
    const panel = document.createElement('div');
    panel.className = 'p-select-overlay';
    panel.appendChild(outside);
    document.body.appendChild(panel);

    outside.dispatchEvent(new Event('pointerdown', { bubbles: true }));
    await settle();

    expect(box()).not.toBeNull();
    panel.remove();
  });

  it('stays as text in a row someone else is editing', async () => {
    await render((_sheet, row) => {
      row.lock = { userId: 2, userName: 'A. Smith', isMine: false };
    });

    host().focus();
    await settle();

    expect(box()).toBeNull();
    expect(host().classList.contains('theirs')).toBe(true);
  });

  it('stays as text in a past version', async () => {
    await render((sheet) => {
      sheet.isLive = false;
    });

    host().focus();
    await settle();

    expect(box()).toBeNull();
  });
});
