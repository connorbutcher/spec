import { computed, inject, Injectable, signal } from '@angular/core';
import { CurrentUserStore } from '../../core/auth/current-user.store';
import { reuseUnchanged } from '../../shared/reuse-unchanged.util';
import { RowCheckout } from './models/row-checkout.model';
import { SheetLock } from './models/sheet-lock.model';

/**
 * The rows people have clicked into without changing them yet, as the server last listed them. Provided
 * by the sheet page. `SheetLiveStore` fills it from the live connection; `SheetStore` reads `locks` and
 * shows each of those rows as locked, exactly as it shows a row someone has changed.
 *
 * It depends on neither of them, which is what lets both use it.
 */
@Injectable()
export class RowCheckoutStore {
  /** The lock each checked-out row shows, by row id. */
  public readonly locks = computed<ReadonlyMap<number, SheetLock>>(() => {
    const me = this.currentUser.user()?.id ?? null;
    return new Map(
      this.checkouts().map((checkout) => [
        checkout.rowId,
        { userId: checkout.userId, userName: checkout.userName, isMine: checkout.userId === me },
      ]),
    );
  });

  private readonly currentUser = inject(CurrentUserStore);
  private readonly checkouts = signal<RowCheckout[]>([]);

  /**
   * Replaces the list with what the server now says; an empty list when there is no live connection.
   * A list that says the same as the last one changes nothing, so the sheet is not re-made for it.
   */
  public set(checkouts: RowCheckout[]): void {
    this.checkouts.update((current) => reuseUnchanged(current, checkouts));
  }
}
