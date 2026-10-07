import { computed, DestroyRef, effect, inject, Injectable, signal, untracked } from '@angular/core';
import { MessageService } from 'primeng/api';
import { CurrentUserStore } from '../../core/auth/current-user.store';
import { apiErrorMessage } from '../templates/api-error-message';
import { RowTakeover } from './models/row-takeover.model';
import { SheetConnectionState } from './models/sheet-connection-state';
import { SheetPresenceUser } from './models/sheet-presence-user.model';
import { RowTakeoversApi } from './row-takeovers-api';
import { SheetHub } from './sheet-hub';
import { SheetStore } from './sheet.store';
import { takeoverNotice } from './takeover-notice.util';

const NOTICE_LIFE_MS = 8000;

/**
 * The multi-user side of the open sheet: who else has it open, and the takeover requests the viewer made
 * or has to answer. Provided by the sheet page beside `SheetStore`.
 *
 * The server is the only record of who a row is checked out to; that arrives in the sheet itself
 * (`row.lock`). This store hears over the live connection that the sheet changed and asks `SheetStore` to
 * read it again, so a checkout made in one browser shows in the others without a refresh.
 */
@Injectable()
export class SheetLiveStore {
  /** Everyone who has the sheet open, the viewer included. */
  public readonly users = signal<SheetPresenceUser[]>([]);

  /** Whether changes are arriving as they happen. */
  public readonly connectionState = computed<SheetConnectionState>(() => this.hub.state());

  /** Requests for rows checked out to the viewer, waiting for their answer, oldest first. */
  public readonly incoming = computed<RowTakeover[]>(() => {
    const me = this.viewerId();
    return this.pending().filter((takeover) => takeover.holderUserId === me);
  });

  /** The viewer's own requests still waiting for an answer, by the row they are for. */
  public readonly outgoingByRow = computed<ReadonlyMap<number, RowTakeover>>(() => {
    const me = this.viewerId();
    return new Map(
      this.pending()
        .filter((takeover) => takeover.requesterUserId === me)
        .map((takeover) => [takeover.rowId, takeover]),
    );
  });

  /** The current time, ticking once a second while a request is waiting, for its countdown. */
  public readonly now = signal(Date.now());

  public readonly viewerId = computed(() => this.currentUser.user()?.id ?? null);

  private readonly hub = inject(SheetHub);
  private readonly api = inject(RowTakeoversApi);
  private readonly sheetStore = inject(SheetStore);
  private readonly currentUser = inject(CurrentUserStore);
  private readonly messages = inject(MessageService);

  private readonly pending = signal<RowTakeover[]>([]);
  private readonly sheetId = computed(() => this.sheetStore.sheet()?.id ?? null);
  /** Requests already settled, so hearing of one twice (the API's answer and the live message) tells the user once. */
  private readonly settled = new Set<string>();
  private watchedSheetId: number | null = null;
  private watchSequence = 0;

  constructor() {
    this.hub.start({
      presenceChanged: (users) => this.users.set(users),
      sheetChanged: () => this.sheetStore.refresh(),
      takeoverChanged: (takeover) => this.apply(takeover),
    });

    // Watches whichever sheet is open, again after every reconnection: the server forgets a dropped connection.
    effect(() => {
      const sheetId = this.sheetId();
      const connected = this.hub.state() === 'connected';
      untracked(() => void this.watch(connected ? sheetId : null));
    });

    effect((onCleanup) => {
      if (this.pending().length === 0) {
        return;
      }
      this.now.set(Date.now());
      const timer = setInterval(() => this.now.set(Date.now()), 1000);
      onCleanup(() => clearInterval(timer));
    });

    inject(DestroyRef).onDestroy(() => void this.hub.stop());
  }

  /** Whole seconds until an unanswered request is granted. */
  public secondsLeft(takeover: RowTakeover): number {
    return Math.max(0, Math.ceil((Date.parse(takeover.expiresAtUtc) - this.now()) / 1000));
  }

  /** Asks whoever has the row checked out to hand it over. */
  public async request(rowId: number): Promise<void> {
    await this.send(() => this.api.request(rowId));
  }

  public async approve(takeover: RowTakeover): Promise<void> {
    await this.answer(takeover, () => this.api.approve(takeover.id));
  }

  public async deny(takeover: RowTakeover): Promise<void> {
    await this.answer(takeover, () => this.api.deny(takeover.id));
  }

  public async cancel(takeover: RowTakeover): Promise<void> {
    await this.answer(takeover, () => this.api.cancel(takeover.id));
  }

  private async watch(sheetId: number | null): Promise<void> {
    const sequence = ++this.watchSequence;
    if (sheetId === null) {
      this.users.set([]);
      this.pending.set([]);
      return;
    }

    try {
      const state = await this.hub.join(sheetId);
      if (sequence !== this.watchSequence) {
        return;
      }
      this.users.set(state.users);
      this.pending.set(state.takeovers);

      // Joining the same sheet again means the connection dropped, and changes may have been missed.
      if (this.watchedSheetId === sheetId) {
        this.sheetStore.refresh();
      }
      this.watchedSheetId = sheetId;
    } catch {
      // The connection dropped mid-join; the effect joins again when it is back.
    }
  }

  /** An answer to a request. If the request has gone by the time it is answered, it just comes off the list. */
  private async answer(takeover: RowTakeover, call: () => Promise<RowTakeover>): Promise<void> {
    if (!(await this.send(call))) {
      this.remove(takeover.id);
    }
  }

  private async send(call: () => Promise<RowTakeover>): Promise<boolean> {
    try {
      this.apply(await call());
      return true;
    } catch (failure) {
      this.sheetStore.error.set(apiErrorMessage(failure));
      return false;
    }
  }

  /** Takes in a request as the server now has it: a waiting one is listed, a settled one is announced. */
  private apply(takeover: RowTakeover): void {
    if (takeover.sheetId !== this.sheetId() || this.settled.has(takeover.id)) {
      return;
    }
    if (takeover.status === 'Pending') {
      this.pending.update((list) => [
        ...list.filter((existing) => existing.id !== takeover.id),
        takeover,
      ]);
      return;
    }

    this.settled.add(takeover.id);
    this.remove(takeover.id);
    this.sheetStore.refresh();

    const me = this.viewerId();
    const notice = me === null ? null : takeoverNotice(takeover, me);
    if (notice !== null) {
      this.messages.add({
        severity: notice.severity,
        summary: notice.summary,
        detail: notice.detail,
        sticky: notice.sticky,
        life: NOTICE_LIFE_MS,
      });
    }
  }

  private remove(takeoverId: string): void {
    this.pending.update((list) => list.filter((takeover) => takeover.id !== takeoverId));
  }
}
