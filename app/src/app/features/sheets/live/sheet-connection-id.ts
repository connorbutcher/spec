import { Service, signal } from '@angular/core';

/**
 * The id of this tab's live connection to the open sheet, or null when there isn't one. Sent with every
 * change so the server doesn't tell this tab about a change it has just made itself.
 */
@Service()
export class SheetConnectionId {
  public readonly value = signal<string | null>(null);
}
