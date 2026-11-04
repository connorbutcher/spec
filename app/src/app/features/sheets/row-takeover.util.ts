import { RowTakeover } from './models/row-takeover.model';

export class RowTakeoverUtil {
  /** Whole seconds from `nowMs` until an unanswered request is granted; never less than zero. */
  public static secondsUntilGranted(takeover: RowTakeover, nowMs: number): number {
    return Math.max(0, Math.ceil((Date.parse(takeover.expiresAtUtc) - nowMs) / 1000));
  }

  /** The list with the request added, or put in place of the copy of it that was there. */
  public static withTakeover(list: RowTakeover[], takeover: RowTakeover): RowTakeover[] {
    return [...RowTakeoverUtil.withoutTakeover(list, takeover.id), takeover];
  }

  public static withoutTakeover(list: RowTakeover[], takeoverId: string): RowTakeover[] {
    return list.filter((takeover) => takeover.id !== takeoverId);
  }
}
