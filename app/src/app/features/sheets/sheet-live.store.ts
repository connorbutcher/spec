import { computed, DestroyRef, effect, inject, Injectable, signal, untracked } from '@angular/core';
import { CurrentUserStore } from '../../core/auth/current-user.store';
import { SheetConnectionState } from './models/sheet-connection-state';
import { SheetPresenceUser } from './models/sheet-presence-user.model';
import { RowTakeoverStore } from './row-takeover.store';
import { SheetHub } from './sheet-hub';
import { SheetStore } from './sheet.store';

/**
 * The live side of the open sheet: who else has it open, and hearing that it changed. Provided by the
 * sheet page beside `SheetStore`.
 *
 * The server is the only record of who a row is checked out to; that arrives in the sheet itself
 * (`row.lock`). This store hears over the live connection that the sheet changed and asks `SheetStore`
 * to read it again, so a checkout made in one browser shows in the others without a refresh. Takeover
 * requests that arrive the same way are handed to `RowTakeoverStore`.
 */
@Injectable()
export class SheetLiveStore {
  /** Everyone who has the sheet open, the viewer included. */
  public readonly users = signal<SheetPresenceUser[]>([]);

  /** Whether changes are arriving as they happen. */
  public readonly connectionState = computed<SheetConnectionState>(() => this.hub.state());

  public readonly viewerId = computed(() => this.currentUser.user()?.id ?? null);

  private readonly hub = inject(SheetHub);
  private readonly sheetStore = inject(SheetStore);
  private readonly takeovers = inject(RowTakeoverStore);
  private readonly currentUser = inject(CurrentUserStore);

  private readonly sheetId = computed(() => this.sheetStore.sheet()?.id ?? null);
  /** The sheet last joined, to tell a first join from a rejoin after the connection dropped. */
  private joinedSheetId: number | null = null;
  /** Counts joins, so the answer to one that has been overtaken by another is ignored. */
  private joinSequence = 0;

  constructor() {
    this.hub.start({
      presenceChanged: (users) => this.users.set(users),
      sheetChanged: () => this.sheetStore.refresh(),
      takeoverChanged: (takeover) => this.takeovers.apply(takeover),
    });

    // Joins whichever sheet is open, again after every reconnection: the server forgets a dropped connection.
    effect(() => {
      const sheetId = this.sheetId();
      const connected = this.hub.state() === 'connected';
      untracked(() => void this.join(connected ? sheetId : null));
    });

    inject(DestroyRef).onDestroy(() => void this.hub.stop());
  }

  /** Joins a sheet and takes in who is there; with no sheet, or no connection, nobody is. */
  private async join(sheetId: number | null): Promise<void> {
    const sequence = ++this.joinSequence;
    if (sheetId === null) {
      this.users.set([]);
      this.takeovers.reset([]);
      return;
    }

    try {
      const state = await this.hub.join(sheetId);
      if (sequence !== this.joinSequence) {
        return;
      }
      this.users.set(state.users);
      this.takeovers.reset(state.takeovers);

      // Joining the same sheet again means the connection dropped, and changes may have been missed.
      if (this.joinedSheetId === sheetId) {
        this.sheetStore.refresh();
      }
      this.joinedSheetId = sheetId;
    } catch {
      // The connection dropped mid-join; the effect joins again when it is back.
    }
  }
}
