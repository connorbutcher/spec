import { computed, inject, Injectable, linkedSignal, signal } from '@angular/core';
import { SheetSearchTarget } from '../models/sheet-search-target.model';
import { SheetSearchUtil } from './sheet-search.util';
import { SheetStore } from '../sheet.store';

/**
 * The search on the open sheet: what is being looked for, everything on the sheet that holds it, in the
 * order it is on screen, and which of those is the current one. It searches the sheet as it is loaded,
 * so it follows the view (live or a past version) and every refresh. Provided by the sheet page.
 *
 * The grid cells and table headings read `isMatch` and `isActive` by their `data-search-key`, and
 * `SheetSearch` scrolls the current one into view.
 */
@Injectable()

export class SheetSearchStore {
  public readonly query = signal('');

  /** The keys of everything that holds the query, in screen order. Nothing is worked out while it is blank. */
  public readonly matches = computed<string[]>(
    () => (this.query().trim() === '' ? [] : SheetSearchUtil.matchingKeys(this.targets(), this.query())),
    { equal: SheetSearchStore.sameKeys },
  );

  public readonly matchSet = computed<ReadonlySet<string>>(() => new Set(this.matches()));

  /**
   * The current match. It stays where it is while it still matches, so typing more of a word or
   * someone else's change elsewhere on the sheet doesn't move it; otherwise it is the first match.
   */
  public readonly activeKey = linkedSignal<string[], string | null>({
    source: this.matches,
    computation: (matches, previous) =>
      previous !== undefined && previous.value !== null && matches.includes(previous.value)
        ? previous.value
        : (matches[0] ?? null),
  });

  public readonly activeIndex = computed(() => {
    const key = this.activeKey();
    return key === null ? -1 : this.matches().indexOf(key);
  });

  public readonly hasQuery = computed(() => this.query().trim() !== '');

  /** "3 of 12", or "No matches". */
  public readonly countLabel = computed(() =>
    SheetSearchUtil.matchCountLabel(this.activeIndex(), this.matches().length),
  );

  private readonly store = inject(SheetStore);

  private readonly targets = computed<SheetSearchTarget[]>(() =>
    SheetSearchUtil.searchTargets(this.store.sheet(), this.store.index(), this.store.cellTypes()),
  );
    private static sameKeys(a: string[], b: string[]): boolean {
      return a.length === b.length && a.every((key, i) => key === b[i]);
    }

  public isMatch(key: string): boolean {
    return this.matchSet().has(key);
  }

  public isActive(key: string): boolean {
    return this.activeKey() === key;
  }

  /** Moves to the next match (`step` 1) or the previous one (`step` -1), going round at either end. */
  public step(step: 1 | -1): void {
    const matches = this.matches();
    if (matches.length === 0) {
      return;
    }
    const from = this.activeIndex();
    const to = from === -1 ? 0 : (from + step + matches.length) % matches.length;
    this.activeKey.set(matches[to]);
  }

  public clear(): void {
    this.query.set('');
  }
}
