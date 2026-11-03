import {
  Component,
  computed,
  effect,
  ElementRef,
  inject,
  untracked,
  viewChild,
} from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { SheetAnchorService } from '../sheet-anchor.service';
import { SheetSearchStore } from '../sheet-search.store';

/**
 * The search box on the sheet's top line. It finds every table heading and cell that shows what is
 * typed, ignoring case, says which of them is the current one ("3 of 12"), and steps through them with
 * Enter and Shift+Enter or the arrows. The current one goes into the address's fragment, which selects
 * it and scrolls it into view (`SheetAnchorService`), so the address links to it. Ctrl+F (Cmd+F) moves into it
 * in place of the browser's own find, and Escape clears it and leaves.
 */
@Component({
  selector: 'app-sheet-search',
  imports: [ButtonModule, IconFieldModule, InputIconModule, InputTextModule],
  templateUrl: './sheet-search.html',
  styleUrl: './sheet-search.scss',
  host: {
    '(document:keydown.control.f)': 'focus($event)',
    '(document:keydown.meta.f)': 'focus($event)',
  },
})
export class SheetSearch {
  public readonly query = computed(() => this.searchStore.query());
  public readonly hasQuery = computed(() => this.searchStore.hasQuery());
  public readonly hasMatches = computed(() => this.searchStore.matches().length > 0);
  public readonly countLabel = computed(() => this.searchStore.countLabel());

  private readonly searchStore = inject(SheetSearchStore);
  private readonly anchors = inject(SheetAnchorService);
  private readonly box = viewChild.required<ElementRef<HTMLInputElement>>('box');

  constructor() {
    // Whenever the current match changes (a new search, or stepping), point the address at it.
    effect(() => {
      const key = this.searchStore.activeKey();
      if (key !== null) {
        untracked(() => this.anchors.show(key));
      }
    });
  }

  public search(query: string): void {
    this.searchStore.query.set(query);
  }

  /** Steps through the matches. The current one is shown again even when it is the only one. */
  public go(step: 1 | -1, event?: Event): void {
    event?.preventDefault();
    this.searchStore.step(step);
    const key = this.searchStore.activeKey();
    if (key !== null) {
      this.anchors.show(key);
    }
  }

  public focus(event: Event): void {
    event.preventDefault();
    const box = this.box().nativeElement;
    box.focus();
    box.select();
  }

  public clear(box: HTMLInputElement): void {
    this.searchStore.clear();
    box.focus();
  }

  public leave(box: HTMLInputElement): void {
    this.searchStore.clear();
    box.blur();
  }
}
