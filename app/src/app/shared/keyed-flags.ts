import { Signal, signal, WritableSignal } from '@angular/core';

/**
 * One key out of many that is "on" (the selected row, the cell being edited), as a flag per key. Each
 * thing on screen reads only its own key's flag, so moving the key from one to another changes two
 * flags, and only those two readers react, however many there are.
 *
 * Comparing every reader with one shared signal would give the same answers, but each move would make
 * every reader on the sheet work its answer out again.
 */
export class KeyedFlags {
  private readonly flags = new Map<number, WritableSignal<boolean>>();
  private current: number | null = null;

  public flag(key: number): Signal<boolean> {
    let flag = this.flags.get(key);
    if (flag === undefined) {
      flag = signal(key === this.current);
      this.flags.set(key, flag);
    }
    return flag.asReadonly();
  }

  /** Turns `key` on, and whichever key was on before off. Null turns every key off. */
  public set(key: number | null): void {
    if (key === this.current) {
      return;
    }
    if (this.current !== null) {
      this.flags.get(this.current)?.set(false);
    }
    this.current = key;
    if (key !== null) {
      this.flags.get(key)?.set(true);
    }
  }
}
