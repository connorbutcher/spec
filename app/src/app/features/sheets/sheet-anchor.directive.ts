import { computed, Directive, effect, ElementRef, inject, input } from '@angular/core';
import { SheetAnchorKind } from './models/sheet-anchor-kind';
import { SheetAnchorService } from './sheet-anchor.service';
import { anchorKey, anchorPublicId } from './sheet-anchor.util';
import { SheetSearchStore } from './sheet-search.store';
import { SheetStore } from './sheet.store';

/**
 * Makes an element the place a link to a table, section, row or cell scrolls to:
 * `<header appSheetAnchor="table" [anchorId]="table.id">`. The element is registered with
 * `SheetAnchorService` by its key (`row-<publicId>`), is marked while the address points at it, and
 * shows whether the sheet's search found it, since search matches go by the same keys.
 *
 * Without the sheet page's services (a component tested on its own) it does nothing.
 */
@Directive({
  selector: '[appSheetAnchor]',
  host: {
    '[attr.data-anchor]': 'key()',
    '[class.anchor-target]': 'isTarget()',
    '[class.search-match]': 'isSearchMatch()',
    '[class.search-active]': 'isSearchActive()',
  },
})
export class SheetAnchor {
  public readonly appSheetAnchor = input.required<SheetAnchorKind>();
  /** The sheet id of the table, section, row or cell. */
  public readonly anchorId = input.required<number>();

  /** The key it goes by, from its public id, or null while it isn't on the sheet. */
  public readonly key = computed(() => {
    const kind = this.appSheetAnchor();
    const publicId = anchorPublicId(this.store.index(), kind, this.anchorId());
    return publicId === null ? null : anchorKey(kind, publicId);
  });

  public readonly isTarget = computed(() => {
    const key = this.key();
    return key !== null && this.anchors?.targetKey() === key;
  });

  public readonly isSearchMatch = computed(() => {
    const key = this.key();
    return key !== null && this.search?.isMatch(key) === true;
  });

  public readonly isSearchActive = computed(() => {
    const key = this.key();
    return key !== null && this.search?.isActive(key) === true;
  });

  private readonly store = inject(SheetStore);
  private readonly anchors = inject(SheetAnchorService, { optional: true });
  private readonly search = inject(SheetSearchStore, { optional: true });
  private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);

  constructor() {
    effect((onCleanup) => {
      const key = this.key();
      const anchors = this.anchors;
      if (key === null || anchors === null) {
        return;
      }
      const element = this.element.nativeElement;
      anchors.register(key, element);
      onCleanup(() => anchors.unregister(key, element));
    });
  }
}
