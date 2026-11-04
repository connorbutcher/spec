import {
  afterNextRender,
  computed,
  effect,
  inject,
  Injectable,
  Injector,
  untracked,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { SheetAnchorTarget } from '../models/sheet-anchor-target.model';
import { SheetAnchorUtil } from './sheet-anchor.util';
import { SheetStore } from '../sheet.store';

/**
 * Links into the open sheet. The address's fragment names one table, section, row or cell
 * (`#row-<publicId>`, see `parseAnchor`); opening such an address selects it and scrolls it into view
 * once the sheet has loaded and it is on screen. Selecting something on the sheet writes it into the
 * fragment in turn, so the address bar always links to what is selected, and the search writes the
 * current match there as it steps through them. Provided by the sheet page.
 *
 * Each element that can be linked to carries the `appSheetAnchor` directive, which registers it here
 * by its key.
 */
@Injectable()

export class SheetAnchorService {
  /** What the address points at, if it names a sheet anchor. */
  public readonly anchor = computed<SheetAnchorTarget | null>(() => SheetAnchorUtil.parseAnchor(this.fragment()));

  /** The key of what the address points at, matching the `appSheetAnchor` of the element showing it. */
  public readonly targetKey = computed(() => {
    const anchor = this.anchor();
    return anchor === null ? null : SheetAnchorUtil.anchorKey(anchor.kind, anchor.publicId);
  });

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(SheetStore);
  private readonly injector = inject(Injector);

  private readonly fragment = toSignal(this.route.fragment, {
    initialValue: this.route.snapshot.fragment,
  });

  /** The elements on screen that can be linked to, by key. */
  private readonly elements = new Map<string, HTMLElement>();

  /** A key the address points at whose element isn't on screen yet: it is scrolled to when it arrives. */
  private pending: string | null = null;

  /** A key written into the address for a selection the user made: it is already where they are looking. */
  private quiet: string | null = null;

  /** The key whose item has been selected, so a later refresh of the sheet doesn't select it again. */
  private selectedFor: string | null = null;

  constructor() {
    // A new address: scroll to what it points at, unless it came from the user's own selection.
    effect(() => {
      const key = this.targetKey();
      untracked(() => {
        if (key === null || key === this.quiet) {
          this.quiet = null;
          return;
        }
        this.reveal(key);
      });
    });

    // Select what the address points at, once it is on the sheet (a link can be opened before it loads).
    effect(() => {
      const anchor = this.anchor();
      const index = this.store.index();
      const key = this.targetKey();
      untracked(() => {
        if (anchor === null || key === this.selectedFor) {
          return;
        }
        const id = SheetAnchorUtil.anchorId(index, anchor);
        if (id === null) {
          return;
        }
        this.selectedFor = key;
        this.select(anchor, id);
      });
    });

    // Write what the user selects into the address, so it can be copied and shared.
    effect(() => {
      const selection = this.store.selection();
      untracked(() => {
        const anchor = SheetAnchorUtil.selectionAnchor(this.store.index(), selection);
        if (anchor === null || this.pointsWithin(selection?.rowId ?? null, anchor)) {
          return;
        }
        const key = SheetAnchorUtil.anchorKey(anchor.kind, anchor.publicId);
        this.quiet = key;
        this.selectedFor = key;
        this.navigate(key);
      });
    });
  }

  /**
   * Points the address at `key` and scrolls it into view. Going to the key the address already has
   * scrolls to it again, so the only match of a search can be brought back after scrolling away.
   */
  public show(key: string): void {
    if (key === this.targetKey()) {
      this.reveal(key);
    } else {
      this.navigate(key);
    }
  }

  public register(key: string, element: HTMLElement): void {
    this.elements.set(key, element);
    if (key === this.pending) {
      this.reveal(key);
    }
  }

  public unregister(key: string, element: HTMLElement): void {
    if (this.elements.get(key) === element) {
      this.elements.delete(key);
    }
  }

  private reveal(key: string): void {
    const element = this.elements.get(key);
    if (element === undefined) {
      this.pending = key;
      return;
    }
    this.pending = null;
    const isBlock = key.startsWith('table-') || key.startsWith('section-');
    afterNextRender(
      () => element.scrollIntoView({ block: isBlock ? 'start' : 'center', inline: 'nearest' }),
      { injector: this.injector },
    );
  }

  private navigate(key: string): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      fragment: key,
      queryParamsHandling: 'preserve',
      replaceUrl: true,
    });
  }

  private select(anchor: SheetAnchorTarget, id: number): void {
    switch (anchor.kind) {
      case 'table': {
        this.store.select({ tableId: id, sectionId: null, rowId: null });
        break;
      }
      case 'section': {
        this.store.selectSection(id);
        break;
      }
      case 'row': {
        this.store.selectRow(id);
        break;
      }
      case 'cell': {
        const rowId = SheetAnchorUtil.cellRowId(this.store.index(), id);
        if (rowId !== null) {
          this.store.selectRow(rowId);
        }
        break;
      }
    }
  }

  /**
   * Whether the address already points at the selection, or at a cell in the selected row: then a
   * link to the cell is kept rather than widened to its row.
   */
  private pointsWithin(rowId: number | null, selected: SheetAnchorTarget): boolean {
    const current = this.anchor();
    if (current === null) {
      return false;
    }
    if (current.kind === selected.kind && current.publicId === selected.publicId) {
      return true;
    }
    if (current.kind !== 'cell' || rowId === null) {
      return false;
    }
    const cellId = SheetAnchorUtil.anchorId(this.store.index(), current);
    return cellId !== null && SheetAnchorUtil.cellRowId(this.store.index(), cellId) === rowId;
  }
}
