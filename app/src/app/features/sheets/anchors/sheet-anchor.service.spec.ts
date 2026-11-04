import { computed, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, NavigationExtras, Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { SheetSelection } from '../models/sheet-selection.model';
import { Sheet } from '../models/sheet.model';
import { SheetAnchorService } from './sheet-anchor.service';
import { SheetAnchorUtil } from './sheet-anchor.util';
import { SheetIndexUtil } from '../layout/sheet-index.util';
import { SheetStructureFixture } from '../data/sheet-structure.fixture';
import { SheetStore } from '../sheet.store';

class SheetAnchorServicePage {
  public fragment!: BehaviorSubject<string | null>;
  public navigate!: ReturnType<typeof vi.fn>;
  public selection!: ReturnType<typeof signal<SheetSelection | null>>;
  public sheet!: Sheet;
  public anchors!: SheetAnchorService;

  public setUp(initialFragment: string | null): void {
    this.fragment = new BehaviorSubject<string | null>(initialFragment);
    this.navigate = vi.fn((_commands: unknown[], extras: NavigationExtras) => {
      this.fragment.next(extras.fragment ?? null);
      return Promise.resolve(true);
    });
    this.selection = signal<SheetSelection | null>(null);
    this.sheet = {
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
      tables: [SheetStructureFixture.fixtureTable()],
      availableTemplates: [],
      linkedSources: [],
    };
    const index = computed(() => SheetIndexUtil.buildSheetIndex(this.sheet));
    TestBed.configureTestingModule({
      providers: [
        SheetAnchorService,
        {
          provide: ActivatedRoute,
          useValue: { fragment: this.fragment, snapshot: { fragment: initialFragment } },
        },
        { provide: Router, useValue: { navigate: this.navigate } },
        {
          provide: SheetStore,
          useValue: {
            index,
            selection: this.selection,
            select: (next: SheetSelection) => this.selection.set(next),
            selectSection: (sectionId: number) =>
              this.selection.set({ tableId: 1, sectionId, rowId: null }),
            selectRow: (rowId: number) =>
              this.selection.set({
                tableId: 1,
                sectionId: index().rowSection.get(rowId) ?? null,
                rowId,
              }),
          },
        },
      ],
    });
    this.anchors = TestBed.inject(SheetAnchorService);
    TestBed.tick();
  }
}

describe('SheetAnchorService', () => {
  let page: SheetAnchorServicePage;

  beforeEach(() => {
    page = new SheetAnchorServicePage();
  });

  it('selects the row a link names', () => {
    page.setUp(null);
    const linked = page.sheet.tables[0].sections[0].rows[0];
    page.fragment.next(SheetAnchorUtil.anchorKey('row', linked.publicId));
    TestBed.tick();

    expect(page.selection()?.rowId).toBe(linked.id);
  });

  it("selects a linked cell's row and keeps the link on the cell", () => {
    page.setUp(null);
    const row = page.sheet.tables[0].sections[0].rows[0];
    page.fragment.next(SheetAnchorUtil.anchorKey('cell', row.cells[0].publicId));
    TestBed.tick();

    expect(page.selection()?.rowId).toBe(row.id);
    expect(page.navigate).not.toHaveBeenCalled();
  });

  it('writes what the user selects into the address, in place of the last entry', () => {
    page.setUp(null);
    const section = page.sheet.tables[0].sections[0];
    page.selection.set({ tableId: 1, sectionId: section.id, rowId: null });
    TestBed.tick();

    expect(page.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({
        fragment: SheetAnchorUtil.anchorKey('section', section.publicId),
        replaceUrl: true,
      }),
    );
  });

  it('scrolls to the linked element once it is on screen, and again when shown a second time', () => {
    page.setUp(null);
    const row = page.sheet.tables[0].sections[0].rows[0];
    const key = SheetAnchorUtil.anchorKey('row', row.publicId);
    const element = document.createElement('div');
    element.scrollIntoView = vi.fn();

    page.fragment.next(key);
    TestBed.tick();
    page.anchors.register(key, element);
    TestBed.tick();
    expect(element.scrollIntoView).toHaveBeenCalledTimes(1);

    page.anchors.show(key);
    TestBed.tick();
    expect(element.scrollIntoView).toHaveBeenCalledTimes(2);
  });
});
