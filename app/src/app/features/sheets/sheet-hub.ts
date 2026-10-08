import { inject, Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { DEVELOPER_USER, DEVELOPER_USER_QUERY_PARAMETER } from '../../core/auth/developer-user';
import { SheetConnectionState } from './models/sheet-connection-state';
import { SheetHubHandlers } from './models/sheet-hub-handlers.model';
import { SheetLiveState } from './models/sheet-live-state.model';
import { SheetConnectionId } from './sheet-connection-id';
import { SHEET_HUB } from './sheet-hub-messages';

const FIRST_RETRY_MS = 1000;
const LONGEST_RETRY_MS = 15000;

/**
 * The live (SignalR) connection the sheet screen keeps to the server while it is open. The server uses
 * it to know who has which sheet open and to say when something changed; every change itself still goes
 * through the HTTP API. It keeps trying to connect for as long as the screen is open.
 *
 * This is the only file that knows about SignalR. `SheetLiveStore` decides what to do with what arrives.
 */
@Injectable()
export class SheetHub {
  public readonly state = signal<SheetConnectionState>('disconnected');

  private readonly connectionId = inject(SheetConnectionId);
  private connection: HubConnection | null = null;
  private retryTimer: ReturnType<typeof setTimeout> | null = null;
  private stopped = false;

  /** Opens the connection and listens for what the server says. Called once. */
  public start(handlers: SheetHubHandlers): void {
    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl())
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retry) => retryDelay(retry.previousRetryCount),
      })
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on(SHEET_HUB.presenceChanged, handlers.presenceChanged);
    connection.on(SHEET_HUB.sheetChanged, handlers.sheetChanged);
    connection.on(SHEET_HUB.takeoverChanged, handlers.takeoverChanged);
    connection.onreconnecting(() => this.setState('reconnecting'));
    connection.onreconnected(() => this.setState('connected'));
    connection.onclose(() => this.setState('disconnected'));

    this.connection = connection;
    void this.connect(0);
  }

  /** Tells the server which sheet this tab is looking at, and gets back who else is. */
  public join(sheetId: number): Promise<SheetLiveState> {
    return this.started().invoke<SheetLiveState>(SHEET_HUB.joinSheet, sheetId);
  }

  public async stop(): Promise<void> {
    this.stopped = true;
    if (this.retryTimer !== null) {
      clearTimeout(this.retryTimer);
    }
    await this.connection?.stop();
  }

  private started(): HubConnection {
    if (this.connection === null) {
      throw new Error('The live connection has not been started.');
    }
    return this.connection;
  }

  /**
   * SignalR reconnects a connection that drops, but gives up on one that never opened, so a server
   * that is down when the screen opens is retried here.
   */
  private async connect(attempt: number): Promise<void> {
    if (this.stopped) {
      return;
    }
    this.setState('connecting');
    try {
      await this.started().start();
      this.setState('connected');
    } catch {
      this.setState('disconnected');
      this.retryTimer = setTimeout(() => void this.connect(attempt + 1), retryDelay(attempt));
    }
  }

  /** The connection's id is only good while connected; every reconnection gets a new one. */
  private setState(state: SheetConnectionState): void {
    this.connectionId.value.set(
      state === 'connected' ? (this.connection?.connectionId ?? null) : null,
    );
    this.state.set(state);
  }
}

/** A WebSocket can't send headers, so the development-only user switch goes in the address. */
function hubUrl(): string {
  return DEVELOPER_USER === null
    ? SHEET_HUB.url
    : `${SHEET_HUB.url}?${DEVELOPER_USER_QUERY_PARAMETER}=${encodeURIComponent(DEVELOPER_USER)}`;
}

/** 1s, 2s, 4s, 8s, then every 15s. */
function retryDelay(previousRetries: number): number {
  return Math.min(FIRST_RETRY_MS * 2 ** previousRetries, LONGEST_RETRY_MS);
}
