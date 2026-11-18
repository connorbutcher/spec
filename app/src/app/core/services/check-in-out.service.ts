import { Injectable, signal } from '@angular/core';
import {
  HttpTransportType,
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  RetryContext,
} from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ConnectedUser } from '../models/check-in-out/connected-user';
import { SheetChangedNotice } from '../models/check-in-out/sheet-changed-notice';
import { SheetPresence } from '../models/check-in-out/sheet-presence';

function reconnectPolicy(retryContext: RetryContext): number | null {
  switch (retryContext.previousRetryCount) {
    case 0:
      return 0; // 0 seconds
    case 1:
      return 2000; // 2 seconds
    case 2:
      return 10000; // 10 seconds
  }

  // Retry every 20 seconds for an additional hour (20seconds * 180 = 1 hour)
  if (retryContext.previousRetryCount < 180) {
    return 20000;
  }

  // Retry every minute for an additional 23 hours (60 seconds * 23 * 60 = 1 hour)
  if (retryContext.previousRetryCount < 23 * 60 * 60) {
    return 60000;
  }

  // Just give up...
  return null;
}

/**
 * The connection to the check-in/out service. Rows and tables are checked out in the API (each draft belongs to the
 * user who made it), so this only says who has each sheet open and tells the others viewing a sheet when someone
 * saves, undoes, submits or decides changes on it, so they read it again.
 */
@Injectable({
  providedIn: 'root',
})
export class CheckInOutService {
  public readonly isServiceConnected = signal<boolean>(false);
  /** Who has each sheet open, by sheet id. */
  public readonly presences = signal<Map<number, ConnectedUser[]>>(
    new Map<number, ConnectedUser[]>(),
  );
  public readonly sheetChanged$ = new Subject<SheetChangedNotice>();

  private _hubConnection!: HubConnection;
  private readonly _joinedSheetIds = new Set<number>();

  /**
   * Start the signalR connection.
   */
  public async startConnectionAsync(
    username: string,
    uid: string,
  ): Promise<void> {
    if (this._hubConnection) {
      return;
    }

    this._hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.signalRUrl}/${environment.signalRHubName}`, {
        transport: HttpTransportType.WebSockets,
        withCredentials: true,
        accessTokenFactory() {
          return `${username}:${uid}`;
        },
      })
      .withAutomaticReconnect({ nextRetryDelayInMilliseconds: reconnectPolicy })
      .build();

    this.setupSubscriptions();

    try {
      await this._hubConnection.start();

      this.handleCompleteDisconnect();
      this.handleReconnection();
    } catch (err) {
      return;
    }

    this.isServiceConnected.set(true);
  }

  /**
   * Disconnect the signalR connection.
   */
  public async stopConnectionAsync(): Promise<void> {
    if (!this._hubConnection) {
      return;
    }

    await this._hubConnection.stop();
    this.isServiceConnected.set(false);
  }

  //#region Presence

  /** Tells the others that this user has the sheet open. */
  public async joinSheetAsync(sheetId: number): Promise<void> {
    this._joinedSheetIds.add(sheetId);
    if (!this.isHubConnected) {
      return;
    }

    try {
      await this._hubConnection.invoke('JoinSheetAsync', sheetId);
    } catch (error) {
      console.error('Error invoking JoinSheet:', error);
    }
  }

  /** Tells the others that this user has closed the sheet. */
  public async leaveSheetAsync(sheetId: number): Promise<void> {
    this._joinedSheetIds.delete(sheetId);
    if (!this.isHubConnected) {
      return;
    }

    try {
      await this._hubConnection.invoke('LeaveSheetAsync', sheetId);
    } catch (error) {
      console.error('Error invoking LeaveSheet:', error);
    }
  }

  /** Who has the sheet open, this user included. */
  public viewersOf(sheetId: number): ConnectedUser[] {
    return this.presences().get(sheetId) ?? [];
  }

  //#endregion Presence

  //#region Changes

  /** Tells the others viewing the sheet that this user changed what is checked out on it. */
  public async notifySheetChangedAsync(
    sheetId: number,
    change: string,
  ): Promise<void> {
    if (!this.isHubConnected) {
      return;
    }

    try {
      await this._hubConnection.invoke('SheetChangedAsync', sheetId, change);
    } catch (error) {
      console.error('Error invoking SheetChanged:', error);
    }
  }

  //#endregion Changes

  public get currentConnectionId(): string {
    return this._hubConnection?.connectionId ?? '';
  }

  //#region Subscriptions

  private setupSubscriptions(): void {
    this._hubConnection.on('SheetPresence', (presence: SheetPresence) => {
      const presences = new Map(this.presences());
      presences.set(presence.sheetId, presence.users);
      this.presences.set(presences);
    });

    this._hubConnection.on('SheetChanged', (notice: SheetChangedNotice) => {
      this.sheetChanged$.next(notice);
    });
  }

  //#endregion Subscriptions

  //#region Disconnection Handling

  private handleCompleteDisconnect() {
    this._hubConnection.onclose(async () => {
      this.isServiceConnected.set(false);
      setTimeout(async () => {
        try {
          await this._hubConnection.start();
          this.isServiceConnected.set(true);
          await this.rejoinSheetsAsync();
        } catch (err) {}
      }, 5000);
    });
  }

  private handleReconnection() {
    this._hubConnection.onreconnected(async () => {
      this.isServiceConnected.set(true);
      await this.rejoinSheetsAsync();
    });
  }

  // A new connection is in no sheet's group, so the sheets open here are joined again.
  private async rejoinSheetsAsync(): Promise<void> {
    for (const sheetId of this._joinedSheetIds) {
      await this.joinSheetAsync(sheetId);
    }
  }

  //#endregion Disconnection Handling

  private get isHubConnected(): boolean {
    return (
      this._hubConnection &&
      this._hubConnection.state === HubConnectionState.Connected
    );
  }
}
