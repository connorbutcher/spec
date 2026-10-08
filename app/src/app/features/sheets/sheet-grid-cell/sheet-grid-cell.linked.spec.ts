import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { CellLayout } from '../../templates/models/cell-layout.model';
import { CellType } from '../../templates/models/cell-type.model';
import { SheetCell } from '../models/sheet-cell.model';
import { Sheet } from '../models/sheet.model';
import { layoutSheetTable } from '../sheet-layout.util';
import { fixtureTable } from '../sheet-structure.fixture';
import { SheetStore } from '../sheet.store';
import { SheetGridCell } from './sheet-grid-cell';

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

describe('SheetGridCell, for a linked dropdown', () => {
  let fixture: ComponentFixture<SheetGridCell>;
  let http: HttpTestingController;

  function host(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  /**
   * Renders the first cell of the fixture table's header row as a linked dropdown, after `change` has
   * adjusted it. The table offers one column, "Part number", holding P-1001 and P-1002.
   */
  async function render(change: (cell: SheetCell, sheet: Sheet) => void): Promise<SheetCell> {
    const table = fixtureTable();
    table.title = 'Piston parts';
    table.linkableColumns = [{ templateCellId: PART_NUMBER, label: 'Part number' }];
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
        { sheetTableId: table.id, templateCellId: PART_NUMBER, options: ['P-1001', 'P-1002'] },
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
    http = TestBed.inject(HttpTestingController);
    const layout = layoutSheetTable(table).sections[0].cells[0] as CellLayout;

    fixture = TestBed.createComponent(SheetGridCell);
    fixture.componentRef.setInput('layout', layout);
    fixture.detectChanges();
    http.expectOne('/api/cell-types').flush([LINKED_TYPE]);
    http.expectOne('/api/phases/1/sheets/2').flush(sheet);
    await settle();
    return cell;
  }

  async function settle(): Promise<void> {
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  function pointAtPartNumbers(cell: SheetCell): void {
    cell.settings = {
      kind: 'LinkedDropdown',
      sourceSheetTableId: 1,
      sourceTemplateCellId: PART_NUMBER,
    };
  }

  it('says so when nobody has chosen where its choices come from', async () => {
    await render(() => undefined);

    expect(host().textContent?.trim()).toBe('No source chosen');
    expect(host().querySelector('.warning')).toBeNull();
  });

  it('shows the value picked from its column, with nothing to flag', async () => {
    await render((cell) => {
      pointAtPartNumbers(cell);
      cell.textValue = 'P-1002';
    });

    expect(host().textContent?.trim()).toBe('P-1002');
    expect(host().querySelector('.warning')).toBeNull();
  });

  it('keeps a value its column no longer has, and flags it', async () => {
    await render((cell) => {
      pointAtPartNumbers(cell);
      cell.textValue = 'P-0999';
    });

    expect(host().textContent?.trim()).toBe('P-0999');
    expect(host().querySelector('.warning')?.getAttribute('aria-label')).toContain(
      '"P-0999" is no longer in Piston parts › Part number',
    );
  });

  it('says its source has gone when the table it was pointed at is no longer on the sheet', async () => {
    await render((cell) => {
      cell.settings = {
        kind: 'LinkedDropdown',
        sourceSheetTableId: 99,
        sourceTemplateCellId: PART_NUMBER,
      };
    });

    expect(host().textContent?.trim()).toBe('Source removed');
    expect(host().querySelector('.warning')?.getAttribute('aria-label')).toContain(
      'no longer on the sheet',
    );
  });

  it('has a settings button beside its control, and saves what is chosen there with the row', async () => {
    const cell = await render(() => undefined);
    host().focus();
    await settle();

    expect(host().querySelector('app-sheet-cell-settings button')).not.toBeNull();
    expect(host().querySelector('.p-select')?.classList).toContain('p-disabled');

    fixture.componentInstance.saveSettings({
      sheetCellId: cell.id,
      settings: { kind: 'LinkedDropdown', sourceSheetTableId: 1, sourceTemplateCellId: PART_NUMBER },
    });
    await settle();

    const rowId = fixture.componentInstance.row()?.id;
    const request = http.expectOne(`/api/sheet-rows/${rowId}/cell-settings`);
    expect(request.request.body.settings[0].settings.sourceTemplateCellId).toBe(PART_NUMBER);
  });
});
