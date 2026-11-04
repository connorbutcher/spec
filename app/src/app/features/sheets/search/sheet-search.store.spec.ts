import { computed, signal, WritableSignal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { CellType } from '../../templates/models/cell-type.model';
import { SheetCell } from '../models/sheet-cell.model';
import { Sheet } from '../models/sheet.model';
import { SheetIndexUtil } from '../layout/sheet-index.util';
import { SheetAnchorUtil } from '../anchors/sheet-anchor.util';
import { SheetSearchStore } from './sheet-search.store';
import { SheetStructureFixture } from '../data/sheet-structure.fixture';
import { SheetStore } from '../sheet.store';

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

class SheetSearchStorePage {
  public cells!: SheetCell[];

  /** A sheet whose header row holds these texts, from the left. */
  public withValues(values: string[]): Sheet {
    const table = SheetStructureFixture.fixtureTable();
    this.cells = table.sections.find((section) => section.role === 'Header')?.rows[0]
      .cells as SheetCell[];
    values.forEach((value, i) => {
      this.cells[i].textValue = value;
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
}

describe('SheetSearchStore', () => {
  let page: SheetSearchStorePage;

  beforeEach(() => {
    page = new SheetSearchStorePage();
  });

  let search: SheetSearchStore;
  let sheet: WritableSignal<Sheet | null>;
  beforeEach(() => {
    sheet = signal<Sheet | null>(page.withValues(['Valve', 'Valve seat', 'Pin', 'valve guide']));
    TestBed.configureTestingModule({
      providers: [
        SheetSearchStore,
        {
          provide: SheetStore,
          useValue: {
            sheet,
            index: computed(() => SheetIndexUtil.buildSheetIndex(sheet())),
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
    expect(search.activeKey()).toBe(SheetAnchorUtil.anchorKey('cell', page.cells[0].publicId));
    expect(search.countLabel()).toBe('1 of 3');
    expect(search.isMatch(SheetAnchorUtil.anchorKey('cell', page.cells[2].publicId))).toBe(false);
  });

  it('steps forwards and backwards, going round at either end', () => {
    search.query.set('valve');

    search.step(-1);
    expect(search.activeKey()).toBe(SheetAnchorUtil.anchorKey('cell', page.cells[3].publicId));
    search.step(1);
    search.step(1);
    expect(search.countLabel()).toBe('2 of 3');
  });

  it('stays on the current match while it still matches, and moves to the first when it does not', () => {
    search.query.set('valve');
    search.step(1);

    search.query.set('valve s');
    expect(search.activeKey()).toBe(SheetAnchorUtil.anchorKey('cell', page.cells[1].publicId));

    search.query.set('valve g');
    expect(search.activeKey()).toBe(SheetAnchorUtil.anchorKey('cell', page.cells[3].publicId));
  });

  it('follows the sheet when it changes', () => {
    search.query.set('pin');
    expect(search.matches().length).toBe(1);

    sheet.set(page.withValues(['Pin', 'Pin bore', 'Pin', 'Ring']));
    expect(search.matches().length).toBe(3);
  });

  it('has nothing current once cleared', () => {
    search.query.set('valve');
    search.clear();

    expect(search.activeKey()).toBeNull();
    expect(search.hasQuery()).toBe(false);
  });
});
