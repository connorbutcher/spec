import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { CellLayout } from '../../templates/models/cell-layout.model';
import { CellType } from '../../templates/models/cell-type.model';
import { SheetCell } from '../models/sheet-cell.model';
import { Sheet } from '../models/sheet.model';
import { SheetIndexUtil } from '../layout/sheet-index.util';
import { SheetLayoutUtil } from '../layout/sheet-layout.util';
import { SheetStructureFixture } from '../data/sheet-structure.fixture';
import { SheetStore } from '../sheet.store';
import { SheetGridCell } from './sheet-grid-cell';
import { SheetResponseFixture } from '../data/sheet-response.fixture';

const LINKED_TYPE: CellType = {
  id: 1,
  name: 'Linked dropdown',
  kind: 'LinkedDropdown',
  description: null,
  displayOrder: 1,
  configuration: { kind: 'LinkedDropdown' },
  style: {},
  options: [],
  usageCount: 0,
};

const PART_NUMBER = 15;
const PIN_OFFSET_ROW = 70;

class LinkedDropdownCellPage {
  public fixture!: ComponentFixture<SheetGridCell>;
  public http!: HttpTestingController;

  public host(): HTMLElement {
    return this.fixture.nativeElement as HTMLElement;
  }

  /**
   * Renders the first cell of the fixture table's header row as a linked dropdown, after `change` has
   * adjusted it. The table offers one column, "Part number", holding P-1001 and P-1002.
   */
  public async render(change: (cell: SheetCell, sheet: Sheet) => void): Promise<SheetCell> {
    const table = SheetStructureFixture.fixtureTable();
    table.title = 'Piston parts';
    table.linkableColumns = [{ templateCellId: PART_NUMBER, label: 'Part number' }];
    table.linkableRows = [{ sheetRowId: PIN_OFFSET_ROW, label: 'Pin offset (mm)' }];
    const header = table.sections.find((section) => section.role === 'Header');
    const cell = header?.rows[0].cells[0] as SheetCell;
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
      linkedSources: [
        {
          sheetTableId: table.id,
          templateCellId: PART_NUMBER,
          sheetRowId: null,
          options: ['P-1001', 'P-1002'],
        },
        {
          sheetTableId: table.id,
          templateCellId: null,
          sheetRowId: PIN_OFFSET_ROW,
          options: ['0.8', '0.85'],
        },
      ],
    };
    change(cell, sheet);

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
    const layout = SheetLayoutUtil.layoutSheetTable(table).sections[0].cells[0] as CellLayout;

    this.fixture = TestBed.createComponent(SheetGridCell);
    this.fixture.componentRef.setInput('layout', layout);
    this.fixture.detectChanges();
    this.http.expectOne('/api/cell-types').flush([LINKED_TYPE]);
    this.http.expectOne('/api/phases/1/sheets/2').flush(SheetResponseFixture.sheetResponse(sheet));
    await this.settle();
    this.handDown(layout);
    await this.settle();
    return cell;
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

  public pointAtPartNumbers(cell: SheetCell): void {
    cell.settings = {
      kind: 'LinkedDropdown',
      sourceSheetTableId: 1,
      sourceTemplateCellId: PART_NUMBER,
    };
  }
}

describe('SheetGridCell, for a linked dropdown', () => {
  let page: LinkedDropdownCellPage;

  beforeEach(() => {
    page = new LinkedDropdownCellPage();
  });

  it('says so when nobody has chosen where its choices come from', async () => {
    await page.render(() => undefined);

    expect(page.host().textContent?.trim()).toBe('No source chosen');
    expect(page.host().querySelector('.warning')).toBeNull();
  });

  it('shows the value picked from its column, with nothing to flag', async () => {
    await page.render((cell) => {
      page.pointAtPartNumbers(cell);
      cell.textValue = 'P-1002';
    });

    expect(page.host().textContent?.trim()).toBe('P-1002');
    expect(page.host().querySelector('.warning')).toBeNull();
  });

  it('pointed at a row, shows a value of that row and flags one the row no longer has', async () => {
    await page.render((cell) => {
      cell.settings = {
        kind: 'LinkedDropdown',
        sourceSheetTableId: 1,
        sourceTemplateCellId: null,
        sourceSheetRowId: PIN_OFFSET_ROW,
      };
      cell.textValue = '0.9';
    });

    expect(page.host().textContent?.trim()).toBe('0.9');
    expect(page.host().querySelector('.warning')?.getAttribute('aria-label')).toContain(
      '"0.9" is no longer in Piston parts › Pin offset (mm)',
    );
  });

  it('keeps a value its column no longer has, and flags it', async () => {
    await page.render((cell) => {
      page.pointAtPartNumbers(cell);
      cell.textValue = 'P-0999';
    });

    expect(page.host().textContent?.trim()).toBe('P-0999');
    expect(page.host().querySelector('.warning')?.getAttribute('aria-label')).toContain(
      '"P-0999" is no longer in Piston parts › Part number',
    );
  });

  it('says its source has gone when the table it was pointed at is no longer on the sheet', async () => {
    await page.render((cell) => {
      cell.settings = {
        kind: 'LinkedDropdown',
        sourceSheetTableId: 99,
        sourceTemplateCellId: PART_NUMBER,
      };
    });

    expect(page.host().textContent?.trim()).toBe('Source removed');
    expect(page.host().querySelector('.warning')?.getAttribute('aria-label')).toContain(
      'no longer on the sheet',
    );
  });

  it('has a settings button beside its control, and saves what is chosen there with the row', async () => {
    const cell = await page.render(() => undefined);
    page.host().focus();
    await page.settle();

    expect(page.host().querySelector('app-sheet-cell-settings button')).not.toBeNull();
    expect(page.host().querySelector('.p-select')?.classList).toContain('p-disabled');

    page.fixture.componentInstance.saveSettings({
      sheetCellId: cell.id,
      settings: {
        kind: 'LinkedDropdown',
        sourceSheetTableId: 1,
        sourceTemplateCellId: PART_NUMBER,
      },
    });
    await page.settle();

    const rowId = page.fixture.componentInstance.row()?.id;
    const request = page.http.expectOne(`/api/sheet-rows/${rowId}/cell-settings`);
    expect(request.request.body.settings[0].settings.sourceTemplateCellId).toBe(PART_NUMBER);
  });
});
