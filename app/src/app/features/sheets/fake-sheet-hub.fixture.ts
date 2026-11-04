import { signal } from '@angular/core';
import { SheetConnectionState } from './models/sheet-connection-state';
import { SheetHubHandlers } from './models/sheet-hub-handlers.model';
import { SheetLiveState } from './models/sheet-live-state.model';

/** Stands in for the SignalR connection: the test plays the server. */
export class FakeSheetHub {
  public readonly state = signal<SheetConnectionState>('disconnected');
  public handlers: SheetHubHandlers | null = null;
  public joined: number[] = [];
  public onJoin: SheetLiveState = { users: [], checkouts: [], takeovers: [] };
  /** Every row call made, in order: a row id for a checkout, 'release' for letting go. */
  public rowCalls: (number | 'release')[] = [];
  /** Set to make the next checkout fail the way the server refuses one. */
  public refusal: string | null = null;

  public start(handlers: SheetHubHandlers): void {
    this.handlers = handlers;
  }

  public join(sheetId: number): Promise<SheetLiveState> {
    this.joined.push(sheetId);
    return Promise.resolve(this.onJoin);
  }

  public checkOut(rowId: number): Promise<void> {
    this.rowCalls.push(rowId);
    return this.refusal === null
      ? Promise.resolve()
      : Promise.reject(new Error(`An unexpected error occurred. HubException: ${this.refusal}`));
  }

  public release(): Promise<void> {
    this.rowCalls.push('release');
    return Promise.resolve();
  }

  public stop(): Promise<void> {
    return Promise.resolve();
  }
}
