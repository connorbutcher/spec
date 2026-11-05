import { Injectable, signal } from '@angular/core';

/**
 * Which settings groups are folded, by name, so a group stays folded as you move from one cell (or
 * section) to the next. Provided by the templates page; forgotten when you leave it.
 */
@Injectable()
export class SettingsGroupState {
  private readonly folded = signal<ReadonlySet<string>>(new Set());

  public isFolded(key: string): boolean {
    return this.folded().has(key);
  }

  public setFolded(key: string, folded: boolean): void {
    this.folded.update((keys) => {
      const next = new Set(keys);
      if (folded) {
        next.add(key);
      } else {
        next.delete(key);
      }
      return next;
    });
  }
}
