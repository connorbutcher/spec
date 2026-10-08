import { computed, effect, inject, Injectable, signal } from '@angular/core';
import { MessageService } from 'primeng/api';
import { CurrentUserStore } from '../../core/auth/current-user.store';
import { apiErrorMessage } from '../templates/api-error-message';
import { RowTakeover } from './models/row-takeover.model';
import { secondsUntilGranted, withoutTakeover, withTakeover } from './row-takeover.util';
import { RowTakeoversApi } from './row-takeovers-api';
import { SheetStore } from './sheet.store';
import { takeoverNotice } from './takeover-notice.util';

const NOTICE_LIFE_MS = 8000;
const TICK_MS = 1000;

/**
 * The takeover requests on the open sheet that involve the viewer: the ones they made and the ones for
 * rows checked out to them. Provided by the sheet page.
 *
 * A request reaches this store twice over, as the API's answer to the viewer's own call and as a live
 * message from the server (`SheetLiveStore` passes those on); both go through `apply`, which lists a
 * waiting request and announces a settled one once.
 */
@Injectable()
export class RowTakeoverStore {
  /** Requests for rows checked out to the viewer, waiting for their answer, oldest first. */
  public readonly incoming = computed<RowTakeover[]>(() => {
    const me = this.viewerId();
    return this.waiting().filter((takeover) => takeover.holderUserId === me);
  });

  /** The viewer's own requests still waiting for an answer, by the row they are for. */
  public readonly outgoingByRow = computed<ReadonlyMap<number, RowTakeover>>(() => {
    const me = this.viewerId();
    return new Map(
      this.waiting()
        .filter((takeover) => takeover.requesterUserId === me)
        .map((takeover) => [takeover.rowId, takeover]),
    );
  });

  /** The current time, ticking once a second only while a request is waiting, for its countdown. */
  public readonly now = signal(Date.now());

  private readonly api = inject(RowTakeoversApi);
  private readonly sheetStore = inject(SheetStore);
  private readonly currentUser = inject(CurrentUserStore);
  private readonly messages = inject(MessageService);

  private readonly waiting = signal<RowTakeover[]>([]);
  private readonly viewerId = computed(() => this.currentUser.user()?.id ?? null);
  private readonly sheetId = computed(() => this.sheetStore.sheet()?.id ?? null);
  /** Requests already settled, so hearing of one twice tells the user once. */
  private readonly settled = new Set<string>();

  constructor() {
    effect((onCleanup) => {
      if (this.waiting().length === 0) {
        return;
      }
      this.now.set(Date.now());
      const timer = setInterval(() => this.now.set(Date.now()), TICK_MS);
      onCleanup(() => clearInterval(timer));
    });
  }

  /** Whole seconds until an unanswered request is granted. */
  public secondsLeft(takeover: RowTakeover): number {
    return secondsUntilGranted(takeover, this.now());
  }

  /** Starts again from what the server says is waiting, on joining a sheet or leaving it. */
  public reset(waiting: RowTakeover[]): void {
    this.waiting.set(waiting);
  }

  /** Takes in a request as the server now has it: a waiting one is listed, a settled one is announced. */
  public apply(takeover: RowTakeover): void {
    if (takeover.sheetId !== this.sheetId() || this.settled.has(takeover.id)) {
      return;
    }
    if (takeover.status === 'Pending') {
      this.waiting.update((list) => withTakeover(list, takeover));
      return;
    }

    this.settled.add(takeover.id);
    this.waiting.update((list) => withoutTakeover(list, takeover.id));
    this.sheetStore.refresh();
    this.announce(takeover);
  }

  /** Asks whoever has the row checked out to hand it over. */
  public async request(rowId: number): Promise<void> {
    await this.send(() => this.api.request(rowId));
  }

  public async approve(takeover: RowTakeover): Promise<void> {
    await this.settle(takeover, () => this.api.approve(takeover.id));
  }

  public async deny(takeover: RowTakeover): Promise<void> {
    await this.settle(takeover, () => this.api.deny(takeover.id));
  }

  public async cancel(takeover: RowTakeover): Promise<void> {
    await this.settle(takeover, () => this.api.cancel(takeover.id));
  }

  /** Answers or withdraws a request. If it had already gone by then, it just comes off the list. */
  private async settle(takeover: RowTakeover, call: () => Promise<RowTakeover>): Promise<void> {
    if (!(await this.send(call))) {
      this.waiting.update((list) => withoutTakeover(list, takeover.id));
    }
  }

  /** Makes a call and takes in its answer. A failure shows its reason where the sheet shows its own. */
  private async send(call: () => Promise<RowTakeover>): Promise<boolean> {
    try {
      this.apply(await call());
      return true;
    } catch (failure) {
      this.sheetStore.error.set(apiErrorMessage(failure));
      return false;
    }
  }

  private announce(takeover: RowTakeover): void {
    const me = this.viewerId();
    const notice = me === null ? null : takeoverNotice(takeover, me);
    if (notice !== null) {
      this.messages.add({ ...notice, life: NOTICE_LIFE_MS });
    }
  }
}
