import { computed, signal, WritableSignal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { CellType } from '../templates/models/cell-type.model';
import { SheetCell } from './models/sheet-cell.model';
import { Sheet } from './models/sheet.model';
import { buildSheetIndex } from './sheet-index.util';
import { cellSearchKey } from './sheet-search.util';
import { SheetSearchStore } from './sheet-search.store';
import { fixtureTable } from './sheet-structure.fixture';
import { SheetStore } from './sheet.store';

const TEXT: CellType = {
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

describe('SheetSearchStore', () => {
  let search: SheetSearchStore;
  let sheet: WritableSignal<Sheet | null>;
  let cells: SheetCell[];

  /** A sheet whose header row holds these texts, from the left. */
  function withValues(values: string[]): Sheet {
    const table = fixtureTable();
    cells = table.sections.find((section) => section.role === 'Header')?.rows[0]
      .cells as SheetCell[];
    values.forEach((value, i) => {
      cells[i].textValue = value;
    });
    return {
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
  }

  beforeEach(() => {
    sheet = signal<Sheet | null>(withValues(['Valve', 'Valve seat', 'Pin', 'valve guide']));
    TestBed.configureTestingModule({
      providers: [
        SheetSearchStore,
        {
          provide: SheetStore,
          useValue: {
            sheet,
            index: computed(() => buildSheetIndex(sheet())),
            cellTypes: signal(new Map([[TEXT.id, TEXT]])),
          },
        },
      ],
    });
    search = TestBed.inject(SheetSearchStore);
  });

  it('starts on the first match and counts them', () => {
    search.query.set('valve');

    expect(search.matches().length).toBe(3);
    expect(search.activeKey()).toBe(cellSearchKey(cells[0].id));
    expect(search.countLabel()).toBe('1 of 3');
    expect(search.isMatch(cellSearchKey(cells[2].id))).toBe(false);
  });

  it('steps forwards and backwards, going round at either end', () => {
    search.query.set('valve');

    search.step(-1);
    expect(search.activeKey()).toBe(cellSearchKey(cells[3].id));
    search.step(1);
    search.step(1);
    expect(search.countLabel()).toBe('2 of 3');
  });

  it('stays on the current match while it still matches, and moves to the first when it does not', () => {
    search.query.set('valve');
    search.step(1);

    search.query.set('valve s');
    expect(search.activeKey()).toBe(cellSearchKey(cells[1].id));

    search.query.set('valve g');
    expect(search.activeKey()).toBe(cellSearchKey(cells[3].id));
  });

  it('follows the sheet when it changes', () => {
    search.query.set('pin');
    expect(search.matches().length).toBe(1);

    sheet.set(withValues(['Pin', 'Pin bore', 'Pin', 'Ring']));
    expect(search.matches().length).toBe(3);
  });

  it('has nothing current once cleared', () => {
    search.query.set('valve');
    search.clear();

    expect(search.activeKey()).toBeNull();
    expect(search.hasQuery()).toBe(false);
  });
});
