import { computed, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, NavigationExtras, Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { SheetSelection } from './models/sheet-selection.model';
import { Sheet } from './models/sheet.model';
import { SheetAnchorService } from './sheet-anchor.service';
import { anchorKey } from './sheet-anchor.util';
import { buildSheetIndex } from './sheet-index.util';
import { fixtureTable } from './sheet-structure.fixture';
import { SheetStore } from './sheet.store';

describe('SheetAnchorService', () => {
  let fragment: BehaviorSubject<string | null>;
  let navigate: ReturnType<typeof vi.fn>;
  let selection: ReturnType<typeof signal<SheetSelection | null>>;
  let sheet: Sheet;
  let anchors: SheetAnchorService;

  function setUp(initialFragment: string | null): void {
    fragment = new BehaviorSubject<string | null>(initialFragment);
    navigate = vi.fn((_commands: unknown[], extras: NavigationExtras) => {
      fragment.next(extras.fragment ?? null);
      return Promise.resolve(true);
    });
    selection = signal<SheetSelection | null>(null);
    sheet = {
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
      tables: [fixtureTable()],
      availableTemplates: [],
      linkedSources: [],
    };
    const index = computed(() => buildSheetIndex(sheet));
    TestBed.configureTestingModule({
      providers: [
        SheetAnchorService,
        {
          provide: ActivatedRoute,
          useValue: { fragment, snapshot: { fragment: initialFragment } },
        },
        { provide: Router, useValue: { navigate } },
        {
          provide: SheetStore,
          useValue: {
            index,
            selection,
            select: (next: SheetSelection) => selection.set(next),
            selectSection: (sectionId: number) =>
              selection.set({ tableId: 1, sectionId, rowId: null }),
            selectRow: (rowId: number) =>
              selection.set({
                tableId: 1,
                sectionId: index().rowSection.get(rowId) ?? null,
                rowId,
              }),
          },
        },
      ],
    });
    anchors = TestBed.inject(SheetAnchorService);
    TestBed.tick();
  }

  it('selects the row a link names', () => {
    setUp(null);
    const linked = sheet.tables[0].sections[0].rows[0];
    fragment.next(anchorKey('row', linked.publicId));
    TestBed.tick();

    expect(selection()?.rowId).toBe(linked.id);
  });

  it("selects a linked cell's row and keeps the link on the cell", () => {
    setUp(null);
    const row = sheet.tables[0].sections[0].rows[0];
    fragment.next(anchorKey('cell', row.cells[0].publicId));
    TestBed.tick();

    expect(selection()?.rowId).toBe(row.id);
    expect(navigate).not.toHaveBeenCalled();
  });

  it('writes what the user selects into the address, in place of the last entry', () => {
    setUp(null);
    const section = sheet.tables[0].sections[0];
    selection.set({ tableId: 1, sectionId: section.id, rowId: null });
    TestBed.tick();

    expect(navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({
        fragment: anchorKey('section', section.publicId),
        replaceUrl: true,
      }),
    );
  });

  it('scrolls to the linked element once it is on screen, and again when shown a second time', () => {
    setUp(null);
    const row = sheet.tables[0].sections[0].rows[0];
    const key = anchorKey('row', row.publicId);
    const element = document.createElement('div');
    element.scrollIntoView = vi.fn();

    fragment.next(key);
    TestBed.tick();
    anchors.register(key, element);
    TestBed.tick();
    expect(element.scrollIntoView).toHaveBeenCalledTimes(1);

    anchors.show(key);
    TestBed.tick();
    expect(element.scrollIntoView).toHaveBeenCalledTimes(2);
  });
});
