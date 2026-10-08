import { RowTakeover } from './models/row-takeover.model';

/** Whole seconds from `nowMs` until an unanswered request is granted; never less than zero. */
export function secondsUntilGranted(takeover: RowTakeover, nowMs: number): number {
  return Math.max(0, Math.ceil((Date.parse(takeover.expiresAtUtc) - nowMs) / 1000));
}

/** The list with the request added, or put in place of the copy of it that was there. */
export function withTakeover(list: RowTakeover[], takeover: RowTakeover): RowTakeover[] {
  return [...withoutTakeover(list, takeover.id), takeover];
}

export function withoutTakeover(list: RowTakeover[], takeoverId: string): RowTakeover[] {
  return list.filter((takeover) => takeover.id !== takeoverId);
}
