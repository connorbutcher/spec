import { computed, DestroyRef, effect, inject, Injectable, signal, untracked } from '@angular/core';
import { CurrentUserStore } from '../../core/auth/current-user.store';
import { SheetConnectionState } from './models/sheet-connection-state';
import { SheetPresenceUser } from './models/sheet-presence-user.model';
import { RowCheckoutStore } from './row-checkout.store';
import { hubRefusalMessage, rowIdOfCell } from './row-checkout.util';
import { RowTakeoverStore } from './row-takeover.store';
import { SheetHub } from './sheet-hub';
import { SheetStore } from './sheet.store';

/** How long after the viewer leaves a row it is let go, unless they are back in it by then. */
const RELEASE_DELAY_MS = 150;

/**
 * The live side of the open sheet: who else has it open, hearing that it changed, and telling the
 * server which row the viewer is in. Provided by the sheet page beside `SheetStore`.
 *
 * A row belongs to someone in one of two ways. Once they have changed it, the sheet itself says so
 * (`row.lock`, from the saved draft); this store hears that the sheet changed and asks `SheetStore` to
 * read it again. While they are only in it, nothing is saved: the server lists those rows over the live
 * connection and this store passes the list to `RowCheckoutStore`. Takeover requests that arrive the
 * same way go to `RowTakeoverStore`.
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
  private readonly rowCheckouts = inject(RowCheckoutStore);
  private readonly takeovers = inject(RowTakeoverStore);
  private readonly currentUser = inject(CurrentUserStore);

  private readonly sheetId = computed(() => this.sheetStore.sheet()?.id ?? null);

  /** The row of the cell the viewer is in, which is the row to hold for them. */
  private readonly editingRowId = computed(() => {
    const cellId = this.sheetStore.editingCellId();
    return cellId === null ? null : rowIdOfCell(this.sheetStore.index(), cellId);
  });

  /** The sheet the server knows this tab is on; null until joined and whenever the connection is down. */
  private readonly joinedSheetId = signal<number | null>(null);
  /** The sheet last joined, to tell a first join from a rejoin after the connection dropped. */
  private lastJoinedSheetId: number | null = null;
  /** Counts joins, so the answer to one that has been overtaken by another is ignored. */
  private joinSequence = 0;
  /** The row the viewer is in now, which is the row the server should be holding. */
  private wantedRowId: number | null = null;
  /** The row the server is holding for this tab, as far as this tab knows. */
  private heldRowId: number | null = null;

  constructor() {
    this.hub.start({
      presenceChanged: (users) => this.users.set(users),
      checkoutsChanged: (checkouts) => this.rowCheckouts.set(checkouts),
      sheetChanged: () => this.sheetStore.refresh(),
      takeoverChanged: (takeover) => this.takeovers.apply(takeover),
    });

    // Joins whichever sheet is open, again after every reconnection: the server forgets a dropped connection.
    effect(() => {
      const sheetId = this.sheetId();
      const connected = this.hub.state() === 'connected';
      untracked(() => void this.join(connected ? sheetId : null));
    });

    // Holds the row the viewer is in, for as long as they are in it and the server knows the tab.
    effect(() => {
      const rowId = this.joinedSheetId() === null ? null : this.editingRowId();
      untracked(() => void this.hold(rowId));
    });

    inject(DestroyRef).onDestroy(() => void this.hub.stop());
  }

  /** Joins a sheet and takes in who is there; with no sheet, or no connection, nobody is. */
  private async join(sheetId: number | null): Promise<void> {
    const sequence = ++this.joinSequence;
    this.joinedSheetId.set(null);
    // Whatever row the server held went with the old connection or the old sheet.
    this.heldRowId = null;
    if (sheetId === null) {
      this.users.set([]);
      this.rowCheckouts.set([]);
      this.takeovers.reset([]);
      return;
    }

    try {
      const state = await this.hub.join(sheetId);
      if (sequence !== this.joinSequence) {
        return;
      }
      this.users.set(state.users);
      this.rowCheckouts.set(state.checkouts);
      this.takeovers.reset(state.takeovers);

      // Joining the same sheet again means the connection dropped, and changes may have been missed.
      if (this.lastJoinedSheetId === sheetId) {
        this.sheetStore.refresh();
      }
      this.lastJoinedSheetId = sheetId;
      this.joinedSheetId.set(sheetId);
    } catch {
      // The connection dropped mid-join; the effect joins again when it is back.
    }
  }

  /**
   * Asks the server to hold the row the viewer is in, or to let go when they are in none. Letting go
   * waits a moment first: moving from one cell to the next in a row leaves the first before entering
   * the second, and the row should not change hands in between.
   */
  private async hold(rowId: number | null): Promise<void> {
    this.wantedRowId = rowId;
    if (rowId === null) {
      await new Promise((resolve) => setTimeout(resolve, RELEASE_DELAY_MS));
    }
    const overtaken = this.wantedRowId !== rowId;
    if (overtaken || rowId === this.heldRowId || this.joinedSheetId() === null) {
      return;
    }

    this.heldRowId = rowId;
    try {
      await (rowId === null ? this.hub.release() : this.hub.checkOut(rowId));
    } catch (failure) {
      this.heldRowId = null;
      if (rowId !== null && this.wantedRowId === rowId) {
        this.refuse(hubRefusalMessage(failure));
      }
    }
  }

  /** Someone else got to the row first: say so, and take the viewer back out of its cell. */
  private refuse(reason: string): void {
    this.sheetStore.error.set(reason);
    const cellId = this.sheetStore.editingCellId();
    if (cellId !== null) {
      this.sheetStore.stopEditing(cellId);
    }
  }
}
